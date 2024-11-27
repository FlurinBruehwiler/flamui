using Flamui;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<LayoutTest>("Sample.LayoutTest", new WindowOptions());

app.Run();

public class LayoutTest : FlamuiComponent
{
    private float rotation = 0;

    public override void Build(Ui ui)
    {
        using (ui.Div().Color(C.Yellow5).Padding(10).Gap(10))
        {
            using (ui.Div().Color(C.Blue5).Width(300).Gap(10).ScrollVertical(overlay:true))
            {
                for (var i = 0; i < 50; i++)
                {
                    using (ui.Div(i.ToString()).ShrinkHeight().Padding(3).Color(C.Green4).CrossAlign(XAlign.FlexEnd))
                    {
                        ui.Text(i.ToString());
                    }
                }
            }

            rotation += 1f;

            using (ui.Div().Rotation(rotation).Width(100).Height(100).Color(C.Green6).Padding(20).Clip())
            {
                using (ui.Div().Color(C.Blue6).Width(1000))
                {

                }
            }
        }
    }
}
