using SkiaSharp;

namespace Demo.Test;

public class TestComponent : UiComponent
{
    public List<int> Items { get; set; } = Enumerable.Range(0, 100).ToList();

    private readonly Random rand = new Random();
    
    public override Div Render()
    {
        return new Div
        {
            new Div().Color(GetRandomColor(20))
                .Items(Items.Select(x => new Div()
                    .Color(GetRandomColor(x))
                    .Dir(Dir.Row)
                    .Gap(10)
                    .Items(Items.Select(y => new Div()
                        .Color(GetRandomColor(y * x))))
                ))
        }.MAlign(MAlign.Center).XAlign(XAlign.FlexStart);
    }

    private ColorDefinition GetRandomColor(int seed)
    {
        return new ColorDefinition(rand.Next(250), rand.Next(250), rand.Next(250), 255);
    }
}