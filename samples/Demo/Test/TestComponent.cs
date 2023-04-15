using SkiaSharp;

namespace Demo.Test;

public class TestComponent : UiComponent
{
    private readonly Random rand = new();
    private ColorDefinition _color;

    public TestComponent()
    {
        _color = GetRandomColor();
    }

    public override Div Render()
    {
        return new Div
        {
            new Div
                {
                    new Txt().Content("TesttestI")
                            .Size(40)
                            .VAlign(TextAlign.Start)
                            .HAlign(TextAlign.End)
                }.Color(_color)
                .Width(50, SizeKind.Percentage)
                .Height(50, SizeKind.Percentage)
                .MAlign(MAlign.Center)
                .XAlign(XAlign.Center)
                .OnClick(() => { _color = GetRandomColor(); })
        }.MAlign(MAlign.Center)
            .XAlign(XAlign.Center);
    }

    private ColorDefinition GetRandomColor()
    {
        return new ColorDefinition(rand.Next(250), rand.Next(250), rand.Next(250), 255);
    }
}