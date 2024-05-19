using System.Diagnostics.CodeAnalysis;

namespace Flamui.Layouting;

public struct BoxConstraint
{
    public float MinWidth;
    public float MaxWidth;
    public float MinHeight;
    public float MaxHeight;

    public BoxConstraint(float minWidth, float maxWidth, float minHeight, float maxHeight)
    {
        MinWidth = minWidth;
        MinHeight = minHeight;
        MaxWidth = maxWidth;
        MaxHeight = maxHeight;
    }

    public void SetMain(Dir dir, float min, float max)
    {
        switch (dir)
        {
            case Dir.Horizontal:
                MinWidth = min;
                MaxWidth = max;
                break;
            case Dir.Vertical:
                MinHeight = min;
                MaxHeight = max;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
        }
    }

    public static BoxConstraint FromBox(float width, float height)
    {
        return new BoxConstraint(width, width, height, height);
    }

    public static BoxConstraint FromDirection(Dir direction, float minMain, float maxMain, float minCross,
        float maxCross)
    {
        return direction switch
        {
            Dir.Horizontal => new BoxConstraint(minMain, maxMain, minCross, maxCross),
            Dir.Vertical => new BoxConstraint(minCross, maxCross, minMain, maxMain),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    public (float Min, float Max) GetMainAxis(Dir direction)
    {
        return direction switch
        {
            Dir.Horizontal => (MinWidth, MaxWidth),
            Dir.Vertical => (MinHeight, MaxHeight),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    public (float Min, float Max) GetCrossAxis(Dir direction)
    {
        return direction switch
        {
            Dir.Horizontal => (MinHeight, MaxHeight),
            Dir.Vertical => (MinWidth, MaxWidth),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    public bool IsWidthTight()
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return MinWidth == MaxWidth;
    }

    public bool IsHeightTight()
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return MinHeight == MaxHeight;
    }

    public bool IsTight()
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (MinWidth != MaxWidth)
            return false;

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return MinHeight == MaxHeight;
    }

    public bool IsValid()
    {
        if (MinWidth > MaxWidth)
            return false;

        if (MinHeight > MaxHeight)
            return false;

        if (MinHeight < 0 || MaxHeight < 0 || MinWidth < 0 || MaxWidth < 0)
            return false;

        return true;
    }
}
