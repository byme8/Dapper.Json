using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dapper.Json.SourceGenerator;

public class DapperSourceGeneratorSyntaxReceiver : ISyntaxReceiver
{
    public List<GenericNameSyntax> Types { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is GenericNameSyntax { Identifier.ValueText: "Json", } type)
        {
            Types.Add(type);
        }
    }
}