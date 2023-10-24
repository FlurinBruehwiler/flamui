using System.Numerics;
using EnumXAlign = ImSharpUISample.XAlign;
using EnumMAlign = ImSharpUISample.MAlign;
using EnumDir = ImSharpUISample.Dir;

namespace ImSharpUISample.UiElements;

public partial class UiContainer
{
    public UiContainer BlockHit(bool blockHit = true)
    {
        PBlockHit = blockHit;
        return this;
    }

    public UiContainer Shadow(float sigma, int top = 0, int right = 0, int left = 0, int bottom = 0)
    {
        ShadowSigma = sigma;
        ShaddowOffset = new Quadrant(left, right, top, bottom);
        return this;
    }

    public UiContainer ShadowColor(byte red, byte green, byte blue, byte alpha = 255)
    {
        PShadowColor = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    public UiContainer Color(byte red, byte green, byte blue, byte alpha = 255)
    {
        PColor = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    public UiContainer Color(ColorDefinition color)
    {
        PColor = color;
        return this;
    }

    public UiContainer BorderColor(byte red, byte green, byte blue, byte alpha = 255)
    {
        PBorderColor = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    public UiContainer BorderColor(ColorDefinition color)
    {
        PBorderColor = color;
        return this;
    }

    public UiContainer BorderWidth(int borderWidth)
    {
        PBorderWidth = borderWidth;
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
    public UiContainer IgnoreClipFrom(UiContainer div)
    {
        ClipToIgnore = (UiContainer)div;
        return this;
    }

    public UiContainer Radius(int radius)
    {
        PRadius = radius;
        return this;
    }

    public UiContainer Focusable(bool focusable = true)
    {
        PFocusable = focusable;
        return this;
    }

    public UiContainer ZIndex(int zIndex)
    {
        PZIndex = zIndex;
        return this;
    }

    public UiContainer Hidden(bool hidden = true)
    {
        PHidden = hidden;
        return this;
    }

    public UiContainer Center()
    {
        PmAlign = EnumMAlign.Center;
        PxAlign = EnumXAlign.Center;
        return this;
    }

    public UiContainer Width(float width)
    {
        PWidth = new SizeDefinition(width, SizeKind.Pixel);
        return this;
    }

    public UiContainer Height(float height)
    {
        PHeight = new SizeDefinition(height, SizeKind.Pixel);
        return this;
    }

    public UiContainer Clip(bool isClipped = true)
    {
        IsClipped = isClipped;
        return this;
    }

    public UiContainer Relative()
    {
        PAbsolute = false;
        AbsoluteContainer = null;
        DisablePositioning = false;
        PAbsolutePosition = new AbsolutePosition();
        return this;
    }

    public UiContainer Absolute(UiContainer? container = null, int? left = null, int? right = null, int? top = null, int? bottom = null, bool disablePositioning = false)
    {
        PAbsolute = true;
        AbsoluteContainer = container as UiContainer;
        DisablePositioning = disablePositioning;
        PAbsolutePosition = new AbsolutePosition(left, right, top, bottom);
        return this;
    }

    public UiContainer WidthFraction(float width)
    {
        PWidth = new SizeDefinition(width, SizeKind.Percentage);
        return this;
    }

    public UiContainer HeightFraction(float height)
    {
        PHeight = new SizeDefinition(height, SizeKind.Percentage);
        return this;
    }

    public UiContainer Padding(int padding)
    {
        PPadding = new Quadrant(padding, padding, padding, padding);
        return this;
    }

    public UiContainer MAlign(MAlign mAlign)
    {
        PmAlign = mAlign;
        return this;
    }

    public UiContainer XAlign(XAlign xAlign)
    {
        PxAlign = xAlign;
        return this;
    }

    public UiContainer PaddingEx(int left = 0, int right = 0, int top = 0, int bottom = 0)
    {
        PPadding = new Quadrant(left, right, top, bottom);
        return this;
    }

    public UiContainer PaddingHorizontal(int paddingHorizontal)
    {
        PPadding = PPadding with { Left = paddingHorizontal, Right = paddingHorizontal };
        return this;
    }

    public UiContainer Scroll()
    {
        PCanScroll = true;
        return this;
    }

    public UiContainer PaddingVertical(int paddingVertical)
    {
        PPadding = PPadding with { Top = paddingVertical, Bottom = paddingVertical };
        return this;
    }

    public UiContainer Dir(Dir dir)
    {
        PDir = dir;
        return this;
    }

    public UiContainer PaddingLeft(int paddingLeft)
    {
        PPadding = PPadding with { Left = paddingLeft };
        return this;
    }

    public UiContainer PaddingRight(int paddingRight)
    {
        PPadding = PPadding with { Right = paddingRight };
        return this;
    }

    public UiContainer PaddingTop(int paddingTop)
    {
        PPadding = PPadding with { Top = paddingTop };
        return this;
    }

    public UiContainer PaddingBottom(int paddingBottom)
    {
        PPadding = PPadding with { Bottom = paddingBottom };
        return this;
    }

    public UiContainer Gap(int gap)
    {
        PGap = gap;
        return this;
    }

}
