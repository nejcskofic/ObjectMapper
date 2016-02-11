using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Formatting;

namespace ObjectMapper.ImplementInterfaceAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ObjectMapperImplementInterfaceAnalyzerCodeFixProvider)), Shared]
    public class ObjectMapperImplementInterfaceAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Generate implementation";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(ObjectMapperImplementInterfaceAnalyzerAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var baseTypeDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<SimpleBaseTypeSyntax>().FirstOrDefault();
            if (baseTypeDeclaration != null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedDocument: c => GenerateInterfaceImplementationAsync(context.Document, baseTypeDeclaration, c),
                        equivalenceKey: title),
                    diagnostic);
                return;
            }

            var methodDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (methodDeclaration != null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedDocument: c => GenerateMethodImplementationAsync(context.Document, methodDeclaration, c),
                        equivalenceKey: title),
                    diagnostic);
                return;
            }
        }

        private static async Task<Document> GenerateInterfaceImplementationAsync(Document document, SimpleBaseTypeSyntax baseTypeSyntax, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            SimpleNameSyntax sns = (baseTypeSyntax.Type as SimpleNameSyntax) ?? (baseTypeSyntax.Type as QualifiedNameSyntax).Right;
            var interfaceSymbol = semanticModel.GetSymbolInfo(sns).Symbol as INamedTypeSymbol;
            if (interfaceSymbol == null || interfaceSymbol.TypeKind != TypeKind.Interface) return document;

            var originalClassDefinitionSyntax = (ClassDeclarationSyntax)baseTypeSyntax.Parent.Parent;
            ClassDeclarationSyntax modifiedClassDefinitionSyntax = null;
            if (interfaceSymbol.Name == "IObjectMapper" && interfaceSymbol.TypeArguments.Length == 1)
            {
                var sourceClassSymbol = semanticModel.GetDeclaredSymbol(originalClassDefinitionSyntax);
                var targetClassSymbol = interfaceSymbol.TypeArguments[0].OriginalDefinition as INamedTypeSymbol;
                if (sourceClassSymbol == null || targetClassSymbol == null) return document;

                var matchedProperties = RetrieveMatchedProperties(sourceClassSymbol, targetClassSymbol);

                modifiedClassDefinitionSyntax = originalClassDefinitionSyntax;
                foreach (IMethodSymbol member in interfaceSymbol.GetMembers().Where(x => x.Kind == SymbolKind.Method))
                {
                    var method = sourceClassSymbol.FindImplementationForInterfaceMember(member) as IMethodSymbol;
                    MethodDeclarationSyntax methodSyntax = null;
                    if (method != null)
                    {
                        methodSyntax = await method.DeclaringSyntaxReferences[0].GetSyntaxAsync(cancellationToken) as MethodDeclarationSyntax;
                        var newMethodSyntax = methodSyntax.WithBody(GenerateMethodBody(member, matchedProperties, semanticModel, originalClassDefinitionSyntax.Span.End - 1));
                        modifiedClassDefinitionSyntax = modifiedClassDefinitionSyntax.ReplaceNode(methodSyntax, newMethodSyntax);
                    }
                    else
                    {
                        methodSyntax = GenerateMethodImplementation(member, semanticModel, originalClassDefinitionSyntax.Span.End - 1).
                            WithBody(GenerateMethodBody(member, matchedProperties, semanticModel, originalClassDefinitionSyntax.Span.End - 1));
                        modifiedClassDefinitionSyntax = modifiedClassDefinitionSyntax.AddMembers(methodSyntax);
                    }
                }
            } 
            else if (interfaceSymbol.Name == "IObjectMapperAdapter" && interfaceSymbol.TypeArguments.Length == 2)
            {
                var adapterClassSymbol = semanticModel.GetDeclaredSymbol(originalClassDefinitionSyntax);
                var sourceClassSymbol = interfaceSymbol.TypeArguments[0].OriginalDefinition as INamedTypeSymbol;
                var targetClassSymbol = interfaceSymbol.TypeArguments[1].OriginalDefinition as INamedTypeSymbol;
                if (sourceClassSymbol == null || targetClassSymbol == null) return document;

                var matchedProperties = RetrieveMatchedProperties(sourceClassSymbol, targetClassSymbol);

                modifiedClassDefinitionSyntax = originalClassDefinitionSyntax;
                foreach (IMethodSymbol member in interfaceSymbol.GetMembers().Where(x => x.Kind == SymbolKind.Method))
                {
                    var matchingPropertyList = matchedProperties;
                    // check if we have to switch matched properties
                    if (member.Parameters.Length == 2 && !interfaceSymbol.TypeArguments[0].Equals(member.Parameters[0].Type))
                    {
                        matchingPropertyList = matchingPropertyList.Select(x => new MatchedPropertySymbols { Source = x.Target, Target = x.Source });
                    }

                    var method = adapterClassSymbol.FindImplementationForInterfaceMember(member) as IMethodSymbol;
                    MethodDeclarationSyntax methodSyntax = null;
                    if (method != null)
                    {
                        methodSyntax = await method.DeclaringSyntaxReferences[0].GetSyntaxAsync(cancellationToken) as MethodDeclarationSyntax;
                        var newMethodSyntax = methodSyntax.WithBody(GenerateMethodBody(member, matchingPropertyList, semanticModel, originalClassDefinitionSyntax.Span.End - 1));
                        modifiedClassDefinitionSyntax = modifiedClassDefinitionSyntax.ReplaceNode(methodSyntax, newMethodSyntax);
                    }
                    else
                    {
                        methodSyntax = GenerateMethodImplementation(member, semanticModel, originalClassDefinitionSyntax.Span.End - 1).
                            WithBody(GenerateMethodBody(member, matchingPropertyList, semanticModel, originalClassDefinitionSyntax.Span.End - 1));
                        modifiedClassDefinitionSyntax = modifiedClassDefinitionSyntax.AddMembers(methodSyntax);
                    }
                }
            }
            else
            {
                return document;
            }


            // replace root and return modified document
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(originalClassDefinitionSyntax, modifiedClassDefinitionSyntax);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }

        private static async Task<Document> GenerateMethodImplementationAsync(Document document, MethodDeclarationSyntax methodSyntax, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            IMethodSymbol methodSymbol = semanticModel.GetDeclaredSymbol(methodSyntax);

            MethodDeclarationSyntax modifiedMethodSyntax = methodSyntax;
            if (methodSymbol.Parameters.Length == 1)
            {
                var sourceClassSymbol = methodSymbol.ContainingType;
                var targetClassSymbol = methodSymbol.Parameters[0].Type as INamedTypeSymbol;
                if (targetClassSymbol == null) return document;

                var matchedProperties = RetrieveMatchedProperties(sourceClassSymbol, targetClassSymbol);
                modifiedMethodSyntax = methodSyntax.WithBody(GenerateMethodBody(methodSymbol, matchedProperties, semanticModel, methodSyntax.Body.Span.End - 1));
            }
            else if (methodSymbol.Parameters.Length == 2)
            {
                var sourceClassSymbol = methodSymbol.Parameters[0].Type as INamedTypeSymbol;
                var targetClassSymbol = methodSymbol.Parameters[1].Type as INamedTypeSymbol;
                if (sourceClassSymbol == null || targetClassSymbol == null) return document;

                var matchedProperties = RetrieveMatchedProperties(sourceClassSymbol, targetClassSymbol);
                modifiedMethodSyntax = methodSyntax.WithBody(GenerateMethodBody(methodSymbol, matchedProperties, semanticModel, methodSyntax.Body.Span.End - 1));
            }

            // replace root and return modified document
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(methodSyntax, modifiedMethodSyntax);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }

        private static IEnumerable<MatchedPropertySymbols> RetrieveMatchedProperties(INamedTypeSymbol source, INamedTypeSymbol target)
        {
            SortedDictionary<string, MatchedPropertySymbols> propertiesMap = new SortedDictionary<string, MatchedPropertySymbols>();

            foreach (IPropertySymbol mSymbol in source.GetMembers().Where(x => x.Kind == SymbolKind.Property))
            {
                if (mSymbol.IsStatic || mSymbol.IsIndexer || mSymbol.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }
                if (!propertiesMap.ContainsKey(mSymbol.Name))
                {
                    // If class definition is invalid, it may happen that we get multiple properties with the same name
                    // Ignore all but first
                    propertiesMap.Add(mSymbol.Name, new MatchedPropertySymbols() { Source = mSymbol });
                }
            }

            foreach (IPropertySymbol mSymbol in target.GetMembers().Where(x => x.Kind == SymbolKind.Property))
            {
                if (mSymbol.IsStatic || mSymbol.IsIndexer || mSymbol.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }
                MatchedPropertySymbols sourceProperty = null;
                if (!propertiesMap.TryGetValue(mSymbol.Name, out sourceProperty))
                {
                    propertiesMap.Add(mSymbol.Name, new MatchedPropertySymbols { Target = mSymbol });
                }
                else if (sourceProperty.Target == null)
                {
                    // If class definition is invalid, it may happen that we get multiple properties with the same name
                    // Ignore all but first
                    sourceProperty.Target = mSymbol;
                }
            }

            return propertiesMap.Values;
        }

        private static BlockSyntax GenerateMethodBody(IMethodSymbol method, IEnumerable<MatchedPropertySymbols> matchedProperties, SemanticModel model, int position)
        {
            if (method.Name == "MapObject" && method.ReturnsVoid && method.Parameters.Length == 1)
            {
                return SyntaxFactory.Block(
                    SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
                    SyntaxFactory.List(GenerateAssignmentSyntax(SyntaxFactory.ThisExpression(), SyntaxFactory.IdentifierName(method.Parameters[0].Name), matchedProperties, model, position)),
                    SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
            }
            else if (method.Name == "MapObject" && method.ReturnsVoid && method.Parameters.Length == 2)
            {
                return SyntaxFactory.Block(
                    SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
                    SyntaxFactory.List(GenerateAssignmentSyntax(SyntaxFactory.IdentifierName(method.Parameters[0].Name), SyntaxFactory.IdentifierName(method.Parameters[1].Name), matchedProperties, model, position)),
                    SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
            }
            else
            {
                return SyntaxFactory.Block();
            }
        }

        private static IEnumerable<StatementSyntax> GenerateAssignmentSyntax(ExpressionSyntax source, ExpressionSyntax target, IEnumerable<MatchedPropertySymbols> matchedPropertySymbols, SemanticModel model, int position)
        {
            foreach (var matchedProperty in matchedPropertySymbols)
            {
                if (matchedProperty.Source == null || matchedProperty.Target == null) continue;
                if (matchedProperty.Source.GetMethod == null || matchedProperty.Source.GetMethod.DeclaredAccessibility != Accessibility.Public) continue;

                if (matchedProperty.Target.SetMethod != null && matchedProperty.Source.SetMethod.DeclaredAccessibility == Accessibility.Public &&
                    (matchedProperty.Source.Type.Equals(matchedProperty.Target.Type) || matchedProperty.Target.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T && (matchedProperty.Target.Type as INamedTypeSymbol).TypeArguments[0].Equals(matchedProperty.Source.Type)))
                {
                    yield return SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, target, SyntaxFactory.IdentifierName(matchedProperty.Target.Name)),
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, source, SyntaxFactory.IdentifierName(matchedProperty.Source.Name))));
                }
                else if (matchedProperty.Target.SetMethod != null && matchedProperty.Source.SetMethod.DeclaredAccessibility == Accessibility.Public &&
                    (matchedProperty.Source.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T && (matchedProperty.Source.Type as INamedTypeSymbol).TypeArguments[0].Equals(matchedProperty.Target.Type)))
                {
                    yield return SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, target, SyntaxFactory.IdentifierName(matchedProperty.Target.Name)),
                        SyntaxFactory.BinaryExpression(
                            SyntaxKind.CoalesceExpression,
                            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, source, SyntaxFactory.IdentifierName(matchedProperty.Source.Name)),
                            SyntaxFactory.DefaultExpression(SyntaxFactory.ParseTypeName(matchedProperty.Target.Type.ToMinimalDisplayString(model, position))))));
                }
                else if (matchedProperty.Target.Type.OriginalDefinition.AllInterfaces.Any(x => x.ToDisplayString() == "System.Collections.Generic.ICollection<T>") &&
                    matchedProperty.Target.GetMethod != null && matchedProperty.Target.GetMethod.DeclaredAccessibility == Accessibility.Public &&
                    matchedProperty.Source.Type.OriginalDefinition.AllInterfaces.Any(x => x.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>" &&
                    (matchedProperty.Target.Type as INamedTypeSymbol).TypeArguments[0].Equals((matchedProperty.Source.Type as INamedTypeSymbol).TypeArguments[0])))
                {
                    yield return SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, target, SyntaxFactory.IdentifierName(matchedProperty.Target.Name)),
                            SyntaxFactory.IdentifierName("CopyFrom"))).
                            WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(
                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, source, SyntaxFactory.IdentifierName(matchedProperty.Source.Name)))))));
                }
            }
        }

        private static MethodDeclarationSyntax GenerateMethodImplementation(IMethodSymbol fromMethod, SemanticModel model, int position)
        {
            MethodDeclarationSyntax syntax = SyntaxFactory.MethodDeclaration(
                fromMethod.ReturnsVoid ? SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)) : SyntaxFactory.ParseTypeName(fromMethod.ReturnType.ToMinimalDisplayString(model, position)),
                fromMethod.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithParameterList(SyntaxFactory.ParameterList(
                    SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                    SyntaxFactory.SeparatedList(fromMethod.Parameters.Select(x => SyntaxFactory.Parameter(SyntaxFactory.Identifier(x.Name)).WithType(SyntaxFactory.ParseTypeName(x.Type.ToMinimalDisplayString(model, position))))),
                    SyntaxFactory.Token(SyntaxKind.CloseParenToken)))
                .WithBody(SyntaxFactory.Block())
                .WithAdditionalAnnotations(Formatter.Annotation);

            return syntax;
        }

        private sealed class MatchedPropertySymbols
        {
            public IPropertySymbol Source { get; set; }
            public IPropertySymbol Target { get; set; }
        }
    }
}