using Flamui;

namespace Sample.ComponentGallery;

public class LayoutTest : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        using (ui.Div().Color(C.Red600).Height(200).Dir(Dir.Horizontal).Padding(10))
        {
            using (ui.Div().Width(48).Color(C.Green600))
            {

            }

            using (ui.Div().WidthFraction(50).Color(C.Blue600))
            {

            }
        }
    }
}

