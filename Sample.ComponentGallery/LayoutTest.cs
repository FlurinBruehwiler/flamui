using Flamui;
using Flamui.Components;

namespace Sample.ComponentGallery;

public class LayoutTest : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        using (ui.Div().Padding(0).Color(ColorPalette.BorderColor).Dir(Dir.Horizontal).ShrinkWidth())
        {
            using (ui.Div().Width(100).Color(C.Red600))
            {
                ui.Text("Test");
            }
        }
    }
}

