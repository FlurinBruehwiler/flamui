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
        using (ui.Div().Padding(10).Dir(Dir.Horizontal).Gap(10))
        {
            using (ui.Div().Padding(10).Color(ColorPalette.BackgroundColor).Gap(0).Scroll())
            {
                for (int i = 1; i < 50; i++)
                {
                    using (var div = ui.Div(Ui.S(i)).Height(20).Border(0, ColorPalette.BorderColor))
                    {
                        if (div.IsHovered)
                        {
                            div.Color(C.Black / 8);
                        }
                        else
                        {
                            div.Color(C.Transparent);
                        }

                        ui.Text(Ui.S(i));
                    }
                }
            }

            using (ui.Div().Padding(10).Color(ColorPalette.BackgroundColor).Gap(0).Scroll())
            {
                for (int i = 1; i < 50; i++)
                {
                    using (var div = ui.Div(Ui.S(i)).Height(20).Border(0, ColorPalette.BorderColor))
                    {
                        if (div.IsHovered)
                        {
                            div.Color(C.Black / 8);
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
}
