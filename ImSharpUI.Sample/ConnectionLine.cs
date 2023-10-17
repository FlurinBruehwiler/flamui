using System.Numerics;
using ImSharpUISample.UiElements;
using SkiaSharp;

namespace ImSharpUISample;

public class ConnectionLine : UiElement
{
    private Vector2 Start { get; set; }
    private Vector2 End { get; set; }

    public ConnectionLine From(Vector2 start)
    {
        Start = start;
        return this;
    }

    public ConnectionLine To(Vector2 end)
    {
        End = end;
        return this;
    }

    private static SKPaint _paint = new()
    {
        Color = new SKColor(200, 0, 0),
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 5
    };

    private static SKPath _path = new();

    public override void Render(SKCanvas canvas)
    {
        var centerX = (Start.X + End.X) / 2;
        var start = new SKPoint(Start.X, Start.Y);
        var startHandle = new SKPoint(centerX, Start.Y);
        var endHandle = new SKPoint(centerX, End.Y);
        var end = new SKPoint(End.X, End.Y);

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
