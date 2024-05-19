namespace Flamui.Layouting;

public struct BoxSize
{
    public readonly float Width;
    public readonly float Height;

    public BoxSize(float width, float height)
    {
        Width = width;
        Height = height;
    }

    public static BoxSize FromDirection(Dir dir, float main, float cross)
    {
        return dir switch
        {
            Dir.Horizontal => new BoxSize(main, cross),
            Dir.Vertical => new BoxSize(cross, main),
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }

    public float GetMainAxis(Dir direction)
    {
        return direction switch
        {
            Dir.Horizontal => Width,
            Dir.Vertical => Height,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    public float GetCrossAxis(Dir direction)
    {
        return direction switch
        {
            Dir.Horizontal => Height,
            Dir.Vertical => Width,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    public Bounds ToBounds()
    {
        return new Bounds
        {
            X = 0,
            Y = 0,
            W = Width,
            H = Height
        };
    }
}
