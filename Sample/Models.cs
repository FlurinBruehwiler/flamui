namespace Sample;

public record struct ToolWindowDefinition(string Path, Type WindowComponent);


public enum SidebarSide
{
    Left,
    Right
}
