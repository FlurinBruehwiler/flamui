using Flamui;
using Flamui.Components.DebugTools;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<LayoutTest>("Sample.ComponentGallery", new WindowOptions());

app.Run();

public class LayoutTest : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        using (ui.Div().Color(C.Red600).Dir(Dir.Horizontal).Padding(10))
        {
        }
    }
}
