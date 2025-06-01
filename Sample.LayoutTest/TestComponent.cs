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

        using (ui.Rect().Color(C.Gray6).Padding(10).Rounded(10).Margin(10).Gap(10))
        {
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

            ref string input = ref ui.GetString("");

            ui.StyledInput(ref input);

            ref bool checkboxValue = ref ui.Get(true);

            ui.Checkbox(ref checkboxValue);

            var fps = 1000 / Stopwatch.GetElapsedTime(lastFrameTimeStamp).TotalMilliseconds;
            ui.Text($"{((int)fps).ToArenaString()}fps");
            lastFrameTimeStamp = Stopwatch.GetTimestamp();

            if (ui.Button("Create window"))
            {
                app.CreateWindow("Anita", (ui2) => Build(ui2, app));
            }

            var popup = ui.GetPopup();

            if (popup.Visible)
            {
                using (popup.Body.Enter())
                {
                    using (
                        ui.Rect().Rounded(10).Color(C.Blue3).Margin(51).BlockHit())
                    {
                        ui.Text("My Popup Text"); //this text will be displayed within the popup :)
                    }
                }
            }

            //with the current architecture, we need to call this after popup.Body.Enter(), this is unfortunate, see comment below for the proper solution
            if (ui.Button("Open Popup", primary: true))
            {
                popup.Visible = true;
            }
        }
    }


}