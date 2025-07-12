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

        var tabBar = ui.GetTabBar();

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
                ui.Text("Tab 2");
            }
        }
    }

    private static void Tab1(Ui ui, FlamuiWindowHost app)
    {
        using (ui.Rect().Color(C.Gray6).Padding(10).Rounded(10).Margin(10).Gap(10))
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