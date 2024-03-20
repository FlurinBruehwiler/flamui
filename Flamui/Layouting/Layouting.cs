namespace Flamui.Layouting;

public interface ILayoutable
{
    public ParentData ParentData { get; set; }

    public FlexibleChildConfig? FlexibleChildConfig { get; }

    public bool IsFlexible(out FlexibleChildConfig config)
    {
        config = new FlexibleChildConfig();

        if (FlexibleChildConfig is null)
        {
            return false;
        }

        config = FlexibleChildConfig.Value;

        return true;
    }
    BoxSize Layout(BoxConstraint constraint);
}

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
    public float Min;
    public float Max;
}

public struct Point
{
    public float X;
    public float Y;
}

public struct ParentData
{
    public Point Position;
}
