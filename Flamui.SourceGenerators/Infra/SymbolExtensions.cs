using Microsoft.CodeAnalysis;

namespace Flamui.SourceGenerators.Infra;

public static class SymbolExtensions
{
    public static string GetFullName(this ITypeSymbol typeSymbol)
    {
        return $"{typeSymbol.ContainingNamespace.ToDisplayString()}.{typeSymbol.ToDisplayString()}";
    }

    public static string ToFileName(this string str)
    {
        return str.Replace("<", "_").Replace(">", "_");
    }
}
