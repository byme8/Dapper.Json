using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using DuckInterface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dapper.Json.SourceGenerator;

[Generator]
public class DapperJsonSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var jsonType = context.CompilationProvider
            .Select((o, t) => o.GetTypeByMetadataName("Dapper.Json.Json`1"));

        var jsons = context.SyntaxProvider
            .CreateSyntaxProvider(SelectJsonT, Transform);

        var combined = jsons.Collect()
            .Combine(jsonType);

        context.RegisterImplementationSourceOutput(combined, Generate);
    }

    private void Generate(SourceProductionContext context,
        (ImmutableArray<ITypeSymbol> Targets, INamedTypeSymbol JsonType) input)
    {
        var (targets, jsonType) = input;
        var uniqueTargets = targets.Distinct(SymbolEqualityComparer.Default);
        foreach (var target in uniqueTargets)
        {
            if (target is not INamedTypeSymbol namedTypeSymbol ||
                !SymbolEqualityComparer.Default.Equals(namedTypeSymbol.ConstructedFrom, jsonType))
            {
                return;
            }

            var type = namedTypeSymbol.TypeArguments.First();
            if (type is ITypeParameterSymbol)
            {
                return;
            }

            var safeTypeName = type.ToSafeGlobalName();
            var source = @$"using System;

namespace Dapper.Json
{{
    public static class DapperJson{safeTypeName}ModuleInitializer
    {{
        [global::System.Runtime.CompilerServices.ModuleInitializer]
        public static void Init()
        {{
            SqlMapper.AddTypeHandler(new JsonTypeHandler<{type.ToGlobalName()}>());
        }}
    }}
}}
";
            context.AddSource($"Dapper.Json.{safeTypeName}.g.cs", source.ToSourceText());
        }
    }

    private ITypeSymbol Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        switch (context.Node)
        {
            case GenericNameSyntax:
                return context.SemanticModel.GetTypeInfo(context.Node, cancellationToken).Type;
            case MemberAccessExpressionSyntax memberAccess:
                var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess.Name, cancellationToken);
                if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
                {
                    return methodSymbol.ReturnType;
                }

                return null;
            default:
                return null;
        }
    }

    private bool SelectJsonT(SyntaxNode syntaxNode, CancellationToken token)
    {
        if (syntaxNode is GenericNameSyntax { Identifier.ValueText: "Json", } type)
        {
            return true;
        }

        if (syntaxNode is MemberAccessExpressionSyntax
            {
                Parent: InvocationExpressionSyntax,
                Name.Identifier.ValueText: "AsJson"
            })
        {
            return true;
        }

        return false;
    }
}