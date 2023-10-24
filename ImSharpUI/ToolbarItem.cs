namespace ImSharpUISample;

public class ToolbarItem<T>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Icons { get; set; }
    public Type Type { get; set; }


    public ToolbarItem(string name, string icons)
    {
        Name = name;
        Icons = icons;
        Type = typeof(T);
        Id = $"bdd-graph-toolbar-{Type.Name}";
    }
}
