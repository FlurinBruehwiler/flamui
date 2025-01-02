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

    public override void Build(Ui ui)
    {
        ui.Text("Test Text").Color(C.Red6);
        //
        // using (ui.Div().Color(c1).Padding(10).Gap(10))
        // {
        //     using (ui.Div().Color(c2).Rounded(20).Border(3, c3))
        //     {
        //
        //     }
        //
        //     using (ui.Div().Color(c2))
        //     {
        //
        //     }
        // }
    }
}
