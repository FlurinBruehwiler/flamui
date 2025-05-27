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

            var popup = GetPopup(ui);

            if (popup.Visible)
            {
                using (popup.Body.Enter())
                {
                    using (ui.Rect().Rounded(10).Color(C.Blue3).Margin(50).BlockHit())
                    {
                        ui.Text("My Popup Text"); //this text will be displayed within the popup :)
                    }
                }
            }

            //with the current architecture, we need to call this after popup.Body.Enter(), this is unfortunate, I'm not sure what the best way to solve this is, I'm also not sure how pangui does it.
            if (ui.Button("Open Popup", primary: true))
            {
                popup.Visible = true;
            }
        }
    }

    public static Popup GetPopup(Ui ui)
    {
        var popup = new Popup
        {
            Visible = ref ui.Get(false) //when setting this, we would only want it to take effect in the next frame....., this avoids all the problems with the order...
        };

        if (!popup.Visible)
            return popup;

        using (var backgorund = ui.Rect().SetParent(ui.Root).AbsoluteSize(0, 0).Center().BlockHit())
        {
            if (backgorund.IsClicked)
            {
                popup.Visible = false;
            }

            popup.Body = ui.CreateLayoutScope();

            return popup;
        }
    }
}

public ref struct Popup
{
    public ref bool Visible;
    public LayoutScope Body;
}