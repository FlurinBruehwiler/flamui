using System.Reflection;
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
// //Test File
// ");
        });


        var flamuiComponents = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, _) => node is InvocationExpressionSyntax,
                transform: (syntaxContext, _) => syntaxContext)
            .Select((syntaxContext, _) =>
            {
                var invocationExpressionSyntax = (InvocationExpressionSyntax)syntaxContext.Node;

                var symbol = syntaxContext.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol;

                if (symbol is not IMethodSymbol methodSymbol)
                    return null;

                foreach (var attributeData in methodSymbol.GetAttributes())
                {
                    if (attributeData.AttributeClass?.Name == "NoScopeGenerationAttribute")
                        return null;
                }

                if (!methodSymbol.IsStatic && methodSymbol.ReceiverType?.Name == "Ui")
                {
                    return new TransformResult(methodSymbol, invocationExpressionSyntax, syntaxContext);
                }

                foreach (var parameter in methodSymbol.Parameters)
                {
                    if (parameter.Type.Name == "Ui")
                    {
                        return new TransformResult(methodSymbol, invocationExpressionSyntax, syntaxContext);
                    }
                }

                return null;
            })
            .Where(x => x != null)
            .Select((result, _) => MethodSymbolToSomething(result.MethodSymbol, result.InvocationExpressionSyntax, result.GeneratorSyntaxContext));

        context.RegisterSourceOutput(flamuiComponents, (ctx, method) =>
        {
            var result = CodeGeneration.Generate(method);
            ctx.AddSource($"FlamuiSourceGenerators.{method.Name.ToFileName()}_{method.Hash}.g.cs",
                SourceText.From(result, Encoding.UTF8));
        });
    }


    private static MethodSignature MethodSymbolToSomething(IMethodSymbol methodSymbol,
        InvocationExpressionSyntax syntax, GeneratorSyntaxContext syntaxContext)
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

        var classTypeParameters = methodSymbol.ContainingType.OriginalDefinition.TypeParameters.Select(x =>
            new TypeParameterDefinition
            {
                Name = x.Name,
                HasReferenceTypeConstraint = x.HasReferenceTypeConstraint,
                HasUnmanagedTypeConstraint = x.HasUnmanagedTypeConstraint,
                HasValueTypeConstraint = x.HasValueTypeConstraint,
                HasConstructorConstraint = x.HasConstructorConstraint,
                IsOnMethod = false
            });

        var methodTypeParameters = methodSymbol.TypeParameters.Select(x => new TypeParameterDefinition
        {
            Name = x.Name,
            HasReferenceTypeConstraint = x.HasReferenceTypeConstraint,
            HasUnmanagedTypeConstraint = x.HasUnmanagedTypeConstraint,
            HasValueTypeConstraint = x.HasValueTypeConstraint,
            HasConstructorConstraint = x.HasConstructorConstraint,
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
            ContainingTypeFullName =
                methodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            IsPrivate = methodSymbol.DeclaredAccessibility != Accessibility.Public ||
                        methodSymbol.ContainingType.DeclaredAccessibility != Accessibility.Public,
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
                if (i == str.Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }

    // private static void Log(string info)
    // {
    //     typeof(BinaryReader).Assembly.GetExportedTypes().First(x => x.Name == "File")
    //         .GetMethods(BindingFlags.Public | BindingFlags.Static)
    //         .First(x => x.Name == "WriteAllText" && x.GetParameters().Length == 2)!
    //         .Invoke(null, [$"C:\\CMI-GitHub\\flamui\\SGLogs\\{Guid.NewGuid()}.txt", info]);
    // }
}

public class TransformResult
{
    public IMethodSymbol MethodSymbol;
    public InvocationExpressionSyntax InvocationExpressionSyntax;
    public GeneratorSyntaxContext GeneratorSyntaxContext;

    public TransformResult(IMethodSymbol methodSymbol, InvocationExpressionSyntax invocationExpressionSyntax,
        GeneratorSyntaxContext generatorSyntaxContext)
    {
        MethodSymbol = methodSymbol;
        InvocationExpressionSyntax = invocationExpressionSyntax;
        GeneratorSyntaxContext = generatorSyntaxContext;
    }
}