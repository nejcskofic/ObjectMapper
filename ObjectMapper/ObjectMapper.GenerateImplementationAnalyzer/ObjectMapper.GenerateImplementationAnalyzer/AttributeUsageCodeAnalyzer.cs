using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ObjectMapper.GenerateImplementationAnalyzer.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMapper.GenerateImplementationAnalyzer
{
    /// <summary>
    /// Analyzer class for analyzing and reporting mapping attribute for mapping methods missuse.
    /// </summary>
    /// <seealso cref="Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer" />
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AttributeUsageCodeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "OMAU01";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AttributeUsageAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AttributeUsageAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AttributeUsageAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Attribute usage";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

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

            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.Attribute);
        }

        /// <summary>
        /// Analyzes the node.
        /// </summary>
        /// <param name="context">The context.</param>
        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var attributeNode = context.Node as AttributeSyntax;
            SimpleNameSyntax sns = (attributeNode.Name as SimpleNameSyntax) ?? (attributeNode.Name as QualifiedNameSyntax).Right;
            var className = sns?.Identifier.Text;
            if (className != "ObjectMapperMethod" && className != "ObjectMapperMethodAttribute")
            {
                return;
            }

            // symbol is ctor call
            var symbol = context.SemanticModel.GetSymbolInfo(attributeNode).Symbol.ContainingType;
            if (symbol == null)
            {
                return;
            }

            var fullSymbolName = symbol.ToDisplayString();
            if (fullSymbolName != "ObjectMapper.Framework.ObjectMapperMethodAttribute")
            {
                return;
            }

            if (!FrameworkHelpers.IsObjectMapperFrameworkAssembly(symbol.ContainingAssembly))
            {
                return;
            }

            var methodSyntax = attributeNode.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (methodSyntax == null)
            {
                // missused attribute - compiler will take care of that
                return;
            }

            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodSyntax);
            if (methodSymbol.ReturnsVoid && methodSymbol.Parameters.Length == 2)
            {
                // correct use
                return;
            }

            var diagnostic = Diagnostic.Create(Rule, attributeNode.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
