using System.Numerics;
using ImSharpUISample.UiElements;
using SkiaSharp;

namespace ImSharpUISample;

public class ConnectionLine : UiElement
{
    private Vector2 _start;
    private Vector2 _end;
    private UiElement? startElement;
    private UiElement? endElement;
    private Func<UiElement, Vector2>? startTransform;
    private Func<UiElement, Vector2>? endTransform;

    public override void CleanElement()
    {
        startElement = null;
        endElement = null;
        startTransform = null;
        endTransform = null;
    }

    public ConnectionLine From(Vector2 start)
    {
        _start = start;
        return this;
    }

    public ConnectionLine From(UiElement anchor, Func<UiElement, Vector2> transformFunction)
    {
        startElement = anchor;
        startTransform = transformFunction;
        return this;
    }

    public ConnectionLine To(UiElement anchor, Func<UiElement, Vector2> transformFunction)
    {
        endElement = anchor;
        endTransform = transformFunction;
        return this;
    }

    public ConnectionLine To(Vector2 end)
    {
        _end = end;
        return this;
    }

    private static SKPaint _paint = new()
    {
        Color = new SKColor(200, 0, 0),
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 5
    };

    private static SKPaint _paintEnds = new()
    {
        Color = new SKColor(200, 0, 0),
        IsAntialias = true,
    };

    private static SKPath _path = new();

    public override void Render(SKCanvas canvas)
    {
        if (startElement is not null && startTransform is not null)
        {
            _start = startTransform(startElement);
        }

        if (endElement is not null && endTransform is not null)
        {
            _start = endTransform(endElement);
        }

        var centerX = (_start.X + _end.X) / 2;
        var start = new SKPoint(_start.X, _start.Y);
        var startHandle = new SKPoint(centerX, _start.Y);
        var endHandle = new SKPoint(centerX, _end.Y);
        var end = new SKPoint(_end.X, _end.Y);

        canvas.DrawCircle(start, 2.5f, _paintEnds);
        canvas.DrawCircle(end, 2.5f, _paintEnds);

        _path.MoveTo(start);
        _path.CubicTo(startHandle, endHandle, end);
        _paint.Clone();
        canvas.DrawPath(_path, _paint);
        _path.Reset();
    }

    public override void Layout(UiWindow uiWindow)
    {
    }

    public override bool LayoutHasChanged()
    {
        throw new NotImplementedException();
    }

    public override bool HasChanges()
    {
        throw new NotImplementedException();
    }
}
