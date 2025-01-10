using Flamui;
using Flamui.UiElements;

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
    private const string loremIpsum =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

    private ColorDefinition c1 = new(43, 45, 48);
    private ColorDefinition c2 = new(30, 31, 34);
    private ColorDefinition c3 = new(75, 76, 79);

    public override void Build(Ui ui)
    {
        using (ui.Div().Color(c1))
        {
            using (ui.Div().Color(c2).Margin(10))
            {
               // using (ui.Div().Height(18))
               // {
               //     for (byte i = 0; i < 9; i++)
               //     {
               //         using (ui.Div(i.ToString()).Height(1).Color(C.Red9 / (byte)(i + 1)))
               //         {
               //
               //         }
               //     }
               //
               //     for (byte i = 0; i < 9; i++)
               //     {
               //         using (ui.Div(i.ToString()).Height(1).Color(C.Blue9 / (byte)(i + 1)))
               //         {
               //
               //         }
               //     }
               // }
            }
        }
    }
}