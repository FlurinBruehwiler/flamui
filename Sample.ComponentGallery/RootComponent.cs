using Flamui;
using Flamui.Components;

namespace Sample.ComponentGallery;

public class RootComponent : FlamuiComponent
{
    private string _selectedOption = "Mark";
    private bool _checkboxState;

    public override void Build(Ui ui)
    {
        ui.DivStart().Padding(10).Color(C.Background);

            ui.DivStart().Width(150).ShrinkHeight();

                var dd = ui.CreateDropDown(_selectedOption);
                dd.Component.Option("Mark");
                dd.Component.Option("Tommy");
                dd.Component.Option("Johnny");
                dd.Component.Option("Bonny");
                dd.Build(out _selectedOption);

            ui.DivEnd();

            ui.Checkbox(ref _checkboxState);

        ui.DivEnd();
    }
}
