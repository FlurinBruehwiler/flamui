using Flamui;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<LayoutTest>("Sample.LayoutTest", new WindowOptions());

app.Run();

public class LayoutTest : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        using (ui.Div().Direction(Dir.Horizontal).Color(C.Yellow5).Padding(10).Gap(10))
        {
            using (ui.Div().Color(C.Blue4).Rounded(10).Padding(0).PaddingLeft(40))
            {
                using (ui.Div().Color(C.Fuchsia7))
                {

                }
            }

            using (ui.Div().Color(C.Green6).Width(100).Rounded(10))
            {
            }
        }
    }
}
