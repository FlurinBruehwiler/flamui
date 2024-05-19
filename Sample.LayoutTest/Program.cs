using Flamui;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<LayoutTest>("Sample.LayoutTest", new WindowOptions());

app.Run();

public class LayoutTest : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        using (ui.Div().Direction(Dir.Horizontal))
        {
            using (ui.Div().Color(C.Blue4).ShrinkWidth())
            {
                using (ui.Div().Color(C.Amber5).Width(200))
                {
                    using (ui.Div().Color(C.Pink5).Width(100).Height(100))
                    {
                    }
                }
            }

            using (ui.Div().Color(C.Green6).Width(100))
            {
            }
        }
    }
}
