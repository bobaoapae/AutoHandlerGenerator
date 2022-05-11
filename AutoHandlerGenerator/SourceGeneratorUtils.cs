using System.Linq;
using Microsoft.CodeAnalysis;

namespace AutoHandlerGenerator;

public static class SourceGeneratorUtils
{
    public static bool CheckClassOrBaseHasAttribute(INamedTypeSymbol namedTypeSymbol, string attributeName)
    {
        if (namedTypeSymbol == null)
            return false;
        
        if (namedTypeSymbol.GetAttributes().Any(ad => ad.AttributeClass?.Name == attributeName))
            return true;
        if (namedTypeSymbol.BaseType != null)
            return CheckClassOrBaseHasAttribute(namedTypeSymbol.BaseType, attributeName);

        return false;
    }
}