using Flamui;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<LayoutTest>("Sample.LayoutTest", new WindowOptions());

app.Run();

public class LayoutTest : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        using (ui.Div().Direction(Dir.Horizontal).Color(C.Yellow5).MainAlign(MAlign.Center).Gap(10).Padding(10))
        {
            using (ui.Div().Color(C.Blue4).Width(100).Height(100).MarginRight(30))
            {

            }

            using (ui.Div().Color(C.Blue5).Width(1000).Height(500).Border(2, C.Black).Rounded(5))
            {

            }

            using (ui.Div().Color(C.Blue6).Width(100).Height(100))
            {

            }
        }
    }
}
