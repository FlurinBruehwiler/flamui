namespace Flamui.Layouting;

/*
 *(dir horizontal)
 * If Inflexible then (aktuell Pixel, Shrink):
 *      height: 0-incommingMaxHeight
 *      width: 0-Infinite(decide yourself!!)
 *
 * If flexible, flexfactor (aktuell Percentage)
 *  height:0-incommingMaxHeight
 *  width: exact-exact
 */

public struct FlexibleChildConfig
{
    public float Percentage;
}

public struct Point
{
    public float X;
    public float Y;

    public Point(float x, float y)
    {
        X = x;
        Y = y;
    }

    public Point Add(Point point)
    {
        return new Point(X + point.X, Y + point.Y);
    }

    public static Point FromDirection(Dir dir, float main, float cross)
    {
        return dir switch
        {
            Dir.Horizontal => new Point(main, cross),
            Dir.Vertical => new Point(cross, main),
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }
}

public struct ParentData
{
    public Point Position;
}
