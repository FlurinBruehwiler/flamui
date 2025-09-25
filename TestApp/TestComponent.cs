using System.Diagnostics;
using Flamui;
using Flamui.Components;

namespace Sample.LayoutTest;

public static class TestComponent
{
    private static long lastFrameTimeStamp;
    private static bool hi;

    // public static Bitmap bitmap = Bitmap.LoadFromFile(@"C:\Users\FBR\Downloads\example-png.png"); //this is just an example, you should obviously not load this here!!!!!

    public static void Build(Ui ui, FlamuiWindowHost app)
    {
        ui.CascadingValues.TextColor = C.White;

        var tabBar = ui.GetTabBar(initialTab: 0);

        if (tabBar.TabItem("Controls"))
        {
            using (tabBar.Body.Enter())
            {
                Tab1(ui, app);
            }
        }

        if (tabBar.TabItem("Grid"))
        {
            using (tabBar.Body.Enter())
            {
                Tab3(ui);
            }
        }

        if (tabBar.TabItem("Shadow"))
        {
            using (tabBar.Body.Enter())
            {
                using (ui.Rect().Color(177, 221, 234).Padding(50))
                {
                    using (ui.Rect().Width(100).Height(100).Color(173, 216, 230)
                               .ShadowColor(C.Black.WithAlpha(50)).Rounded(16)
                               .DropShadow(40, 0, x: -6, y: 6))
                    {

                    }
                }
            }
        }

        if (tabBar.TabItem("Layout"))
        {
            using (tabBar.Body.Enter())
            {
                using (ui.Rect().Padding(10).Gap(10))
                {
                    using (var rect = ui.Rect().Width(100).Height(100).Color(C.Green7).Rounded(10))
                    {
                        if (rect.IsHovered)
                        {
                            rect.Color(C.Blue7);
                        }

                        if (ui.Tree.IsKeyDown(Key.A))
                        {
                            rect.Color(C.Red7);
                        }
                    }
                }
            }
        }
    }

    private static void Tab3(Ui ui)
    {
        //todo make resize work in reusable component....

        using (ui.Rect().Color(20, 20, 20).Padding(10).Gap(10).ScrollVertical())
        {
            using (var grid = ui.Grid().Border(2, new ColorDefinition(47, 47, 47)).Gap(10))
            {
                //could be better I think
                var columns = ui.GetObj<float[]>(() => [100, 100, 100]);

                grid.Resizable(columns);

                foreach (var column in columns)
                {
                    grid.DefineColumn(column, fractional: true);
                }

                //5 rows
                for (int i = 0; i < 30; i++)
                {
                    //3 columns
                    for (int j = 0; j < 3; j++)
                    {
                        using (var cell = ui.Rect().Height(20).Padding(2).Focusable().Color(C.Transparent).Rounded(0))
                        {
                            if (cell.HasFocusWithin)
                            {
                                cell.Border(2, ColorPalette.SelectedColor);
                            }

                            if (i % 2 == 0)
                            {
                                cell.Color(26, 26, 26);
                            }

                            ref string t = ref ui.GetString($"{i} Cell");
                            ui.Input(ref t, cell.HasFocusWithin);
                        }
                    }
                }
            }
        }
    }

    private static void Tab2(Ui ui)
    {
        using (ui.Rect().Padding(10).ScrollVertical(overlay: true).Clip())
        {
            for (int i = 0; i < 50; i++)
            {
                using var _ = ui.CreateIdScope(i);
                using (var rect = ui.Rect().ShrinkHeight().PaddingHorizontal(5))
                {
                    if (i % 2 == 0)
                    {
                        rect.Color(ColorPalette.AccentColor);
                    }
                    else
                    {
                        rect.Color(ColorPalette.BorderColor);
                    }

                    ui.Text(i.ToArenaString());
                }
            }
        }
    }

    private static void Tab1(Ui ui, FlamuiWindowHost app)
    {
        using (ui.Rect().Color(C.Gray6).Padding(10).Rounded(5).Margin(10).Gap(10).ScrollVertical())
        {
            ref float sliderValue = ref ui.Get(0f);
            ui.Slider(0, 1, ref sliderValue);

            ref string selectedOption = ref ui.GetString("John");

            Span<string> items = ["John", "Albert", "Div", "Size"];
            ui.DropDown(items, ref selectedOption);

            //
            ref string input = ref ui.GetString("");

            ui.StyledInput(ref input);
            //
            ref bool checkboxValue = ref ui.Get(true);

            ui.Checkbox(ref checkboxValue);
            //
            var fps = 1000 / Stopwatch.GetElapsedTime(lastFrameTimeStamp).TotalMilliseconds;
            ui.Text($"{((int)fps).ToArenaString()}fps");
            lastFrameTimeStamp = Stopwatch.GetTimestamp();

            if (ui.Button("Create window 7"))
            {
                app.CreateWindow("Anita", (ui2) => Build(ui2, app));
            }

            // --------------- custom popup -----------------
            var popup = ui.GetPopup();

            if (popup.Visible)
            {
                using (popup.Body.Enter())
                {
                    using (ui.Rect().Rounded(5).Color(ColorPalette.BackgroundColor).Margin(51).BlockHit().Padding(10).Blur(10).Border(2, ColorPalette.BorderColor))
                    {
                        ui.Text("My Popup Text"); //this text will be displayed within the popup :)
                    }
                }
            }

            // with the current architecture, we need to call this after popup.Body.Enter(), this is unfortunate, see comment below for the proper solution
            if (ui.Button("Open Popup 99", primary: true))
            {
                popup.Visible = true;
            }

            // ---------------- confirmation popup --------------------
            var confirmPopup = ui.GetConfirmationPopup("Confirm Exit", "Are you sure you want to exit?");

            if (ui.Button("Show Confirmation Popup"))
            {
                confirmPopup.Show();
            }

            if (confirmPopup.Result == ConfirmationPopupResult.Ok)
            {
                Console.WriteLine("Ok");
            }
            else if (confirmPopup.Result == ConfirmationPopupResult.Cancel)
            {
                Console.WriteLine("Cancel");
            }


            //--------------- radio button group ------------------

            ref int selectedRadioButton = ref ui.Get(0);
            for (int i = 0; i < 10; i++)
            {
                using var _ = ui.CreateIdScope(i);
                ui.RadioButton(ref selectedRadioButton, i);
            }
        }
    }
}