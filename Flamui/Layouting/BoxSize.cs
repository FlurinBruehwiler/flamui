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

    public BoxSize ApplyConstraint(BoxConstraint constraint)
    {
        var width = Math.Min(constraint.MaxWidth, Width);
        width = Math.Max(constraint.MinWidth, width);

        var height = Math.Min(constraint.MaxHeight, Height);
        height = Math.Max(constraint.MinHeight, height);

        return new BoxSize(width, height);
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

    public Bounds ToBounds(Point point)
    {
        return new Bounds
        {
            X = point.X,
            Y = point.Y,
            W = Width,
            H = Height
        };
    }
}
