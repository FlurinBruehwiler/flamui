using Flamui;
using Flamui.Components;

namespace Sample.LayoutTest;

public static class TestComponent
{
    public static void Build(Ui ui, FlamuiWindowHost app)
    {
        ui.CascadingValues.TextColor = C.White;

        var b = true;
        // UiExtensions.Checkbox(ui, ref b);

        using (ui.Rect().Color(C.Gray6).Padding(10).Rounded(10).Margin(10).Gap(10))
        {
            ref string selectedOption = ref ui.GetString("John");

            ui.DropDown(["John", "Albert", "Div", "Size"], ref selectedOption);

            ref string input = ref ui.GetString("");

            ui.StyledInput(ref input);

            ref bool checkboxValue = ref ui.Get(false);

            ui.Checkbox(ref checkboxValue);

            // using (ui.Rect().Width(50).Height(50))
            // {
            //     ui.SvgImage("Icons/TVG/expand_more.tvg");
            // }

            if (ui.Button("Create window"))
            {
                app.CreateWindow("Anita", (ui2) => Build(ui2, app));
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