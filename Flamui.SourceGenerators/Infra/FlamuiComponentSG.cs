using Microsoft.CodeAnalysis;

namespace Flamui.SourceGenerators.Infra;

public readonly record struct MethodSignature
{
    public readonly IMethodSymbol MethodSymbol;
    public readonly string Name;
    public readonly string ReturnTypeFullName;

    public readonly EquatableArray<ParameterDefinition> Parameters;

    public MethodSignature(List<ParameterDefinition> parameters, IMethodSymbol methodSymbol, string name)
    {
        MethodSymbol = methodSymbol;
        Name = name;
        Parameters = new EquatableArray<ParameterDefinition>(parameters.ToArray());
    }
}

public readonly record struct ParameterDefinition
{
    public readonly string Name;
    public readonly bool Mandatory;
    public readonly string FullTypename;
    public readonly bool IsRef;

    public ParameterDefinition(string name, string fullTypename)
    {
        Name = name;
        Mandatory = false;
        IsRef = false;
        FullTypename = fullTypename;
    }
}
