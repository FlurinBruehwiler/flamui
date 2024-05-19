using EnumXAlign = Flamui.XAlign;
using EnumMAlign = Flamui.MAlign;

namespace Flamui.UiElements;

public partial class FlexContainer
{
    public FlexContainer BlockHit(bool blockHit = true)
    {
        PBlockHit = blockHit;
        return this;
    }

    public FlexContainer Shadow(float sigma, int top = 0, int right = 0, int left = 0, int bottom = 0)
    {
        ShadowSigma = sigma;
        ShaddowOffset = new Quadrant(left, right, top, bottom);
        return this;
    }

    public FlexContainer ShadowColor(byte red, byte green, byte blue, byte alpha = 255)
    {
        PShadowColor = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    public FlexContainer Color(byte red, byte green, byte blue, byte alpha = 255)
    {
        PColor = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    public FlexContainer Color(ColorDefinition color)
    {
        PColor = color;
        return this;
    }

    public FlexContainer BorderColor(byte red, byte green, byte blue, byte alpha = 255)
    {
        PBorderColor = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    public FlexContainer BorderColor(ColorDefinition color)
    {
        PBorderColor = color;
        return this;
    }

    public FlexContainer BorderWidth(int borderWidth)
    {
        PBorderWidth = borderWidth;
        return this;
    }

    public FlexContainer Border(int borderWidth, ColorDefinition color)
    {
        PBorderWidth = borderWidth;
        PBorderColor = color;
        return this;
    }

    // public UiContainer MAlign(MAlign mAlign)
    // {
    //     PmAlign = mAlign;
    //     return this;
    // }
    //
    // public UiContainer XAlign(XAlign xAlign)
    // {
    //     PxAlign = xAlign;
    //     return this;
    // }

    /// <summary>
    /// only works if this is the last clip applied!!!
    /// </summary>
    /// <param name="div"></param>
    /// <returns></returns>
    public FlexContainer IgnoreClipFrom(FlexContainer div)
    {
        ClipToIgnore = (FlexContainer)div;
        return this;
    }

    public FlexContainer Rounded(int radius)
    {
        PRadius = radius;
        return this;
    }

    public FlexContainer Focusable(bool focusable = true)
    {
        PFocusable = focusable;
        return this;
    }

    public FlexContainer ZIndex(int zIndex)
    {
        PZIndex = zIndex;
        return this;
    }

    public FlexContainer Hidden(bool hidden = true)
    {
        PHidden = hidden;
        return this;
    }

    public FlexContainer Center()
    {
        PmAlign = EnumMAlign.Center;
        PxAlign = EnumXAlign.Center;
        return this;
    }

    public FlexContainer Width(float width)
    {
        PWidth = new SizeDefinition(width, SizeKind.Pixel);
        return this;
    }

    public FlexContainer Height(float height)
    {
        PHeight = new SizeDefinition(height, SizeKind.Pixel);
        return this;
    }

    public FlexContainer ShrinkHeight()
    {
        PHeight = new SizeDefinition(0, SizeKind.Shrink);
        return this;
    }

    public FlexContainer ShrinkWidth()
    {
        PWidth = new SizeDefinition(0, SizeKind.Shrink);
        return this;
    }

    public FlexContainer Clip(bool isClipped = true)
    {
        IsClipped = isClipped;
        return this;
    }

    public FlexContainer Relative()
    {
        PAbsolute = false;
        AbsoluteContainer = null;
        DisablePositioning = false;
        PAbsolutePosition = new AbsolutePosition();
        return this;
    }

    public FlexContainer Absolute(FlexContainer? container = null, float? left = null, float? right = null, float? top = null, float? bottom = null, bool disablePositioning = false)
    {
        PAbsolute = true;
        AbsoluteContainer = container;
        DisablePositioning = disablePositioning;
        PAbsolutePosition = new AbsolutePosition(left, right, top, bottom);
        return this;
    }

    public FlexContainer WidthFraction(float width)
    {
        PWidth = new SizeDefinition(width, SizeKind.Percentage);
        return this;
    }

    public FlexContainer HeightFraction(float height)
    {
        PHeight = new SizeDefinition(height, SizeKind.Percentage);
        return this;
    }

    public FlexContainer Padding(int padding)
    {
        PPadding = new Quadrant(padding, padding, padding, padding);
        return this;
    }

    public FlexContainer MAlign(MAlign mAlign)
    {
        PmAlign = mAlign;
        return this;
    }

    public FlexContainer XAlign(XAlign xAlign)
    {
        PxAlign = xAlign;
        return this;
    }

    public FlexContainer PaddingEx(int left = 0, int right = 0, int top = 0, int bottom = 0)
    {
        PPadding = new Quadrant(left, right, top, bottom);
        return this;
    }

    public FlexContainer PaddingHorizontal(int paddingHorizontal)
    {
        PPadding = PPadding with { Left = paddingHorizontal, Right = paddingHorizontal };
        return this;
    }

    public FlexContainer Scroll()
    {
        PCanScroll = true;
        return this;
    }

    public FlexContainer PaddingVertical(int paddingVertical)
    {
        PPadding = PPadding with { Top = paddingVertical, Bottom = paddingVertical };
        return this;
    }

    public FlexContainer Dir(Dir dir)
    {
        Direction = dir;
        return this;
    }

    public FlexContainer PaddingLeft(int paddingLeft)
    {
        PPadding = PPadding with { Left = paddingLeft };
        return this;
    }

    public FlexContainer PaddingRight(int paddingRight)
    {
        PPadding = PPadding with { Right = paddingRight };
        return this;
    }

    public FlexContainer PaddingTop(int paddingTop)
    {
        PPadding = PPadding with { Top = paddingTop };
        return this;
    }

    public FlexContainer PaddingBottom(int paddingBottom)
    {
        PPadding = PPadding with { Bottom = paddingBottom };
        return this;
    }

    public FlexContainer Gap(int gap)
    {
        PGap = gap;
        return this;
    }
}
