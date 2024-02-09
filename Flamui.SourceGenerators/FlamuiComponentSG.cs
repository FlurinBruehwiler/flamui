using Microsoft.CodeAnalysis;

namespace Flamui.SourceGenerators;

public readonly record struct FlamuiComponentSg
{
    public readonly INamedTypeSymbol Component;
    public readonly string FullName;

    public readonly EquatableArray<ComponentParameter> Parameters;

    public FlamuiComponentSg(List<ComponentParameter> parameters, INamedTypeSymbol component, string fullName)
    {
        Component = component;
        FullName = fullName;
        Parameters = new EquatableArray<ComponentParameter>(parameters.ToArray());
    }
}

public readonly record struct ComponentParameter
{
    public readonly string Name;
    public readonly bool Mandatory;
    public readonly string FullTypename;
    public readonly bool IsRef;

    public ComponentParameter(string name, string fullTypename, bool mandatory, bool isRef)
    {
        Name = name;
        Mandatory = mandatory;
        IsRef = isRef;
        FullTypename = fullTypename;
    }
}
