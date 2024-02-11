using Flamui;

namespace Sample.ComponentGallery;

public class DebugWindow : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        ui.DivStart().Padding(10).Gap(10);

            ui.DivStart().ShrinkHeight().Color(100, 0, 0).Padding(10);

                ui.DivStart().Height(100).Width(100).Color(C.Blue);
                ui.DivEnd();

            ui.DivEnd();

            ui.DivStart().Height(100).Width(100).Color(C.Text);
            ui.DivEnd();

        ui.DivEnd();
    }
}
