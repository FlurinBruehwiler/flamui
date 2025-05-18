using System.Text;
using Flamui.SourceGenerators.Infra;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace Flamui.SourceGenerators;

// https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md

[Generator]
public class SourceGeneratorRoot : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(x =>
        {

        });


        var flamuiComponents = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: Filter, transform: Transform)
            .Where(static m => m is not null);

        var res = flamuiComponents.Select(static (tuple, _) =>
        {
            if (tuple is null)
                return default;

            return MethodSymbolToSomething(tuple.Value.Item1, tuple.Value.Item2, tuple.Value.Item3);
        });

        context.RegisterSourceOutput(res, (ctx, component) =>
        {
            var result = CodeGeneration.Generate(component);
            ctx.AddSource($"FlamuiSourceGenerators.{component.Name.ToFileName()}_{Guid.NewGuid().ToString().Substring(0, 5)}.g.cs",
                SourceText.From(result, Encoding.UTF8));
        });
    }

    private static MethodSignature MethodSymbolToSomething(IMethodSymbol methodSymbol, InvocationExpressionSyntax syntax, GeneratorSyntaxContext syntaxContext)
    {
        var parameters = methodSymbol.Parameters.Select(x =>
        {
            var pd = new ParameterDefinition
            {
                DisplayString = x.ToDisplayString(),
                IsUiType = x.Type.GetFullName() is "Flamui.Flamui.Ui" or "Flamui.Ui",
                Name = x.Name,
                RefKind = x.RefKind
            };

            return pd;
        });

        var methodSignature = new MethodSignature
        {
            ReceiverTypeFullyQualifiedName =
                methodSymbol.ReceiverType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            ReturnTypeFullyQualifiedName =
                methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            Name = methodSymbol.Name,
            IsStatic = methodSymbol.IsStatic,
            ReceiverTypeIsUiType = methodSymbol.ReceiverType.GetFullName() is "Flamui.Flamui.Ui" or "Flamui.Ui",
            ReturnsVoid = methodSymbol.ReturnsVoid,
            InterceptableLocation = CSharpExtensions.GetInterceptableLocation(syntaxContext.SemanticModel, syntax),
            Parameters = new EquatableArray<ParameterDefinition>(parameters.ToArray())
        };

        return methodSignature;
    }

    private bool Filter(SyntaxNode syntaxNode, CancellationToken token)
    {
        if (syntaxNode is not InvocationExpressionSyntax)
            return false;

        return true;
    }

    private (IMethodSymbol, InvocationExpressionSyntax, GeneratorSyntaxContext)? Transform(GeneratorSyntaxContext syntaxContext, CancellationToken token)
    {
        var invocationExpressionSyntax = (InvocationExpressionSyntax)syntaxContext.Node;

        var symbol = syntaxContext.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol;

        if (symbol is not IMethodSymbol methodSymbol)
            return null;

        if(!methodSymbol.IsStatic && methodSymbol.ReceiverType.GetFullName() is "Flamui.Flamui.Ui" or "Flamui.Ui")
            return (methodSymbol, invocationExpressionSyntax, syntaxContext);

        foreach (var parameter in methodSymbol.Parameters)
        {
            if (parameter.Type.GetFullName() is "Flamui.Flamui.Ui" or "Flamui.Ui")
                return (methodSymbol, invocationExpressionSyntax, syntaxContext);
        }

        return null;
    }
}
