using System.Linq;
using Microsoft.CodeAnalysis;

namespace AutoHandlerGenerator;

public static class SourceGeneratorUtils
{
    public static bool CheckClassOrBaseHasAttribute(INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol attributeSymbol)
    {
        if (namedTypeSymbol == null)
            return false;
        
        if (namedTypeSymbol.GetAttributes().Any(ad => ad.AttributeClass?.Name == attributeSymbol.Name))
            return true;
        if (namedTypeSymbol.BaseType != null)
            return CheckClassOrBaseHasAttribute(namedTypeSymbol.BaseType, attributeSymbol);

        return false;
    }
}