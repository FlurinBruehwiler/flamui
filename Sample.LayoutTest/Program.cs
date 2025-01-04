using Flamui;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

// ArenaString x = "test";
// ArenaString x2 = "tes2";
// var y = x + x2;
// ArenaString z = $"abc {x} {14321}";
// var a = 12321.ToArenaString();

app.CreateWindow<LayoutTest>("Sample.LayoutTest", new FlamuiWindowOptions());

app.Run();

public class LayoutTest : FlamuiComponent
{
    private ColorDefinition c1 = new(43, 45, 48);
    private ColorDefinition c2 = new(30, 31, 34);
    private ColorDefinition c3 = new(75, 76, 79);

    public override void Build(Ui ui)
    {
        using (ui.Div().Color(c1).Padding(10).Gap(10))
        {
            using (ui.Div().Color(c2).Rounded(20).Border(3, c3).Padding(20))
            {
               ui.Text("Test Text").Color(c3);
            }

            using (ui.Div().Color(c2))
            {

            }
        }
    }
}
