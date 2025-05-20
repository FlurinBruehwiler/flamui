using Flamui;
using Flamui.Components;

namespace Sample.LayoutTest;

public class TestComponent
{
    public static string input = string.Empty;

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

        using (ui.Div().Color(C.Gray6).Padding(10).Rounded(10).Margin(10).Gap(10))
        {
            // for (int i = 0; i < 10; i++)
            // {
            //     using (var div = ui.Div(i.ToString()).Height(50).Color(C.Black))
            //     {
            //         if (div.IsHovered)
            //         {
            //             div.Color(C.Red8);
            //         }
            //     }
            // }

            // var dd = ui.CreateDropDown(selectedOption);
            // dd.Component.Option("John");
            // dd.Component.Option("Albert");
            // dd.Component.Option("Div");
            // dd.Component.Option("Size");
            // dd.Build(out selectedOption);
            //
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