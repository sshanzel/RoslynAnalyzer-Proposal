using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProducesResponseTypeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ProducesResponseTypeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ProducesResponseTypeAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(syntaxTreeContext =>
            {
                // Iterate through all statements in the tree
                var root = syntaxTreeContext.Tree.GetRoot(syntaxTreeContext.CancellationToken);
                foreach (var statement in root.DescendantNodes().OfType<ReturnStatementSyntax>()) {
                    var returnType = statement.ReturnKeyword.Value.GetType();

                    if (returnType == typeof(ActionResult)) {
                        if (!returnType.ContainsGenericParameters) continue;

                        var genericType = returnType.GenericTypeArguments.FirstOrDefault();

                        if (genericType == null) continue;

                        if (genericType != typeof(OkObjectResult)) continue;

                        if (returnType != genericType) continue;

                        // Sample Message
                        var diagnostic = Diagnostic.Create(Rule, statement.GetFirstToken().GetLocation());
                        syntaxTreeContext.ReportDiagnostic(diagnostic);
                    }

                    if (returnType != typeof(Task)) continue;

                    if (returnType == typeof(Task)) {
                        var parameterType = returnType.GetGenericParameterConstraints().FirstOrDefault();

                        if (parameterType == null) continue;

                        if (parameterType != typeof(ActionResult)) continue;

                        var genericType = parameterType.GenericTypeArguments.FirstOrDefault();

                        if (genericType == null) continue;

                        if (genericType != typeof(OkObjectResult)) continue;

                        if (!genericType.ContainsGenericParameters) continue;

                        if (returnType != genericType) continue;

                        // Sample Message
                        var diagnostic = Diagnostic.Create(Rule, statement.GetFirstToken().GetLocation());
                        syntaxTreeContext.ReportDiagnostic(diagnostic);
                    }
                }
            });
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            // Find just those named type symbols with names containing lowercase letters.
            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
