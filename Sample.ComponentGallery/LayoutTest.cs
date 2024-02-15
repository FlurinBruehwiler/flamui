using Flamui;

namespace Sample.ComponentGallery;

public class LayoutTest : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        using (ui.Div().Color(C.Border).Dir(Dir.Horizontal))
        {
            for (int i = 0; i < 10; i++)
            {
                using (ui.Div(Ui.S(i)).Color(C.Text))
                {

                }
            }
        }
    }
}
