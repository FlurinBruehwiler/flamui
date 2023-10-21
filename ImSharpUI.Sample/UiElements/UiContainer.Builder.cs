using System.Numerics;
using EnumXAlign = ImSharpUISample.XAlign;
using EnumMAlign = ImSharpUISample.MAlign;
using EnumDir = ImSharpUISample.Dir;

namespace ImSharpUISample.UiElements;

public interface IUiContainerBuilder
{
    public IUiContainerBuilder Color(byte red, byte green, byte blue, byte alpha = 255);
    public IUiContainerBuilder BorderColor(byte red, byte green, byte blue, byte alpha = 255);
    public IUiContainerBuilder Scroll();
    public IUiContainerBuilder Center();
    public IUiContainerBuilder Width(float width);
    public IUiContainerBuilder Height(float height);
    public IUiContainerBuilder WidthFraction(float width);
    public IUiContainerBuilder HeightFraction(float height);
    public IUiContainerBuilder Radius(int radius);
    public IUiContainerBuilder BorderWidth(int borderWidth);
    public IUiContainerBuilder Gap(int gap);
    public IUiContainerBuilder PaddingBottom(int paddingBottom);
    public IUiContainerBuilder PaddingTop(int paddingTop);
    public IUiContainerBuilder PaddingRight(int paddingRight);
    public IUiContainerBuilder PaddingLeft(int paddingLeft);
    public IUiContainerBuilder PaddingVertical(int paddingVertical);
    public IUiContainerBuilder PaddingHorizontal(int paddingHorizontal);
    public IUiContainerBuilder PaddingEx(int left = 0, int right = 0, int top = 0, int bottom = 0);
    public IUiContainerBuilder Padding(int padding);
    public IUiContainerBuilder XAlign(XAlign xAlign);
    public IUiContainerBuilder MAlign(MAlign xAlign);
    public IUiContainerBuilder Dir(Dir dir);
    public IUiContainerBuilder Clip(bool isClipped = true);
    public IUiContainerBuilder Relative();
    public IUiContainerBuilder Absolute(IUiContainerBuilder? container = null, int left = 0, int right = 0, int top = 0, int bottom = 0, bool disablePositioning = false);
    public IUiContainerBuilder Focusable(bool focusable = true);
    public IUiContainerBuilder ZIndex(int zIndex);
    public IUiContainerBuilder Hidden(bool hidden = true);
    public bool ContainsPoint(Vector2 point);
    public IUiContainerBuilder Blur(float sigma, int top = 0, int right = 0, int left = 0, int bottom = 0);
    public IUiContainerBuilder BlurColor(byte red, byte green, byte blue, byte alpha = 255);
    public bool IsNew { get; set; }
    public bool IsHovered { get; }
    public bool IsActive { get; set; }
    public bool HasFocusWithin { get; }
    public bool FocusIn { get; set; }
    public bool FocusOut { get; set; }
    public bool Clicked { get; }
    public float ComputedX { get; set; }

    public float ComputedY { get; set; }
    public float ComputedWidth { get; set; }

    public float ComputedHeight { get; set; }
}


public partial class UiContainer
{
    public IUiContainerBuilder Blur(float sigma, int top = 0, int right = 0, int left = 0, int bottom = 0)
    {
        BlurSigma = sigma;
        BlurOffset = new Quadrant(left, right, top, bottom);
        return this;
    }

