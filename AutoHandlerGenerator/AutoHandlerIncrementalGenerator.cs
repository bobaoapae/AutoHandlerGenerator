using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoHandlerGenerator;

[Generator]
public class AutoHandlerIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarationsServer = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGenerationSerialize(ctx))
            .Where(static m => m is not null);

        IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClassesServer
            = context.CompilationProvider.Combine(classDeclarationsServer.Collect());

        context.RegisterSourceOutput(compilationAndClassesServer,
            static (spc, source) => AutoHandlerGenerator.Generate(source.Item1, source.Item2, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax;
    }

    private static ClassDeclarationSyntax GetSemanticTargetForGenerationSerialize(GeneratorSyntaxContext context)
    {
        var attributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("AutoHandlerGenerator.Definitions.AutoHandlerAttribute")!;

        var classDeclarationSyntax = (ClassDeclarationSyntax) context.Node;

        var model = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

        if (model is INamedTypeSymbol namedTypeSymbol)
        {
            if (SourceGeneratorUtils.CheckClassOrBaseHasAttribute(namedTypeSymbol, attributeSymbol))
            {
                return classDeclarationSyntax;
            }
        }

        return null;
    }
}