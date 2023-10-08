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
    public IUiContainerBuilder Padding(int left, int right, int top, int bottom);
    public IUiContainerBuilder Padding(int padding);
    public IUiContainerBuilder XAlign(XAlign xAlign);
    public IUiContainerBuilder MAlign(MAlign xAlign);
    public IUiContainerBuilder Dir(Dir dir);
    public IUiContainerBuilder Absolute(int left = 0, int right = 0, int top = 0, int bottom = 0);

    public bool IsHovered { get; set; }
    public bool IsActive { get; set; }
    public bool FocusIn { get; set; }
    public bool FocusOut { get; set; }
    public bool Clicked { get; set; }
}


public partial class UiContainer
{

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

    public IUiContainerBuilder Absolute(int left = 0, int right = 0, int top = 0, int bottom = 0)
    {
        PAbsolute = true;
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

    public IUiContainerBuilder Padding(int left, int right, int top, int bottom)
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
