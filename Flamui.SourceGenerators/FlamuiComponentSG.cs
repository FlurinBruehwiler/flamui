namespace Flamui.SourceGenerators;

public readonly record struct FlamuiComponentSg
{
    public readonly string ComponentName;
    public readonly string ComponentNamespace;
    public readonly string ComponentFullName;
    public readonly EquatableArray<ComponentParameter> Parameters;

    public FlamuiComponentSg(string componentName, string componentNamespace, string componentFullName, List<ComponentParameter> parameters)
    {
        ComponentName = componentName;
        ComponentNamespace = componentNamespace;
        ComponentFullName = componentFullName;
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
