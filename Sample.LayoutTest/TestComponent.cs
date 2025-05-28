using Flamui;
using Flamui.Components;
using ZLinq;

namespace Sample.LayoutTest;

public static class TestComponent
{
    public static void Build(Ui ui, FlamuiWindowHost app)
    {
        ui.CascadingValues.TextColor = C.White;

        var b = true;

        using (ui.Rect().Color(C.Gray6).Padding(10).Rounded(10).Margin(10).Gap(10))
        {
            ref string selectedOption = ref ui.GetString("John");

            Span<string> items = ["John", "Albert", "Div", "Size"];

            var y = items.AsValueEnumerable().Select(x => x);

            ui.DropDown(items, ref selectedOption);

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

            var popup = ui.GetPopup();


            if (popup.Visible)
            {
                using (popup.Body.Enter())
                {
                    using (

                        ui.Rect().Rounded(10)
                               .Color(C.Blue3).Margin(50)
                               .BlockHit())
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