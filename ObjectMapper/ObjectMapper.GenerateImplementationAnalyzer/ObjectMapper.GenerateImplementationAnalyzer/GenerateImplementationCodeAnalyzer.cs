using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ObjectMapper.GenerateImplementationAnalyzer.Utilities;

namespace ObjectMapper.GenerateImplementationAnalyzer
{
    /// <summary>
    /// Analyzer class for registering code generator for object mapper interfaces.
    /// </summary>
    /// <seealso cref="Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer" />
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class GenerateImplementationCodeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "OMCG01";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.GenerateImplementationAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.GenerateImplementationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.GenerateImplementationAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Code generation";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Hidden, isEnabledByDefault: true, description: Description);

        /// <summary>
        /// Returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        /// <summary>
        /// Called once at session start to register actions in the analysis context.
        /// </summary>
        /// <param name="context"></param>
        public override void Initialize(AnalysisContext context)
        {
            if (context == null) return;

            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleBaseType, SyntaxKind.MethodDeclaration);
        }

        /// <summary>
        /// Analyzes the node.
        /// </summary>
        /// <param name="context">The context.</param>
        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var baseNode = context.Node as SimpleBaseTypeSyntax;
            if (baseNode != null)
            {
                CheckForObjectMapperBaseInterface(baseNode, context);
                return;
            }

            var methodNode = context.Node as MethodDeclarationSyntax;
            if (methodNode != null && CheckForObjectMapperMethod(methodNode, context))
            {
                return;
            }
            if (methodNode != null && CheckForMethodWithObjectMapperAttribute(methodNode, context))
            {
                return;
            }
        }

        /// <summary>
        /// Checks if symbol under carret is object mapper interface.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="context">The context.</param>
        private static void CheckForObjectMapperBaseInterface(SimpleBaseTypeSyntax node, SyntaxNodeAnalysisContext context)
        {
            SimpleNameSyntax sns = (node.Type as SimpleNameSyntax) ?? (node.Type as QualifiedNameSyntax).Right;
            var className = sns?.Identifier.Text;
            if (className != "IObjectMapper" && className != "IObjectMapperAdapter")
            {
                return;
            }
            var symbol = context.SemanticModel.GetSymbolInfo(sns).Symbol as INamedTypeSymbol;
            if (symbol == null || symbol.TypeKind != TypeKind.Interface || !symbol.IsGenericType)
            {
                return;
            }

            var fullSymbolName = symbol.OriginalDefinition.ToDisplayString();
            if (fullSymbolName != "ObjectMapper.Framework.IObjectMapper<T>" && fullSymbolName != "ObjectMapper.Framework.IObjectMapperAdapter<T, U>")
            {
                return;
            }

            if (!FrameworkHelpers.IsObjectMapperFrameworkAssembly(symbol.OriginalDefinition.ContainingAssembly))
            {
                return;
            }

            var diagnostic = Diagnostic.Create(Rule, node.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

        /// <summary>
        /// Checks if method is implementation of object mapper interface.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="context">The context.</param>
        private static bool CheckForObjectMapperMethod(MethodDeclarationSyntax node, SyntaxNodeAnalysisContext context)
        {
            if (node?.Identifier.Text != "MapObject")
            {
                return false;
            }
            var symbol = context.SemanticModel.GetDeclaredSymbol(node);
            if (symbol == null || symbol.Kind != SymbolKind.Method || 
                (symbol.MethodKind != MethodKind.Ordinary && symbol.MethodKind != MethodKind.ExplicitInterfaceImplementation) || 
                symbol.DeclaredAccessibility != Accessibility.Public || !symbol.ReturnsVoid || 
                (symbol.Parameters.Length != 1 && symbol.Parameters.Length != 2))
            {
                return false;
            }

            // find out if we have implementation of framework interfaces
            INamedTypeSymbol mapperInterface = null;
            if (symbol.Parameters.Length == 1)
            {
                mapperInterface = symbol.ContainingType.AllInterfaces.FirstOrDefault(x => 
                    x.OriginalDefinition.ToDisplayString() == "ObjectMapper.Framework.IObjectMapper<T>" &&
                    FrameworkHelpers.IsObjectMapperFrameworkAssembly(x.OriginalDefinition.ContainingAssembly) &&
                    x.TypeArguments[0].Equals(symbol.Parameters[0].Type));
            }
            else if (symbol.Parameters.Length == 2)
            {
                mapperInterface = symbol.ContainingType.AllInterfaces.FirstOrDefault(x => 
                    x.OriginalDefinition.ToDisplayString() == "ObjectMapper.Framework.IObjectMapperAdapter<T, U>" &&
                    FrameworkHelpers.IsObjectMapperFrameworkAssembly(x.OriginalDefinition.ContainingAssembly) && 
                    (x.TypeArguments[0].Equals(symbol.Parameters[0].Type) && x.TypeArguments[1].Equals(symbol.Parameters[1].Type) 
                    || x.TypeArguments[0].Equals(symbol.Parameters[1].Type) && x.TypeArguments[1].Equals(symbol.Parameters[0].Type)));
            }
            if (mapperInterface == null)
            {
                return false;
            }

            // final check
            bool implementsInterfaceMethod = false;
            foreach (IMethodSymbol member in mapperInterface.GetMembers().Where(x => x.Kind == SymbolKind.Method))
            {
                if (symbol.Equals(symbol.ContainingType.FindImplementationForInterfaceMember(member)))
                {
                    implementsInterfaceMethod = true;
                    break;
                }
            }
            if (!implementsInterfaceMethod)
            {
                return false;
            }

            var diagnostic = Diagnostic.Create(Rule, node.Identifier.GetLocation());
            context.ReportDiagnostic(diagnostic);
            return true;
        }

        /// <summary>
        /// Checks for method with object mapper attribute.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static bool CheckForMethodWithObjectMapperAttribute(MethodDeclarationSyntax node, SyntaxNodeAnalysisContext context)
        {
            if (node.AttributeLists.Count == 0)
            {
                return false;
            }

            var candidateAttributes = node.AttributeLists.SelectMany(x => x.Attributes).Where(x =>
            {
                SimpleNameSyntax sns = (x.Name as SimpleNameSyntax) ?? (x.Name as QualifiedNameSyntax).Right;
                var className = sns?.Identifier.Text;
                return className == "ObjectMapperMethod" || className == "ObjectMapperMethodAttribute";
            }).ToList();
            if (candidateAttributes.Count == 0)
            {
                return false;
            }

            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(node);
            if (!methodSymbol.ReturnsVoid || methodSymbol.Parameters.Length != 2)
            {
                return false;
            }

            if (!candidateAttributes.Any(x =>
            {
                var symbol = context.SemanticModel.GetSymbolInfo(x).Symbol?.ContainingType;
                if (symbol == null)
                {
                    return false;
                }

                var fullSymbolName = symbol.ToDisplayString();
                if (fullSymbolName != "ObjectMapper.Framework.ObjectMapperMethodAttribute")
                {
                    return false;
                }

                if (!FrameworkHelpers.IsObjectMapperFrameworkAssembly(symbol.ContainingAssembly))
                {
                    return false;
                }

                return true;
            }))
            {
                return false;
            }

            var diagnostic = Diagnostic.Create(Rule, node.Identifier.GetLocation());
            context.ReportDiagnostic(diagnostic);
            return true;
        }
    }
}
