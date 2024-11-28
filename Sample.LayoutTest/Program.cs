using Flamui;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<LayoutTest>("Sample.LayoutTest", new WindowOptions());

app.Run();

public class LayoutTest : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        using (ui.Div().Color(C.Yellow5).Padding(10).Gap(10))
        {
            // using (ui.Div().Color(C.Blue5).Width(300).Gap(10).ScrollVertical(overlay:true))
            // {
            //     for (var i = 0; i < 50; i++)
            //     {
            //         using (ui.Div(i.ToString()).ShrinkHeight().Padding(3).Color(C.Green4).CrossAlign(XAlign.FlexEnd))
            //         {
            //             ui.Text(i.ToString());
            //         }
            //     }
            // }
            using (ui.Div().Color(C.Blue5).Width(300).ScrollHorizontal().ScrollVertical().Direction(Dir.Horizontal))
            {
                using (ui.Div().Width(150).Direction(Dir.Vertical).Gap(10))
                {
                    for (var i = 0; i < 50; i++)
                    {
                        using (ui.Div(i.ToString()).Padding(3).Color(C.Green4).Height(20))
                        {
                            // ui.Text(i.ToString());
                        }
                    }
                }

                using (ui.Div().Direction(Dir.Horizontal).ShrinkWidth().Gap(10))
                {
                    for (var i = 0; i < 50; i++)
                    {
                        using (ui.Div(i.ToString()).Padding(3).Color(C.Green4).Width(20))
                        {
                            // ui.Text(i.ToString());
                        }
                    }
                }

            }
        }
    }
}
