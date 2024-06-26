using Flamui;
using Flamui.Components;

namespace Sample.ComponentGallery;

public class RootComponent(FlamuiApp flamuiApp) : FlamuiComponent
{
    private string _selectedOption = "Mark";
    private string _inputText = string.Empty;
    private string _inputNumeric = string.Empty;

    private bool _checkboxState;

    private int _tabIndex;

    private int _counter = 0;

    public override void Build(Ui ui)
    {
        using (var button = ui.Div().Width(30).Height(30))
        {
            if (button.IsClicked)
            {
                _counter++;
            }

            ui.Text(_counter.ToString());
        }

        using (ui.Div().Color(ColorPalette.BackgroundColor))
        {
            using (ui.Div().Direction(Dir.Horizontal).Padding(10).Gap(10).ShrinkHeight())
            {
                if (ui.Button("Main controls"))
                {
                    _tabIndex = 0;
                }

                if (ui.Button("Scroll Test"))
                {
                    _tabIndex = 1;
                }
            }

            if (_tabIndex == 1)
            {
                ScrollTest(ui);
            }
            else if(_tabIndex == 0)
            {
                Other(ui);
            }
        }
    }

    private void Other(Ui ui)
    {
        using (ui.Div().Padding(10).Gap(10))
        {
            var dd = ui.CreateDropDown(_selectedOption);
            dd.Component.Option("Mark");
            dd.Component.Option("Johnny");
            dd.Component.Option("Frank");
            dd.Component.Option("Linus");
            dd.Build(out _selectedOption);

            ui.StyledInput(ref _inputText);

            ui.StyledInput(ref _inputNumeric, inputType: InputType.Text);

            ui.Checkbox(ref _checkboxState);

            using (ui.Div().Height(20))
            {
                ui.Text("Oh hi mark").Color(C.Red7);
            }

            if (ui.Button("Create new Window"))
            {
                flamuiApp.CreateWindow<RootComponent>("Second window");
            }
        }
    }

    private void ScrollTest(Ui ui)
    {
        using (ui.Div().Padding(10).Direction(Dir.Horizontal).Gap(10))
        {
            using (ui.Div().Padding(10).Color(C.Black / 9).Gap(0).ScrollVertical())
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

            using (ui.Div().Padding(10).Color(C.Black / 9).Gap(0).ScrollVertical())
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
