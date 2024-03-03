using Flamui;
using Flamui.Components;

namespace Sample.ComponentGallery;

public class LayoutTest : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        using (ui.Div().Color(ColorPalette.BorderColor).Dir(Dir.Horizontal))
        {
            for (var i = 0; i < 10; i++)
            {
                using (ui.Div(Ui.S(i)).Color(ColorPalette.TextColor))
                {

                }
            }
        }
    }
}