    public IUiContainerBuilder BlurColor(byte red, byte green, byte blue, byte alpha = 255)
    {
        PBlurColor = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    public IUiContainerBuilder Color(byte red, byte green, byte blue, byte alpha = 255)
    {
        PColor = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    public IUiContainerBuilder BorderColor(byte red, byte green, byte blue, byte alpha = 255)
    {
        PBorderColor = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    public IUiContainerBuilder BorderWidth(int borderWidth)
    {
        PBorderWidth = borderWidth;
        return this;
    }

    // public IUiContainerBuilder MAlign(MAlign mAlign)
    // {
    //     PmAlign = mAlign;
    //     return this;
    // }
    //
    // public IUiContainerBuilder XAlign(XAlign xAlign)
    // {
    //     PxAlign = xAlign;
    //     return this;
    // }

    public IUiContainerBuilder Radius(int radius)
    {
        PRadius = radius;
        return this;
    }

    public IUiContainerBuilder Focusable(bool focusable = true)
    {
        PFocusable = focusable;
        return this;
    }

    public IUiContainerBuilder ZIndex(int zIndex)
    {
        PZIndex = zIndex;
        return this;
    }

    public IUiContainerBuilder Hidden(bool hidden = true)
    {
        PHidden = hidden;
        return this;
    }

    public IUiContainerBuilder Center()
    {
        PmAlign = EnumMAlign.Center;
        PxAlign = EnumXAlign.Center;
        return this;
    }

    public IUiContainerBuilder Width(float width)
    {
        PWidth = new SizeDefinition(width, SizeKind.Pixel);
        return this;
    }

    public IUiContainerBuilder Height(float height)
    {
        PHeight = new SizeDefinition(height, SizeKind.Pixel);
        return this;
    }

    public IUiContainerBuilder Clip(bool isClipped = true)
    {
        IsClipped = isClipped;
        return this;
    }

    public IUiContainerBuilder Relative()
    {
        PAbsolute = false;
        AbsoluteContainer = null;
        DisablePositioning = false;
        PAbsolutePosition = new Quadrant();
        return this;
    }

    public IUiContainerBuilder Absolute(IUiContainerBuilder? container = null, int left = 0, int right = 0, int top = 0, int bottom = 0, bool disablePositioning = false)
    {
        PAbsolute = true;
        AbsoluteContainer = container as UiContainer;
        DisablePositioning = disablePositioning;
        PAbsolutePosition = new Quadrant(left, right, top, bottom);
        return this;
    }

    public IUiContainerBuilder WidthFraction(float width)
    {
        PWidth = new SizeDefinition(width, SizeKind.Percentage);
        return this;
    }

    public IUiContainerBuilder HeightFraction(float height)
    {
        PHeight = new SizeDefinition(height, SizeKind.Percentage);
        return this;
    }

    public IUiContainerBuilder Padding(int padding)
    {
        PPadding = new Quadrant(padding, padding, padding, padding);
        return this;
    }

    public IUiContainerBuilder MAlign(MAlign mAlign)
    {
        PmAlign = mAlign;
        return this;
    }

    public IUiContainerBuilder XAlign(XAlign xAlign)
    {
        PxAlign = xAlign;
        return this;
    }

    public IUiContainerBuilder PaddingEx(int left = 0, int right = 0, int top = 0, int bottom = 0)
    {
        PPadding = new Quadrant(left, right, top, bottom);
        return this;
    }

    public IUiContainerBuilder PaddingHorizontal(int paddingHorizontal)
    {
        PPadding = PPadding with { Left = paddingHorizontal, Right = paddingHorizontal };
        return this;
    }

    public IUiContainerBuilder Scroll()
    {
        PCanScroll = true;
        return this;
    }

    public IUiContainerBuilder PaddingVertical(int paddingVertical)
    {
        PPadding = PPadding with { Top = paddingVertical, Bottom = paddingVertical };
        return this;
    }

    public IUiContainerBuilder Dir(Dir dir)
    {
        PDir = dir;
        return this;
    }

    public IUiContainerBuilder PaddingLeft(int paddingLeft)
    {
        PPadding = PPadding with { Left = paddingLeft };
        return this;
    }

    public IUiContainerBuilder PaddingRight(int paddingRight)
    {
        PPadding = PPadding with { Right = paddingRight };
        return this;
    }

    public IUiContainerBuilder PaddingTop(int paddingTop)
    {
        PPadding = PPadding with { Top = paddingTop };
        return this;
    }

    public IUiContainerBuilder PaddingBottom(int paddingBottom)
    {
        PPadding = PPadding with { Bottom = paddingBottom };
        return this;
    }

    public IUiContainerBuilder Gap(int gap)
    {
        PGap = gap;
        return this;
    }

}
