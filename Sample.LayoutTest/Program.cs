using Flamui;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<LayoutTest>("Sample.LayoutTest", new WindowOptions());

app.Run();

public class LayoutTest : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        using (ui.Div().Direction(Dir.Horizontal).Color(C.Yellow5))
        {
            using (ui.Div().Absolute().ShrinkWidth(150).ShrinkHeight(150).Color(C.Blue5))
            {
                using (ui.Div().Color(C.Green5))
                {

                }
            }
        }
    }
}
