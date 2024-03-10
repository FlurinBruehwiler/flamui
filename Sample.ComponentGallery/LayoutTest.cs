using Flamui;

namespace Sample.ComponentGallery;

public class LayoutTest : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        using (ui.Div().Color(C.Red600).ShrinkWidth().Height(200).XAlign(XAlign.Center).Padding(10).ShrinkHeight().Dir(Dir.Horizontal).Gap(10))
        {
            using (ui.Div().Dir(Dir.Horizontal))
            {
                using (ui.Div().Height(100).Width(200).Color(C.Blue400))
                {

                }

                using (ui.Div().Height(100).Width(200).Color(C.Yellow500))
                {

                }
            }

            // ui.Text("Test");
        }
    }
}

