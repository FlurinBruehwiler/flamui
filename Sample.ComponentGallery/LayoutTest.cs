using Flamui;

namespace Sample.ComponentGallery;

public class LayoutTest : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        using (ui.Div().ShrinkHeight().Color(C.Selected))
        {
            using (ui.Div().Height(50).Width(50).Color(C.Text))
            {

            }
        }

        using (ui.Div().Color(C.Blue).HeightFraction(90))
        {

        }
    }
}
