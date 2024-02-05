using Microsoft.CodeAnalysis;

namespace Flamui.SourceGenerators;

public static class SymbolExtensions
{
    public static string GetFullName(this ITypeSymbol typeSymbol)
    {
        return $"{typeSymbol.ContainingNamespace}.{typeSymbol.Name}";
    }
}
