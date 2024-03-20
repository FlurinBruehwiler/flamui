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
}
