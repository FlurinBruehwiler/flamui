namespace Flamui.SourceGenerators;

public readonly record struct ComponentParameters
{
    public readonly string Name;
    public readonly EquatableArray<ComponentParameter> Parameters;

    public ComponentParameters(string name, List<ComponentParameter> parameters)
    {
        Name = name;
        Parameters = new EquatableArray<ComponentParameter>(parameters.ToArray());
    }
}

public readonly record struct ComponentParameter
{
    public readonly string Name;
    public readonly bool Mandatory;
    public readonly string Type;

    public ComponentParameter(string name, string type, bool mandatory)
    {
        Name = name;
        Mandatory = mandatory;
        Type = type;
    }
}
