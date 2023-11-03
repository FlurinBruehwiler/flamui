using System.Numerics;
using ImSharpUISample.UiElements;
using SkiaSharp;

namespace ImSharpUISample;

public record struct PortPosition(Vector2 Pos, PortDirection Direction);

public class ConnectionLine : UiElement
{
    private Vector2? _end;
    private ConnectionTarget _a;
    private ConnectionTarget? _b;
    private bool _isDynamic;

    public override void CleanElement()
    {
        _a = default!;
        _b = null;
        _end = null;
        _isDynamic = false;
    }

    public ConnectionLine Static(ConnectionTarget a, Vector2 endPos)
    {
        _a = a;
        _end = endPos;
        return this;
    }

    public ConnectionLine Static(ConnectionTarget a, ConnectionTarget b)
    {
        _a = a;
        _b = b;
        return this;
    }

    public ConnectionLine Dynamic(ConnectionTarget a, ConnectionTarget b)
    {
        _a = a;
        _b = b;
        _isDynamic = true;
        return this;
    }

    private static SKPaint _paint = new()
    {
        Color = new SKColor(0, 214, 163),
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 2
    };

    private static SKPaint _paintEnds = new()
    {
        Color = new SKColor(0, 214, 163),
        IsAntialias = true,
    };


    private static PortPosition GetCenter(Port port)
    {
        return new PortPosition(new Vector2(port.PortElement.ComputedX + port.PortElement.ComputedWidth / 2,
            port.PortElement.ComputedY + port.PortElement.ComputedHeight / 2), port.PortDirection);
    }

    private ValueTuple<PortPosition, PortPosition> GetFixPositions()
    {
        PortPosition end;

        if (_end is null)
        {
            end = GetCenter(_b.Value.GetActivePort());
        }
        else
        {
            end = new PortPosition(_end.Value, PortDirection.Undefined);
        }

        var start = GetCenter(_a.GetActivePort());

        return (end, start);
    }

    private ValueTuple<PortPosition, PortPosition> GetDynamicPositions()
    {
        var start1 = GetCenter(_a.LeftPort);
        var start2 = GetCenter(_a.RightPort);
        var end1 = GetCenter(_b.Value.LeftPort);
        var end2 = GetCenter(_b.Value.RightPort);

        var distance12 = Vector2.Distance(start1.Pos, end2.Pos);
        var distance21 = Vector2.Distance(start2.Pos, end1.Pos);

        ValueTuple<float, PortPosition, PortPosition> smallest = (distance12, start1, end2);

        if (distance21 < smallest.Item1)
        {
            smallest = (distance12, start2, end1);
        }

        return (smallest.Item2, smallest.Item3);
    }

    public override void Render(RenderContext renderContext)
    {
        PortPosition source;
        PortPosition target;

        if (_isDynamic)
        {
            (source, target) = GetDynamicPositions();
        }
        else
        {
            (source, target) = GetFixPositions();
        }

        var centerX = (source.Pos.X + target.Pos.X) / 2;

        var start = new SKPoint(source.Pos.X, source.Pos.Y);
        var end = new SKPoint(target.Pos.X, target.Pos.Y);

        SKPoint startHandle;
        SKPoint endHandle;

        if (target.Pos.X > source.Pos.X && source.Direction == PortDirection.Left ||
            source.Pos.X > target.Pos.X && target.Direction == PortDirection.Left)
        {
            var difX = Math.Abs(source.Pos.X - target.Pos.X);

            startHandle = new SKPoint(RemoveInDirection(source.Pos.X, difX, source.Direction), source.Pos.Y);
            endHandle = new SKPoint(RemoveInDirection(target.Pos.X, difX, target.Direction), target.Pos.Y);
        }
        else
        {
            startHandle = new SKPoint(centerX, source.Pos.Y);
            endHandle = new SKPoint(centerX, target.Pos.Y);
        }

        renderContext.Add(new Circle
        {
            SkPaint = _paintEnds,
            Pos = start,
            Radius = 2.5f
        });

        renderContext.Add(new Circle
        {
            SkPaint = _paintEnds,
            Pos = end,
            Radius = 2.5f
        });

        renderContext.Add(new Path
        {
            SkPaint = _paint,
            Start = start,
            StartHandle = startHandle,
            EndHandle = endHandle,
            End = end
        });
    }

    private float RemoveInDirection(float value, float subtract, PortDirection position)
    {
        if (position == PortDirection.Left)
            return value - subtract;

        return value + subtract;
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

public enum PortDirection
{
    Left,
    Right,
    Undefined
}
