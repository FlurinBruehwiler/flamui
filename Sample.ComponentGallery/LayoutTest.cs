using Flamui;
using Flamui.Components;

namespace Sample.ComponentGallery;

public class LayoutTest : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        using (ui.Div().Color(ColorPalette.BorderColor).Dir(Dir.Horizontal))
        {
            ui.Text("Oh hi mark");
        }
    }
}

