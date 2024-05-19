using EnumXAlign = Flamui.XAlign;
using EnumMAlign = Flamui.MAlign;

namespace Flamui.UiElements;

public partial class FlexContainer
{
    public FlexContainer BlockHit(bool blockHit = true)
    {
        FlexContainerInfo.BlockHit = blockHit;
        return this;
    }

    public FlexContainer Shadow(float sigma, int top = 0, int right = 0, int left = 0, int bottom = 0)
    {
        FlexContainerInfo.ShadowSigma = sigma;
        FlexContainerInfo.ShaddowOffset = new Quadrant(left, right, top, bottom);
        return this;
    }

    public FlexContainer ShadowColor(byte red, byte green, byte blue, byte alpha = 255)
    {
        FlexContainerInfo.PShadowColor = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    public FlexContainer Color(byte red, byte green, byte blue, byte alpha = 255)
    {
        FlexContainerInfo.Color = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    public FlexContainer Color(ColorDefinition color)
    {
        FlexContainerInfo.Color = color;
        return this;
    }

    public FlexContainer BorderColor(byte red, byte green, byte blue, byte alpha = 255)
    {
        FlexContainerInfo.BorderColor = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    public FlexContainer BorderColor(ColorDefinition color)
    {
        FlexContainerInfo.BorderColor = color;
        return this;
    }

    public FlexContainer BorderWidth(int borderWidth)
    {
        FlexContainerInfo.BorderWidth = borderWidth;
        return this;
    }

    public FlexContainer Border(int borderWidth, ColorDefinition color)
    {
        FlexContainerInfo.BorderWidth = borderWidth;
        FlexContainerInfo.BorderColor = color;
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
        FlexContainerInfo.ClipToIgnore = (FlexContainer)div;
        return this;
    }

    public FlexContainer Rounded(int radius)
    {
        FlexContainerInfo.Radius = radius;
        return this;
    }

    public FlexContainer Focusable(bool focusable = true)
    {
        FlexContainerInfo.Focusable = focusable;
        return this;
    }

    public FlexContainer ZIndex(int zIndex)
    {
        FlexContainerInfo.ZIndex = zIndex;
        return this;
    }

    public FlexContainer Hidden(bool hidden = true)
    {
        FlexContainerInfo.Hidden = hidden;
        return this;
    }

    public FlexContainer Center()
    {
        FlexContainerInfo.MainAlignment = EnumMAlign.Center;
        FlexContainerInfo.CrossAlignment = EnumXAlign.Center;
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
        FlexContainerInfo.IsClipped = isClipped;
        return this;
    }

    public FlexContainer Relative()
    {
        FlexContainerInfo.Absolute = false;
        FlexContainerInfo.AbsoluteContainer = null;
        FlexContainerInfo.DisablePositioning = false;
        FlexContainerInfo.AbsolutePosition = new AbsolutePosition();
        return this;
    }

    public FlexContainer Absolute(FlexContainer? container = null, float? left = null, float? right = null, float? top = null, float? bottom = null, bool disablePositioning = false)
    {
        FlexContainerInfo.Absolute = true;
        FlexContainerInfo.AbsoluteContainer = container;
        FlexContainerInfo.DisablePositioning = disablePositioning;
        FlexContainerInfo.AbsolutePosition = new AbsolutePosition(left, right, top, bottom);
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
        FlexContainerInfo.Padding = new Quadrant(padding, padding, padding, padding);
        return this;
    }

    public FlexContainer MAlign(MAlign mAlign)
    {
        FlexContainerInfo.MainAlignment = mAlign;
        return this;
    }

    public FlexContainer XAlign(XAlign xAlign)
    {
        FlexContainerInfo.CrossAlignment = xAlign;
        return this;
    }

    public FlexContainer PaddingEx(int left = 0, int right = 0, int top = 0, int bottom = 0)
    {
        FlexContainerInfo.Padding = new Quadrant(left, right, top, bottom);
        return this;
    }

    public FlexContainer PaddingHorizontal(int paddingHorizontal)
    {
        FlexContainerInfo.Padding = FlexContainerInfo.Padding with { Left = paddingHorizontal, Right = paddingHorizontal };
        return this;
    }

    public FlexContainer Scroll()
    {
        PCanScroll = true;
        return this;
    }

    public FlexContainer PaddingVertical(int paddingVertical)
    {
        FlexContainerInfo.Padding = FlexContainerInfo.Padding with { Top = paddingVertical, Bottom = paddingVertical };
        return this;
    }

    public FlexContainer Dir(Dir dir)
    {
        FlexContainerInfo.Direction = dir;
        return this;
    }

    public FlexContainer PaddingLeft(int paddingLeft)
    {
        FlexContainerInfo.Padding = FlexContainerInfo.Padding with { Left = paddingLeft };
        return this;
    }

    public FlexContainer PaddingRight(int paddingRight)
    {
        FlexContainerInfo.Padding = FlexContainerInfo.Padding with { Right = paddingRight };
        return this;
    }

    public FlexContainer PaddingTop(int paddingTop)
    {
        FlexContainerInfo.Padding = FlexContainerInfo.Padding with { Top = paddingTop };
        return this;
    }

    public FlexContainer PaddingBottom(int paddingBottom)
    {
        FlexContainerInfo.Padding = FlexContainerInfo.Padding with { Bottom = paddingBottom };
        return this;
    }

    public FlexContainer Gap(int gap)
    {
        FlexContainerInfo.Gap = gap;
        return this;
    }
}
