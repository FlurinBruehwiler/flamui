global using static ImSharpUISample.Ui;
global using static SDL2.SDL;
using ImSharpUISample.ToolWindows;


namespace ImSharpUISample;

public class BddGraph
{
    public void Build()
    {
        DivStart().Dir(Dir.Horizontal);

            StartComponent<Sidebar>(out var l).Side(SidebarSide.Left);
                l.ToolWindow<FilePicker>("folder");
                l.ToolWindow<History>("history");
                l.ToolWindow<DetailView>("account_tree");
            EndComponent<Sidebar>();

            GetComponent<NodeGraph>().Build();

            StartComponent<Sidebar>(out var r).Side(SidebarSide.Right);
                r.ToolWindow<DetailView>("info");
                r.ToolWindow<DetailView>("shelves");
            EndComponent<Sidebar>();

        DivEnd();
    }
}

public record struct ToolWindowDefinition(string Path, Type WindowComponent);


public enum SidebarSide
{
    Left,
    Right
}
