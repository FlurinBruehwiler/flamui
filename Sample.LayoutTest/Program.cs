using Flamui;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<LayoutTest>("Sample.LayoutTest", new WindowOptions());

app.Run();

public class LayoutTest : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        using (ui.Div().Color(C.Red5).Center())
        {
            using (ui.Div().Color(C.Green6).Shrink().Padding(10))
            {
                ui.Text("Oh hi mark");
            }
        }
    }
}
