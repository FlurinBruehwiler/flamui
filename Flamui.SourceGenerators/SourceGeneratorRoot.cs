using System.Text;
using Flamui.SourceGenerators.Infra;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Flamui.SourceGenerators;

// https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md

[Generator]
public class SourceGeneratorRoot : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(x =>
        {
//             x.AddSource("InterceptsLocationAttribute.generated.cs", @"
// [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
// file sealed class InterceptsLocationAttribute(string filePath, int line, int column) : Attribute;
// ");

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
            var result = CodeGeneration.Generate(component);
            ctx.AddSource($"FlamuiSourceGenerators.{component.Name.ToFileName()}_{Guid.NewGuid().ToString().Substring(0, 5)}.g.cs",
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

        return new MethodSignature(parameters, component, component.Name);
    }

    private bool Filter(SyntaxNode syntaxNode, CancellationToken token)
    {
        if (syntaxNode is not InvocationExpressionSyntax invocationExpressionSyntax)
            return false;

        if (invocationExpressionSyntax.ArgumentList.Arguments.Count == 0)
            return false;

        return true;
    }

    private (IMethodSymbol, GeneratorSyntaxContext)? Transform(GeneratorSyntaxContext syntaxContext, CancellationToken token)
    {
        //todo update .net sdk so that we can generate the data needed for the interceptor

        var invocationExpressionSyntax = (InvocationExpressionSyntax)syntaxContext.Node;

        var symbol = syntaxContext.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol;

        if (symbol is not IMethodSymbol methodSymbol)
            return null;

        foreach (var parameter in methodSymbol.Parameters)
        {
            if (parameter.Type.GetFullName() == "Flamui.Flamui.Ui")
                return (methodSymbol, syntaxContext);
        }

        return null;
    }
}
