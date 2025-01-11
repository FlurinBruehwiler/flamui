using Flamui;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<LayoutTest>("Sample.LayoutTest", new FlamuiWindowOptions());

app.Run();

public class LayoutTest : FlamuiComponent
{
    private ColorDefinition c1 = new(43, 45, 48);
    private ColorDefinition c2 = new(30, 31, 34);
    private ColorDefinition c3 = new(75, 76, 79);

    private const string loremIpsum =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

    public override void Build(Ui ui)
    {
        ui.CascadingValues.Font = ui.FontManager.GetFont("Jetbrains Mono");
        ui.CascadingValues.TextColor = new ColorDefinition(188, 190, 196);

        using (ui.Div().Color(c1).Padding(10).Gap(10))
        {
            using (var innerDiv = ui.Div().Color(c2).Rounded(20).Border(3, c3).Padding(20).Direction(Dir.Horizontal))
            {
                if (innerDiv.IsHovered)
                {
                    innerDiv.Color(c3);
                }

                ui.Text(loremIpsum);
            }
        }
    }
}