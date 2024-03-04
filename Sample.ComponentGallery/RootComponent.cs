using Flamui;
using Flamui.Components;

namespace Sample.ComponentGallery;

public class RootComponent(FlamuiApp flamuiApp) : FlamuiComponent
{
    private string _selectedOption = "Mark";
    private bool _checkboxState;
    private string _inputText = string.Empty;

    public override void Build(Ui ui)
    {
        using (ui.Div().Padding(0).Color(ColorPalette.BackgroundColor).Gap(10).Scroll())
        {
            // using (ui.Div().Width(150).ShrinkHeight())
            // {
            //     var dd = ui.CreateDropDown(_selectedOption);
            //     dd.Component.Option("Mark");
            //     dd.Component.Option("Tommy");
            //     dd.Component.Option("Johnny");
            //     dd.Component.Option("Bonny");
            //     dd.Build(out _selectedOption);
            // }
            //
            // ui.Checkbox(ref _checkboxState);
            //
            // using (ui.Div().Width(200).ShrinkHeight())
            // {
            //     ui.StyledInput(ref _inputText);
            // }
            //
            // ui.Button("Click be");
            // ui.Button("Click be (primary)", primary:true, width: 150);
            //
            using (ui.CascadingValue("oh hi"))
            {
                ui.CreateLayoutTest();
            }

            // if (ui.Button("Open new Window", width:150))
            // {
            //     flamuiApp.CreateWindow<LayoutTest>("LayoutTest");
            // }

            for (int i = 1; i < 50; i++)
            {
                using (var div = ui.Div(Ui.S(i)).Height(12).Border(0, ColorPalette.BorderColor))
                {
                    if (div.IsHovered)
                    {
                        // div.Color(C.Amber600);
                    }
                    else
                    {
                        div.Color(C.Transparent);
                    }

                    ui.Text(Ui.S(i));
                }
            }
        }
    }
}
