namespace Flamui.SourceGenerators;

public readonly record struct ComponentParameters
{
    public readonly string ComponentName;
    public readonly string ComponentNamespace;
    public readonly EquatableArray<ComponentParameter> Parameters;

    public ComponentParameters(string componentName, string componentNamespace, List<ComponentParameter> parameters)
    {
        ComponentName = componentName;
        ComponentNamespace = componentNamespace;
        Parameters = new EquatableArray<ComponentParameter>(parameters.ToArray());
    }
}

public readonly record struct ComponentParameter
{
    public readonly string Name;
    public readonly bool Mandatory;
    public readonly string Type;
    public readonly bool IsRef;

    public ComponentParameter(string name, string type, bool mandatory, bool isRef)
    {
        Name = name;
        Mandatory = mandatory;
        IsRef = isRef;
        Type = type;
    }
}
