using Flamui;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<LayoutTest>("Sample.ComponentGallery", new WindowOptions());

app.Run();

public class LayoutTest : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        using (ui.Div().Color(C.Green6).Height(100).Width(100))
        {
        }

        using (ui.Div().Color(C.Green5))
        {
            using (ui.Div().Color(C.Blue4))
            {

            }
        }
    }
}
