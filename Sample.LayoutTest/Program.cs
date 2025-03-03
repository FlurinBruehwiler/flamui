using Flamui;

/*
 * Todo
 * - Scroll
 * - Fix Border
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
        using (ui.Div().Width(100))
        {
            ui.SvgImage(@"C:\Users\bruhw\Downloads\test_tvg_file.tvg");
        }
    }
}