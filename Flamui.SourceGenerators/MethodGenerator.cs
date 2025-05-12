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
            x.AddSource("FlamuiSourceGenerators.UiFragmentAttribute.cs", @"
namespace Flamui;

[AttributeUsage(AttributeTargets.Method)]
public class UiFragmentAttribute : Attribute
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

            return MethodSymbolToSomething(tuple.Value.Item1, tuple.Value.Item2);
        });

        context.RegisterSourceOutput(res, (ctx, component) =>
        {
            var result = BuilderClassGenerator.Generate(component);
            ctx.AddSource($"FlamuiSourceGenerators.{component.Name.ToFileName()}.g.cs",
                SourceText.From(result, Encoding.UTF8));

        });
    }

    private static MethodSignature MethodSymbolToSomething(IMethodSymbol component, GeneratorSyntaxContext syntaxContext)
    {
        // var attributeClass = syntaxContext.SemanticModel.Compilation.GetTypeByMetadataName("Flamui.ParameterAttribute");

        var parameters = new List<ParameterDefinition>();

        foreach (var parameter in component.Parameters)
        {
            parameters.Add(new ParameterDefinition(parameter.Name, parameter.Type.ToDisplayString()));
        }

        return new MethodSignature(parameters, component, component.ToDisplayString());
    }

    private bool Filter(SyntaxNode syntaxNode, CancellationToken token)
    {
        return syntaxNode is InvocationExpressionSyntax;
    }

    private (IMethodSymbol, GeneratorSyntaxContext)? Transform(GeneratorSyntaxContext syntaxContext, CancellationToken token)
    {
        var invocationExpressionSyntax = (InvocationExpressionSyntax)syntaxContext.Node;

        var symbol = syntaxContext.SemanticModel.GetDeclaredSymbol(invocationExpressionSyntax);

        if (symbol is not IMethodSymbol methodSymbol)
            return null;

        var attributes = methodSymbol.GetAttributes();

        if (attributes.Any(x => x.AttributeClass?.GetFullName() == "Flamui.UiFragmentAttribute"))
        {
            return (methodSymbol, syntaxContext);
        }

        return null;
    }
}
