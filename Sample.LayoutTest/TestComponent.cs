using Flamui;
using Flamui.Components;

namespace Sample.LayoutTest;

public static class TestComponent
{
    public static void Build(Ui ui)
    {
        ui.CascadingValues.TextColor = C.White;

        // using (ui.Div().Margin(10).Color(C.Gray6).ScrollVertical().Clip().Padding(10).Rounded(10))
        // {
        //     foreach (var icon in icons)
        //     {
        //         ui.Text(icon, icon).Size(20);
        //         using (ui.Div(icon).Width(100).Height(100))
        //         {
        //             ui.SvgImage($"Icons/TVG/{icon}");
        //         }
        //     }
        // }

        // var res = UiExtensions.Button(ui, "hi");
        // res = UiExtensions.Button(ui, "hi");
        // res = UiExtensions.Button(ui, "hi");
        // res = UiExtensions.Button(ui, "hi");
        // res = UiExtensions.Button(ui, "hi");
        // res = UiExtensions.Button(ui, "hi");

        var b = true;
        // UiExtensions.Checkbox(ui, ref b);

        using (ui.Rect().Color(C.Gray6).Padding(10).Rounded(10).Margin(10).Gap(10))
        {
            ref string selectedOption = ref ui.GetString("John");

            ui.DropDown(["John", "Albert", "Div", "Size"], ref selectedOption);

            ref string input = ref ui.GetString("");

            ui.StyledInput(ref input);

            if (ui.Button("Create window"))
            {
                // app.CreateWindow("Anita", Build);
            }

            ui.Button("Press me!", primary: true);

            // var fps = (float)(1 / ui.Window.DeltaTime);
            // ui.Text($"{fps} fps");

            // var popup = GetPopup();
            //
            // using (popup.Enter())
            // {
            //     ui.Text("My Popup Text"); //this text will be displayed within the popup :)
            // }
        }
    }
}