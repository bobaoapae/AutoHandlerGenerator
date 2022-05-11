using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoHandlerGenerator;

[Generator(LanguageNames.CSharp)]
public class AutoHandlerIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarationsServer = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGenerationSerialize(ctx))
            .Where(static (s) => IsNamedTargetForGeneration(s));

        var compilationAndClassesServer = context.CompilationProvider.Combine(classDeclarationsServer.Collect());

        context.RegisterSourceOutput(compilationAndClassesServer, static (spc, source) => AutoHandlerGenerator.Generate(source.Item1, source.Item2, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax;
    }

    private static bool IsNamedTargetForGeneration(INamedTypeSymbol node)
    {
        return SourceGeneratorUtils.CheckClassOrBaseHasAttribute(node, "AutoHandlerAttribute");
    }

    private static INamedTypeSymbol GetSemanticTargetForGenerationSerialize(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax) context.Node;

        var model = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

        return (INamedTypeSymbol) model;
    }
}