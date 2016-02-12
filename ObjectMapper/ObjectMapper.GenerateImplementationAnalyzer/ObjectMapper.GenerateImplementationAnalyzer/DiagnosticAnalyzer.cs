using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ObjectMapper.GenerateImplementationAnalyzer
{
    /// <summary>
    /// Analyzer class for registering code generator for object mapper interfaces.
    /// </summary>
    /// <seealso cref="Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer" />
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ObjectMapperImplementInterfaceAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "OMCG001";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
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
            if (methodNode != null)
            {
                CheckForObjectMapperMethod(methodNode, context);
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

            var diagnostic = Diagnostic.Create(Rule, node.GetLocation(), symbol.ToString());
            context.ReportDiagnostic(diagnostic);
        }

        /// <summary>
        /// Checks if method is implementation of object mapper interface.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="context">The context.</param>
        private static void CheckForObjectMapperMethod(MethodDeclarationSyntax node, SyntaxNodeAnalysisContext context)
        {
            if (node?.Identifier.Text != "MapObject")
            {
                return;
            }
            var symbol = context.SemanticModel.GetDeclaredSymbol(node);
            if (symbol == null || symbol.Kind != SymbolKind.Method || 
                (symbol.MethodKind != MethodKind.Ordinary && symbol.MethodKind != MethodKind.ExplicitInterfaceImplementation) || 
                symbol.DeclaredAccessibility != Accessibility.Public || !symbol.ReturnsVoid || 
                (symbol.Parameters.Length != 1 && symbol.Parameters.Length != 2))
            {
                return;
            }

            // find out if we have implementation of framework interfaces
            INamedTypeSymbol mapperInterface = null;
            if (symbol.Parameters.Length == 1)
            {
                mapperInterface = symbol.ContainingType.AllInterfaces.FirstOrDefault(x => x.OriginalDefinition.ToDisplayString() == "ObjectMapper.Framework.IObjectMapper<T>" && x.TypeArguments[0].Equals(symbol.Parameters[0].Type));
            }
            else if (symbol.Parameters.Length == 2)
            {
                mapperInterface = symbol.ContainingType.AllInterfaces.FirstOrDefault(x => x.OriginalDefinition.ToDisplayString() == "ObjectMapper.Framework.IObjectMapperAdapter<T, U>" && (x.TypeArguments[0].Equals(symbol.Parameters[0].Type) && x.TypeArguments[1].Equals(symbol.Parameters[1].Type) || x.TypeArguments[0].Equals(symbol.Parameters[1].Type) && x.TypeArguments[1].Equals(symbol.Parameters[0].Type)));
            }
            if (mapperInterface == null)
            {
                return;
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
                return;
            }

            var diagnostic = Diagnostic.Create(Rule, node.Identifier.GetLocation(), mapperInterface.ToString());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
