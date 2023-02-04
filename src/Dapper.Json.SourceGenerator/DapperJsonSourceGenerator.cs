using System;
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
            .CreateSyntaxProvider(
                SelectJsonT,
                (o, token) => o.SemanticModel.GetTypeInfo(o.Node, token));
        
        var combined = jsons.Combine(jsonType);
        context.RegisterImplementationSourceOutput(combined, Generate);
    }

    private void Generate(SourceProductionContext context, (TypeInfo Target, INamedTypeSymbol JsonType) input)
    {
        var (target, jsonType) = input;
        if (target.Type is not INamedTypeSymbol namedTypeSymbol ||
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

    private bool SelectJsonT(SyntaxNode syntaxNode, CancellationToken token)
    {
        if (syntaxNode is GenericNameSyntax { Identifier.ValueText: "Json", } type)
        {
            return true;
        }

        return false;
    }
}