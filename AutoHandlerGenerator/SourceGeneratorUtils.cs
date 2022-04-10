using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoHandlerGenerator;

public static class SourceGeneratorUtils
{
    public static bool NeedUseAutoSerializeOrDeserialize(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.GetAttributes()
                    .Any(symbol => symbol.AttributeClass?.Name is "AutoSerializeAttribute") || namedTypeSymbol
                    .GetAttributes().Any(symbol => symbol.AttributeClass?.Name is "AutoDeserializeAttribute"))
            {
                return true;
            }

            if (IsList(namedTypeSymbol))
            {
                return NeedUseAutoSerializeOrDeserialize(namedTypeSymbol.TypeArguments[0]);
            }
        }
        else if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            return NeedUseAutoSerializeOrDeserialize(arrayTypeSymbol.ElementType);
        }

        return false;
    }
    
    public static bool CheckClassIsPartial(INamedTypeSymbol namedTypeSymbol)
    {
        foreach (var declaringSyntaxReference in namedTypeSymbol.DeclaringSyntaxReferences)
        {
            if (CheckClassIsPartial((ClassDeclarationSyntax) declaringSyntaxReference.GetSyntax()))
                return true;
        }

        return false;
    }

    public static bool CheckClassIsPublic(INamedTypeSymbol namedTypeSymbol)
    {
        foreach (var declaringSyntaxReference in namedTypeSymbol.DeclaringSyntaxReferences)
        {
            if (CheckClassIsPublic((ClassDeclarationSyntax) declaringSyntaxReference.GetSyntax()))
                return true;
        }

        return false;
    }

    public static bool CheckClassIsPublic(ClassDeclarationSyntax classDeclarationSyntax)
    {
        return classDeclarationSyntax.Modifiers.Any(SyntaxKind.PublicKeyword);
    }

    public static bool CheckClassIsPartial(ClassDeclarationSyntax classDeclarationSyntax)
    {
        return classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword);
    }

    public static bool IsList(ITypeSymbol typeSymbol)
    {
        return typeSymbol.AllInterfaces.Any(symbol =>
            symbol.Name == "ICollection" || symbol.Name == "IReadOnlyCollection`1");
    }

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