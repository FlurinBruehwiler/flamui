using EnumXAlign = Flamui.XAlign;
using EnumMAlign = Flamui.MAlign;

namespace Flamui.UiElements;

public partial class FlexContainer
{
    public FlexContainer BlockHit(bool blockHit = true)
    {
        Info.BlockHit = blockHit;
        return this;
    }

    public FlexContainer Shadow(float sigma, int top = 0, int right = 0, int left = 0, int bottom = 0)
    {
        Info.ShadowSigma = sigma;
        Info.ShaddowOffset = new Quadrant(left, right, top, bottom);
        return this;
    }

    public FlexContainer ShadowColor(byte red, byte green, byte blue, byte alpha = 255)
    {
        Info.PShadowColor = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    public FlexContainer Color(byte red, byte green, byte blue, byte alpha = 255)
    {
        Info.Color = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    public FlexContainer Color(ColorDefinition color)
    {
        Info.Color = color;
        return this;
    }

    public FlexContainer BorderColor(byte red, byte green, byte blue, byte alpha = 255)
    {
        Info.BorderColor = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    public FlexContainer BorderColor(ColorDefinition color)
    {
        Info.BorderColor = color;
        return this;
    }

    public FlexContainer BorderWidth(int borderWidth)
    {
        Info.BorderWidth = borderWidth;
        return this;
    }

    public FlexContainer Border(int borderWidth, ColorDefinition color)
    {
        Info.BorderWidth = borderWidth;
        Info.BorderColor = color;
        return this;
    }

    /// <summary>
    /// only works if this is the last clip applied!!!
    /// </summary>
    /// <param name="div"></param>
    /// <returns></returns>
    public FlexContainer IgnoreClipFrom(FlexContainer div)
    {
        Info.ClipToIgnore = (FlexContainer)div;
        return this;
    }

    public FlexContainer Rounded(int radius)
    {
        Info.Radius = radius;
        return this;
    }

    public FlexContainer Focusable(bool focusable = true)
    {
        Info.Focusable = focusable;
        return this;
    }

    public FlexContainer ZIndex(int zIndex)
    {
        Info.ZIndex = zIndex;
        return this;
    }

    public FlexContainer Hidden(bool hidden = true)
    {
        Info.Hidden = hidden;
        return this;
    }

    public FlexContainer Center()
    {
        Info.MainAlignment = EnumMAlign.Center;
        Info.CrossAlignment = EnumXAlign.Center;
        return this;
    }

    public FlexContainer Size(float width, float height)
    {
        Info.WidthValue = width;
        Info.WidthKind = SizeKind.Pixel;
        Info.HeightValue = height;
        Info.HeightKind = SizeKind.Pixel;
        return this;
    }

    public FlexContainer Width(float width)
    {
        Info.WidthValue = width;
        Info.WidthKind = SizeKind.Pixel;
        return this;
    }

    public FlexContainer Height(float height)
    {
        Info.HeightValue = height;
        Info.HeightKind = SizeKind.Pixel;
        return this;
    }

    public FlexContainer Shrink()
    {
        Info.HeightKind = SizeKind.Shrink;
        Info.WidthKind = SizeKind.Shrink;
        return this;
    }

    public FlexContainer ShrinkHeight()
    {
        Info.HeightKind = SizeKind.Shrink;
        return this;
    }

    public FlexContainer ShrinkWidth()
    {
        Info.WidthKind = SizeKind.Shrink;
        return this;
    }

    public FlexContainer Clip(bool isClipped = true)
    {
        Info.IsClipped = isClipped;
        return this;
    }

    public FlexContainer WidthFraction(float width)
    {
        Info.WidthValue = width;
        Info.WidthKind = SizeKind.Percentage;
        return this;
    }

    public FlexContainer HeightFraction(float height)
    {
        Info.HeightValue = height;
        Info.HeightKind = SizeKind.Percentage;
        return this;
    }

    public FlexContainer MAlign(MAlign mAlign)
    {
        Info.MainAlignment = mAlign;
        return this;
    }

    public FlexContainer XAlign(XAlign xAlign)
    {
        Info.CrossAlignment = xAlign;
        return this;
    }

    public FlexContainer Scroll()
    {
        Info.CanScroll = true;
        return this;
    }



    public FlexContainer Direction(Dir dir)
    {
        Info.Direction = dir;
        return this;
    }

    #region Padding

    public FlexContainer Padding(int padding)
    {
        Info.Padding = new Quadrant(padding, padding, padding, padding);
        return this;
    }

    public FlexContainer PaddingEx(int left = 0, int right = 0, int top = 0, int bottom = 0)
    {
        Info.Padding = new Quadrant(left, right, top, bottom);
        return this;
    }

    public FlexContainer PaddingHorizontal(int paddingHorizontal)
    {
        Info.Padding = Info.Padding with { Left = paddingHorizontal, Right = paddingHorizontal };
        return this;
    }

    public FlexContainer PaddingVertical(int paddingVertical)
    {
        Info.Padding = Info.Padding with { Top = paddingVertical, Bottom = paddingVertical };
        return this;
    }

    public FlexContainer PaddingLeft(int paddingLeft)
    {
        Info.Padding = Info.Padding with { Left = paddingLeft };
        return this;
    }

    public FlexContainer PaddingRight(int paddingRight)
    {
        Info.Padding = Info.Padding with { Right = paddingRight };
        return this;
    }

    public FlexContainer PaddingTop(int paddingTop)
    {
        Info.Padding = Info.Padding with { Top = paddingTop };
        return this;
    }

    public FlexContainer PaddingBottom(int paddingBottom)
    {
        Info.Padding = Info.Padding with { Bottom = paddingBottom };
        return this;
    }

    #endregion

    public FlexContainer Gap(int gap)
    {
        Info.Gap = gap;
        return this;
    }
}
