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
//             x.AddSource("NoScopeGenerationAttribute.cs", @"
// using System;
// using System.Diagnostics;
//
// #nullable enable
// namespace Flamui
// {
//     [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
//     public class NoScopeGenerationAttribute : Attribute
//     {
//     }
// }
// ");
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

        context.RegisterSourceOutput(res, (ctx, method) =>
        {
            var result = CodeGeneration.Generate(method);
            ctx.AddSource($"FlamuiSourceGenerators.{method.Name.ToFileName()}_{method.Hash}.g.cs",
                SourceText.From(result, Encoding.UTF8));
        });
    }

    private static MethodSignature MethodSymbolToSomething(IMethodSymbol methodSymbol, InvocationExpressionSyntax syntax, GeneratorSyntaxContext syntaxContext)
    {
        methodSymbol = methodSymbol.OriginalDefinition;

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

        var classTypeParameters = methodSymbol.ContainingType.OriginalDefinition.TypeParameters.Select(x => new TypeParameterDefinition
        {
            Name = x.Name,
            HasReferenceTypeConstraint = x.HasReferenceTypeConstraint,
            HasUnmanagedTypeConstraint = x.HasUnmanagedTypeConstraint,
            HasValueTypeConstraint = x.HasValueTypeConstraint,
            IsOnMethod = false
        });

        var methodTypeParameters = methodSymbol.TypeParameters.Select(x => new TypeParameterDefinition
        {
            Name = x.Name,
            HasReferenceTypeConstraint = x.HasReferenceTypeConstraint,
            HasUnmanagedTypeConstraint = x.HasUnmanagedTypeConstraint,
            HasValueTypeConstraint = x.HasValueTypeConstraint,
            IsOnMethod = true
        });

        var interceptLocation = CSharpExtensions.GetInterceptableLocation(syntaxContext.SemanticModel, syntax);

        var methodSignature = new MethodSignature
        {
            ReceiverTypeFullyQualifiedName =
                methodSymbol.ReceiverType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            ReturnTypeFullyQualifiedName =
                methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            Name = methodSymbol.Name,
            IsStatic = methodSymbol.IsStatic,
            IsExtensionMethod = methodSymbol.IsExtensionMethod,
            ReceiverTypeIsUiType = methodSymbol.ReceiverType.GetFullName() is "Flamui.Flamui.Ui" or "Flamui.Ui",
            ReturnsVoid = methodSymbol.ReturnsVoid,
            InterceptableLocation = interceptLocation,
            Parameters = new EquatableArray<ParameterDefinition>(parameters.ToArray()),
            ContainingTypeFullName = methodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            IsPrivate = methodSymbol.DeclaredAccessibility != Accessibility.Public || methodSymbol.ContainingType.DeclaredAccessibility != Accessibility.Public,
            ReturnsByRef = methodSymbol.ReturnsByRef,
            Hash = Math.Abs(GetDeterministicHashCode(interceptLocation.Data)),
            MethodTypeParameters = new EquatableArray<TypeParameterDefinition>(methodTypeParameters.ToArray()),
            ClassTypeParameters = new EquatableArray<TypeParameterDefinition>(classTypeParameters.ToArray()),
        };

        return methodSignature;
    }

    private static int GetDeterministicHashCode(string str)
    {
        unchecked
        {
            int hash1 = (5381 << 16) + 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if(i == str.Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
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

        if (methodSymbol.GetAttributes().Any(x => x.AttributeClass?.Name == "NoScopeGenerationAttribute"))
            return null;

        if(!methodSymbol.IsStatic && methodSymbol.ReceiverType?.Name == "Ui")
            return (methodSymbol, invocationExpressionSyntax, syntaxContext);

        foreach (var parameter in methodSymbol.Parameters)
        {
            if (parameter.Type.Name == "Ui")
                return (methodSymbol, invocationExpressionSyntax, syntaxContext);
        }

        return null;
    }
}
