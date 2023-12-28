using Flamui;
using Sample.ToolWindows;

namespace Sample;

public class RootComponent : FlamuiComponent
{
    public override void Build()
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
