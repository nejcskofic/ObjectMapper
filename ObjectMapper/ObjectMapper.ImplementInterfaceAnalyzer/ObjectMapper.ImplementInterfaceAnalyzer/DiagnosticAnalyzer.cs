using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ObjectMapper.ImplementInterfaceAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ObjectMapperImplementInterfaceAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "OMCG001";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Code generation";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleBaseType);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = (SimpleBaseTypeSyntax)context.Node;
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
    }
}
