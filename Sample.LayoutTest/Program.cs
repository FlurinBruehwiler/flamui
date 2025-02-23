using Flamui;
using Flamui.Components;

/*
 * Todo
 * - non ascii text
 * - Scroll
 * - Fix Border
 * - Only rerender when changed
 */

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<LayoutTest>("Sample.LayoutTest");

app.Run();

public class LayoutTest(FlamuiApp app) : FlamuiComponent
{
    private ColorDefinition cc = new(43, 45, 48);
    private ColorDefinition c2 = new(30, 31, 34);
    private ColorDefinition c3 = new(75, 76, 79);

    private const string loremIpsum =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

    private string input2 = "anita max wynn";
    private string selectedOption = "";

    public override void Build(Ui ui)
    {
        ui.CascadingValues.TextColor = new ColorDefinition(188, 190, 196);
        ui.CascadingValues.TextSize = 17;

        using (ui.Div().Color(cc).Padding(10).Gap(10))
        {
            using (ui.Div().Color(c2).Rounded(2).Border(1, ColorPalette.BorderColor).Padding(20).Direction(Dir.Vertical).Gap(10).Clip())
            {
                using (ui.Div().Color(c2).Rounded(2).Border(1, ColorPalette.BorderColor).Padding(20).Direction(Dir.Vertical).Gap(10).Clip().ShrinkHeight())
                {
                    ui.StyledInput(ref input2);

                    // var dropdown = ui.CreateDropDown(selectedOption);
                    // dropdown.Component.Option("Hi");
                    // dropdown.Component.Option("Anita");
                    // dropdown.Component.Option("Max");
                    // dropdown.Component.Option("Wynn");
                    // dropdown.Build(out selectedOption);
                }
            }
        }
    }

}