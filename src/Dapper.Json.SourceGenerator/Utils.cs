using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DuckInterface
{
    public static class Utils
    {
        public const string EditorBrowsable = "[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]";

        public static string GetDuckImplementationClassName(ITypeSymbol duckInterface)
        {
            return $"Duck_{duckInterface.ToSafeGlobalName()}";
        }

        public static string LowerFirstChar(this string value)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(char.ToLower(value.First()));
            var span = value.AsSpan(1);
            for (int i = 0; i < span.Length; i++)
            {
                stringBuilder.Append(span[i]);
            }

            return stringBuilder.ToString();
        }

        public static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol symbol, HashSet<ITypeSymbol> processedInterfaces = null)
        {
            processedInterfaces ??= new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            if (symbol.BaseType != null)
            {
                foreach (var member in symbol.BaseType.GetAllMembers(processedInterfaces))
                {
                    yield return member;
                }
            }

            foreach (var member in symbol.GetMembers())
            {
                yield return member;
            }

            var typeSymbols = symbol.Interfaces
                .Where(o => !processedInterfaces.Contains(o))
                .OfType<ITypeSymbol>()
                .ToArray();

            foreach (var type in typeSymbols)
            {
                processedInterfaces.Add(type);
            }

            var members = typeSymbols
                .SelectMany(o => o.GetAllMembers(processedInterfaces))
                    .Distinct(SymbolEqualityComparer.Default);

            foreach (var member in members)
            {
                yield return member;
            }
        }

        public static ITypeSymbol GetTypeSymbol(this SymbolInfo info)
        {
            switch (info.Symbol)
            {
                case ITypeSymbol type:
                    return type;
                case ILocalSymbol local:
                    return local.Type;
                case IParameterSymbol parameterSymbol:
                    return parameterSymbol.Type;
                default:
                    return null;
            }
        }

        public static IEnumerable<IMethodSymbol> GetPublicMethods(this IEnumerable<ISymbol> members)
        {
            return members
                .OfType<IMethodSymbol>()
                .Where(o => o.DeclaredAccessibility == Accessibility.Public)
                .Where(o => o.MethodKind == MethodKind.Ordinary);
        }

        public static (bool IsDuckable, IEnumerable<ISymbol> MissingSymbols) IsDuckableTo(
            this ITypeSymbol @interface, ITypeSymbol implementation)
        {
            var methodsToDuck = MemberThatCanBeDucked(@interface);
            var memberThatCanBeDucked = MemberThatCanBeDucked(implementation);

            var missingSymbols = methodsToDuck
                .Where(o => !memberThatCanBeDucked.ContainsKey(o.Key))
                .Select(o => o.Value)
                .ToArray();

            return (!missingSymbols.Any(), missingSymbols);
        }

        private static Dictionary<string, ISymbol> MemberThatCanBeDucked(ITypeSymbol type)
        {
            var properties = type
                .GetAllMembers()
                .OfType<IPropertySymbol>()
                .Where(o => o.DeclaredAccessibility.HasFlag(Accessibility.Public))
                .Select(o =>
                (
                    Key:
                    $"{o.Type.ToGlobalName()}_{o.Name}{(o.GetMethod != null ? "_getter" : string.Empty)}{(o.SetMethod != null ? "_setter" : string.Empty)}",
                    Value: (ISymbol)o
                ))
                .ToArray();

            var methods = type
                .GetAllMembers()
                .GetPublicMethods()
                .Select(o =>
                (
                    Key:
                    $"{o.ReturnType.ToGlobalName()}_${o.Name}_{o.Parameters.Select(oo => oo.Type.ToGlobalName() + oo.Name).Join("_")}",
                    Value: (ISymbol)o
                ))
                .ToArray();

            return methods
                .Concat(properties)
                .GroupBy(o => o.Key)
                .ToDictionary(o => o.Key, o => o.First().Value);
        }

        public static string GetUniqueName(this ITypeSymbol type)
        {
            return $"{type.Name}_{Guid.NewGuid().ToString().Replace("-", "")}";
        }

        public static SourceText ToSourceText(this string source)
        {
            return SourceText.From(source, Encoding.UTF8);
        }

        public static string ToGlobalName(this ISymbol symbol)
        {
            return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        public static string ToSafeGlobalName(this ISymbol symbol)
        {
            return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                .Replace("global::", "")
                .Replace("<", "Of_")
                .Replace(">", "_End")
                .Replace(".", "_");
        }

        public static string Join(this IEnumerable<string> values, string separator = ", ")
        {
            return string.Join(separator, values);
        }

        public static string Wrap(this string text, string left = "", string right = "")
        {
            return $"{left}{text}{right}";
        }

        public static string JoinWithNewLine(this IEnumerable<string> values, string separator = "")
        {
            return string.Join($"{separator}{Environment.NewLine}", values);
        }
    }
}