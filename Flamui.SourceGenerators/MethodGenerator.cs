using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Flamui.SourceGenerators;

// https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md

[Generator]
public class MethodGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add the marker attribute to the compilation
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "EnumExtensionsAttribute.g.cs",
            SourceText.From(BuilderClassGenerator.Attribute, Encoding.UTF8)));

        var flamuiComponents = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: Filter, transform: Transform)
            .Where(static m => m is not null);

        var res = flamuiComponents.Select(static (tuple, _) =>
        {
            if (tuple is null)
                return default;

            return TypeSymbolToComponent(tuple.Value.Item1, tuple.Value.Item2);
        });

        context.RegisterSourceOutput(res, (ctx, component) =>
        {
            var result = BuilderClassGenerator.Generate(component);
            ctx.AddSource($"FlamuiSourceGenerators.{component.FullName.Replace("<", "_").Replace(">", "_")}Builder.g.cs", SourceText.From(result, Encoding.UTF8));
        });

        context.RegisterSourceOutput(res, (ctx, component) =>
        {
            var result = CreateMethodGenerator.Generate(component);
            ctx.AddSource($"FlamuiSourceGenerators.{component.FullName.Replace("<", "_").Replace(">", "_")}.g.cs", SourceText.From(result, Encoding.UTF8));
        });
    }

    private static FlamuiComponentSg TypeSymbolToComponent(INamedTypeSymbol component, GeneratorSyntaxContext syntaxContext)
    {
        //ToDo
        //var attributeClass = syntaxContext.SemanticModel.Compilation.GetTypeByMetadataName("Flamui.ParameterAttribute");

        var parameters = new List<ComponentParameter>();

        foreach (var symbol in component.GetMembers())
        {
            if(symbol is not IPropertySymbol propertySymbol)
                continue;

            foreach (var attributeData in propertySymbol.GetAttributes())
            {
                if(attributeData.AttributeClass is null)
                    continue;

                if(attributeData.AttributeClass.Name != "ParameterAttribute"
                   || attributeData.AttributeClass.ContainingNamespace.ToDisplayString() != "Flamui")
                    continue;

                var isRequired = propertySymbol.IsRequired;
                var isRef = attributeData.ConstructorArguments.Any(x => x.Value?.ToString() == "true");

                parameters.Add(new ComponentParameter(propertySymbol.Name, propertySymbol.Type.GetFullName(), isRequired, isRef));
                break;
            }
        }

        return new FlamuiComponentSg(parameters, component, component.ToDisplayString());
    }

    private bool Filter(SyntaxNode syntaxNode, CancellationToken token)
    {
        return syntaxNode is ClassDeclarationSyntax { BaseList.Types.Count: >= 1 };
    }

    private (INamedTypeSymbol, GeneratorSyntaxContext)? Transform(GeneratorSyntaxContext syntaxContext, CancellationToken token)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)syntaxContext.Node;

        if (classDeclarationSyntax.BaseList is null)
            return null;

        var symbol = syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

        if (symbol is not INamedTypeSymbol namedTypeSymbol)
            return null;

        if (namedTypeSymbol.BaseType is null)
            return null;

        var flamuiComponent = syntaxContext.SemanticModel.Compilation.GetTypeByMetadataName("Flamui.FlamuiComponent");

        if (SymbolEqualityComparer.Default.Equals(flamuiComponent, namedTypeSymbol.BaseType))
        {
            return (namedTypeSymbol, syntaxContext);
        }

        return null;
    }
}
