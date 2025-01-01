using Flamui;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<LayoutTest>("Sample.LayoutTest", new FlamuiWindowOptions());

app.Run();

public class LayoutTest : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        using (ui.Div().Color(C.Yellow5).Padding(10).Gap(10))
        {
            using (ui.Div().Color(C.Blue5))
            {
                
            }

            using (ui.Div().Color(C.Blue5))
            {

            }
        }
    }
}
