using Modern.WindowKit.Input;
using SkiaSharp;

namespace Demo.Test;

public class TestComponent : UiComponent
{
    private readonly Random rand = new();
    private ColorDefinition _color;
    private bool _isActive;
    private string _text = string.Empty;

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
                    new Txt().Content(_isActive ? "Active" : "Inactive")
                            .Size(40)
                            .VAlign(TextAlign.Start)
                            .HAlign(TextAlign.End),
                    new Txt().Content(_text)
                    .Size(40)
     
                }.Color(_color)
                .Width(50, SizeKind.Percentage)
                .Height(50, SizeKind.Percentage)
                .OnActive(() => _isActive = true)
                .OnInactive(() => _isActive = false)
                .OnClick(() => { _color = GetRandomColor(); })
                .OnKeyDown(key =>
                {
                    if (key == Key.Back)
                    {
                        _text = _text.Remove(_text.Length - 1);
                        return;
                    }
                    
                    _text += key.ToString();
                })
                .Padding(10)
        }.MAlign(MAlign.Center)
            .XAlign(XAlign.Center);
    }

    private ColorDefinition GetRandomColor()
    {
        return new ColorDefinition(rand.Next(250), rand.Next(250), rand.Next(250), 255);
    }
}