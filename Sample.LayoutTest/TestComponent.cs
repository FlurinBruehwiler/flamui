using System.Diagnostics;
using Flamui;
using Flamui.Components;

namespace Sample.LayoutTest;

public static class TestComponent
{
    private static long lastFrameTimeStamp;

    public static void Build(Ui ui, FlamuiWindowHost app)
    {
        ui.CascadingValues.TextColor = C.White;

        var tabBar = ui.GetTabBar(initialTab: 2);

        if (tabBar.TabItem("Tab 1"))
        {
            using (tabBar.Body.Enter())
            {
                Tab1(ui, app);
            }
        }

        if (tabBar.TabItem("Tab 2"))
        {
            using (tabBar.Body.Enter())
            {
                Tab2(ui);
            }
        }

        if (tabBar.TabItem("Tab 3"))
        {
            using (tabBar.Body.Enter())
            {
                using (var grid = ui.Grid())
                {
                    for (int i = 0; i < 3; i++)
                    {
                        grid.DefineColumn(width: 10);
                    }

                    //10 rows
                    for (int i = 0; i < 10; i++)
                    {
                        //3 columns
                        for (int j = 0; j < 3; j++)
                        {
                            using (ui.Rect())
                            {

                            }

                            //you now want it so that the column has the same width across the rows, you also want an easy way to declare a border
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

            // for (int i = 0; i < 10; i++)
            // {
            //     using (var r = ui.Rect().Height(20).Color(C.Gray4))
            //     {
            //         ui.CreateIdScope(i);
            //
            //         if (r.IsHovered)
            //         {
            //             r.Color(C.Red5);
            //         }
            //     }
            // }

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
                    using (ui.Rect().Rounded(10).Color(ColorPalette.BackgroundColor).Margin(51).BlockHit().Padding(10))
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
             ui.RadioButton(ref selectedRadioButton, 0);
             ui.RadioButton(ref selectedRadioButton, 1);
             ui.RadioButton(ref selectedRadioButton, 2);

        }
    }
}