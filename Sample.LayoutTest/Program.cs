using Flamui;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<LayoutTest>("Sample.LayoutTest", new WindowOptions());

app.Run();

public class LayoutTest : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        using (ui.Div().Direction(Dir.Horizontal).Color(C.Yellow5).Center().Gap(10))
        {
            using (ui.Div().Color(C.Blue6).Width(100).Height(100).MarginRight(10).Rounded(10))
            {

            }

            using (ui.Div().Color(C.Blue6).Width(100).Height(100).Border(2, C.Black))
            {
                using (ui.Div().Absolute(left: 25, top: -25).Width(50).Height(50).Color(C.Red5))
                {
                    
                }
            }
        }
    }
}
