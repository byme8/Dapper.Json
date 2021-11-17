using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DuckInterface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dapper.Json.SourceGenerator
{
    [Generator]
    public class DapperJsonSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new DapperSourceGeneratorSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not DapperSourceGeneratorSyntaxReceiver syntaxReceiver)
            {
                return;
            }

            try
            {

                var jsonType = context.Compilation.GetTypeByMetadataName("Dapper.Json.Json`1");

                var types = syntaxReceiver.Types
                    .Select(o =>
                    {
                        var semanticModel = context.Compilation.GetSemanticModel(o.SyntaxTree);
                        var type = semanticModel.GetTypeInfo(o);
                        if (type.Type is INamedTypeSymbol namedTypeSymbol &&
                            namedTypeSymbol.ConstructedFrom.Equals(jsonType))
                        {
                            return namedTypeSymbol.TypeArguments.First();
                        }

                        return null;
                    })
                    .Where(o => o != null)
                    .Distinct(SymbolEqualityComparer.Default)
                    .ToArray();

                var source = @$"using System;

namespace Dapper.Json
{{
    public static class DapperJsonModuleInitializer
    {{
        [global::System.Runtime.CompilerServices.ModuleInitializer]
        public static void Init()
        {{
{types.Select(o => $@"            SqlMapper.AddTypeHandler(new JsonTypeHandler<{o.ToGlobalName()}>());").Join()}
        }}
    }}
}}
";

                context.AddSource("Dapper.Json.g.cs", source.ToSourceText());
            }
            catch (Exception e)
            {
                context.AddSource("Dapper.Json.Error.g.cs", @$"

namespace Dapper.Json
{{
    public static class DapperJsonError
    {{
        public static string Error = ""{e.Message}"";
    }}
}}

");
            }
        }
    }

    public class DapperSourceGeneratorSyntaxReceiver : ISyntaxReceiver
    {
        public List<GenericNameSyntax> Types { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is PropertyDeclarationSyntax { Type: GenericNameSyntax { Identifier.ValueText: "Json", } type })
            {
                Types.Add(type);
            }
        }
    }
}