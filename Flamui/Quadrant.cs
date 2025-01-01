using Flamui.Layouting;
using Flamui.UiElements;

namespace Flamui;

public record struct Quadrant(float Left, float Right, float Top, float Bottom)
{
    public float SumInDirection(Dir dir)
    {
        return dir switch
        {
            Dir.Horizontal => Left + Right,
            Dir.Vertical => Top + Bottom,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }

    public float EndOfDirection(Dir dir)
    {
        return dir switch
        {
            Dir.Horizontal => Right,
            Dir.Vertical => Bottom,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }

    public float StartOfDirection(Dir dir)
    {
        return dir switch
        {
            Dir.Horizontal => Left,
            Dir.Vertical => Top,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }
}
public record struct AbsolutePosition(float? Left, float? Right, float? Top, float? Bottom);

public struct RelativeSize
{
    public float? HeightOffsetFromParent; //has an effect, if RelativeToParent = true
    public float? WidthOffsetFromParent; //has an effect, if RelativeToParent = true
}

public enum MAlign
{
    FlexStart = 0, //Default
    FlexEnd,
    Center,
    SpaceBetween
}

public enum Dir
{
    Vertical = 0, //Default
    Horizontal,
}

public enum XAlign
{
    FlexStart = 0, //Default
    FlexEnd,
    Center
}

public enum SizeKind
{
    Percentage = 0, //Default
    Pixel,
    Shrink
}


public static class Extensions
{
    public static T Absolute<T>(this T uiElement, FlexContainer? anchor = null) where T : UiElement
    {
        uiElement.UiElementInfo.AbsoluteInfo = (uiElement.UiElementInfo.AbsoluteInfo ?? new AbsoluteInfo()) with
        {
            Anchor = anchor,
        };
        return uiElement;
    }

    public static T AbsolutePosition<T>(this T uiElement, float? left = null, float? right = null, float? top = null, float? bottom = null) where T : UiElement
    {
        uiElement.UiElementInfo.AbsoluteInfo = (uiElement.UiElementInfo.AbsoluteInfo ?? new AbsoluteInfo()) with
        {
            Position = new AbsolutePosition(left, right, top, bottom)
        };
        return uiElement;
    }

    public static T AbsoluteSize<T>(this T uiElement, float? widthOffsetParent = null, float? heightOffsetParent = null) where T : UiElement
    {
        uiElement.UiElementInfo.AbsoluteInfo = (uiElement.UiElementInfo.AbsoluteInfo ?? new AbsoluteInfo()) with
        {
            Size = new RelativeSize
            {
                HeightOffsetFromParent = heightOffsetParent,
                WidthOffsetFromParent = widthOffsetParent
            }
        };
        return uiElement;
    }

    #region Margin
    public static T Margin<T>(this T uiElement, float margin) where T : UiElement
    {
        uiElement.UiElementInfo.Margin = new Quadrant(margin, margin, margin, margin);
        return uiElement;
    }

    public static T MarginEx<T>(this T uiElement, int left = 0, int right = 0, int top = 0, int bottom = 0) where T : UiElement
    {
        uiElement.UiElementInfo.Margin = new Quadrant(left, right, top, bottom);
        return uiElement;
    }

    public static T MarginHorizontal<T>(this T uiElement, int marginHorizontal) where T : UiElement
    {
        uiElement.UiElementInfo.Margin = uiElement.UiElementInfo.Margin with { Left = marginHorizontal, Right = marginHorizontal };
        return uiElement;
    }

    public static T MarginVertical<T>(this T uiElement, int marginVertical) where T : UiElement
    {
        uiElement.UiElementInfo.Margin = uiElement.UiElementInfo.Margin with { Top = marginVertical, Bottom = marginVertical };
        return uiElement;
    }

    public static T MarginLeft<T>(this T uiElement, int marginLeft) where T : UiElement
    {
        uiElement.UiElementInfo.Margin = uiElement.UiElementInfo.Margin with { Left = marginLeft };
        return uiElement;
    }

    public static T MarginRight<T>(this T uiElement, int marginRight) where T : UiElement
    {
        uiElement.UiElementInfo.Margin = uiElement.UiElementInfo.Margin with { Right = marginRight };
        return uiElement;
    }

    public static T MarginTop<T>(this T uiElement, int marginTop) where T : UiElement
    {
        uiElement.UiElementInfo.Margin = uiElement.UiElementInfo.Margin with { Top = marginTop };
        return uiElement;
    }

    public static T MarginBottom<T>(this T uiElement, int marginBottom) where T : UiElement
    {
        uiElement.UiElementInfo.Margin = uiElement.UiElementInfo.Margin with { Bottom = marginBottom };
        return uiElement;
    }

    #endregion

    public static bool IsFlexible(this UiElement uiElement, out FlexibleChildConfig config)
    {
        config = new FlexibleChildConfig();

        if (uiElement.FlexibleChildConfig is null)
        {
            return false;
        }

        config = uiElement.FlexibleChildConfig.Value;

        return true;
    }

    public static Dir Other(this Dir dir)
    {
        return dir == Dir.Horizontal ? Dir.Vertical : Dir.Horizontal;
    }


    public static float GetMain(this Dir dir, float width, float height)
    {
        return dir switch
        {
            Dir.Horizontal => width,
            Dir.Vertical => height,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }

    public static float Cross(this Dir dir, float width, float height)
    {
        return dir switch
        {
            Dir.Horizontal => height,
            Dir.Vertical => width,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }
}

public readonly record struct ColorDefinition(byte Red, byte Green, byte Blue, byte Alpha = 255)
{
    public static ColorDefinition operator /(ColorDefinition original, byte opacity)
    {
        return original with
        {
            Alpha = (byte)(original.Alpha / opacity)
        };
    }
}

