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
        context.RegisterPostInitializationOutput(x =>
        {
            x.AddSource("FlamuiSourceGenerators.ParameterAttribute.cs", @"
namespace Flamui;

[AttributeUsage(AttributeTargets.Property)]
public class ParameterAttribute : Attribute
{
}
");

            x.AddSource("FlamuiSourceGenerators.RefParameterAttribute.cs", @"
namespace Flamui;

[AttributeUsage(AttributeTargets.Property)]
public class RefParameterAttribute : Attribute
{
}
");
        });

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
            ctx.AddSource($"FlamuiSourceGenerators.{component.FullName.ToFileName()}Builder.g.cs",
                SourceText.From(result, Encoding.UTF8));

            var result2 = CreateMethodGenerator.Generate(component);
            ctx.AddSource($"FlamuiSourceGenerators.{component.FullName.ToFileName()}.g.cs",
                SourceText.From(result2, Encoding.UTF8));
        });
    }

    private static FlamuiComponentSg TypeSymbolToComponent(INamedTypeSymbol component, GeneratorSyntaxContext syntaxContext)
    {
        // var attributeClass = syntaxContext.SemanticModel.Compilation.GetTypeByMetadataName("Flamui.ParameterAttribute");

        var parameters = new List<ComponentParameter>();

        foreach (var symbol in component.GetMembers())
        {
            if(symbol is not IPropertySymbol propertySymbol)
                continue;

            foreach (var attributeData in propertySymbol.GetAttributes())
            {
                if(attributeData.AttributeClass is null)
                    continue;

                if(attributeData.AttributeClass.ContainingNamespace.ToDisplayString() != "Flamui")
                    continue;

                var isRef = false;

                if (attributeData.AttributeClass.Name == "RefParameterAttribute")
                {
                    isRef = true;
                }
                else if (attributeData.AttributeClass.Name != "ParameterAttribute")
                {
                    continue;
                }

                var isRequired = propertySymbol.IsRequired;

                parameters.Add(new ComponentParameter(propertySymbol.Name, propertySymbol.Type.ToDisplayString(), isRequired, isRef));
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
