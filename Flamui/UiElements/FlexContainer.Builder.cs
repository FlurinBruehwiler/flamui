using System.Numerics;
using System.Runtime.CompilerServices;
using EnumXAlign = Flamui.XAlign;
using EnumMAlign = Flamui.MAlign;

namespace Flamui.UiElements;

public sealed partial class FlexContainer
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer BlockHit(bool blockHit = true)
    {
        Info.BlockHit = blockHit;
        Info.Interactable = true;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Interactable()
    {
        Info.Interactable = true;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer DropShadow(float blur, float spread, float x = 0, float y = 0)
    {
        Info.ShadowBlur = blur;
        Info.ShadowSpread = spread;
        Info.ShadowOffset = new Vector2(x, y);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer ShadowColor(byte red, byte green, byte blue, byte alpha = 255)
    {
        Info.ShadowColor = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer ShadowColor(ColorDefinition colorDefinition)
    {
        Info.ShadowColor = colorDefinition;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Color(byte red, byte green, byte blue, byte alpha = 255)
    {
        Info.Color = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Color(ColorDefinition color)
    {
        Info.Color = color;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Blur(float blurRadius)
    {
        Info.BlurRadius = blurRadius;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer BorderColor(byte red, byte green, byte blue, byte alpha = 255)
    {
        Info.BorderColor = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer BorderColor(ColorDefinition color)
    {
        Info.BorderColor = color;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer BorderWidth(int borderWidth)
    {
        Info.BorderWidth = borderWidth;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer IgnoreClipFrom(FlexContainer div)
    {
        Info.ClipToIgnore = (FlexContainer)div;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Rounded(float radius)
    {
        Info.Radius = radius;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Tag(string debugTag)
    {
        UiElementInfo.DebugTag = debugTag;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Focusable(bool focusable = true)
    {
        Info.Focusable = focusable;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Circle(float radius)
    {
        Width(radius * 2);
        Height(radius * 2);
        Rounded(radius);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer ZIndex(int zIndex)
    {
        Info.ZIndex = zIndex;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Hidden(bool hidden = true)
    {
        Info.Hidden = hidden;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Center()
    {
        Info.MainAlignment = EnumMAlign.Center;
        Info.CrossAlignment = EnumXAlign.Center;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Size(float width, float height)
    {
        Info.WidthValue = width;
        Info.WidthKind = SizeKind.Pixel;
        Info.HeightValue = height;
        Info.HeightKind = SizeKind.Pixel;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Width(float width)
    {
        Info.WidthValue = width;
        Info.WidthKind = SizeKind.Pixel;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Height(float height)
    {
        Info.HeightValue = height;
        Info.HeightKind = SizeKind.Pixel;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Shrink(float minWidth = 0, float minHeight = 0)
    {
        Info.HeightKind = SizeKind.Shrink;
        Info.MinWidth = minWidth;
        Info.WidthKind = SizeKind.Shrink;
        Info.MinHeight = minHeight;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer ShrinkHeight(float minHeight = 0)
    {
        Info.HeightKind = SizeKind.Shrink;
        Info.MinHeight = minHeight;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer ShrinkWidth(float minWidth = 0)
    {
        Info.WidthKind = SizeKind.Shrink;
        Info.MinWidth = minWidth;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Clip(bool isClipped = true)
    {
        Info.IsClipped = isClipped;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer WidthFraction(float width)
    {
        Info.WidthValue = width;
        Info.WidthKind = SizeKind.Percentage;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer HeightFraction(float height)
    {
        Info.HeightValue = height;
        Info.HeightKind = SizeKind.Percentage;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer MainAlign(MAlign mAlign)
    {
        Info.MainAlignment = mAlign;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer CrossAlign(XAlign xAlign)
    {
        Info.CrossAlignment = xAlign;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer ScrollVertical(bool overlay = false)
    {
        Info.ScrollConfigY = new ScrollConfig
        {
            CanScroll = true,
            OverlayScrollbar = overlay,
        };
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer ScrollHorizontal(bool overlay = false)
    {
        Info.ScrollConfigX = new ScrollConfig
        {
            CanScroll = true,
            OverlayScrollbar = overlay,
        };
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Direction(Dir dir)
    {
        Info.Direction = dir;
        return this;
    }

    #region Padding

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Padding(int padding)
    {
        Info.Padding = new Quadrant(padding, padding, padding, padding);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Rotation(float rotationInDegrees, RotationPivot rotationPivot = RotationPivot.Center)
    {
        Info.Rotation = rotationInDegrees;
        Info.RotationPivot = rotationPivot;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer PaddingEx(int left = 0, int right = 0, int top = 0, int bottom = 0)
    {
        Info.Padding = new Quadrant(left, right, top, bottom);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer PaddingHorizontal(int paddingHorizontal)
    {
        Info.Padding = Info.Padding with { Left = paddingHorizontal, Right = paddingHorizontal };
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer PaddingVertical(int paddingVertical)
    {
        Info.Padding = Info.Padding with { Top = paddingVertical, Bottom = paddingVertical };
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer PaddingLeft(int paddingLeft)
    {
        Info.Padding = Info.Padding with { Left = paddingLeft };
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer PaddingRight(int paddingRight)
    {
        Info.Padding = Info.Padding with { Right = paddingRight };
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer PaddingTop(int paddingTop)
    {
        Info.Padding = Info.Padding with { Top = paddingTop };
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer PaddingBottom(int paddingBottom)
    {
        Info.Padding = Info.Padding with { Bottom = paddingBottom };
        return this;
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Gap(int gap)
    {
        Info.Gap = gap;
        return this;
    }
}
