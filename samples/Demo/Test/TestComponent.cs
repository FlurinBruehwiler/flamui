using SkiaSharp;

namespace Demo.Test;

public class TestComponent : UiComponent
{
    public List<int> Items { get; set; } = Enumerable.Range(0, 100).ToList();

    public override Div Render()
    {
        return new Div
        {
            new Div()
                .Height(50, SizeKind.Percentage)
                .Dir(Dir.Row)
                .Items(Items.Select(x => new Div()
                    .Gap(5)
                    .Color(GetRandomColor(x)))),
            new Div().Color(GetRandomColor(20))
        }.MAlign(MAlign.Center).XAlign(XAlign.FlexStart);
    }

    private ColorDefinition GetRandomColor(int seed)
    {
        var rand = new Random(seed);
        return new ColorDefinition(rand.Next(250), rand.Next(250), rand.Next(250), 255);
    }
}