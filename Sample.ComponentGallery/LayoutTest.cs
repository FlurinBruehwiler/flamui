using Flamui;

namespace Sample.ComponentGallery;

public class LayoutTest : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        using (ui.Div().Color(C.Red600).Dir(Dir.Horizontal).ShrinkWidth().Height(200).XAlign(XAlign.Center))
        {
            using (ui.Div().Height(100).Width(200).Color(C.Blue400))
            {

            }
            // ui.Text("Test");
        }
    }
}

