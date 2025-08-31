using System.Numerics;

namespace Flamui.UiElements;

public struct ScrollConfig
{
    public bool CanScroll;
    public bool OverlayScrollbar;

    public bool TakesUpSpace() => CanScroll && !OverlayScrollbar;
}

public enum RotationPivot
{
    Center,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

public struct FlexContainerInfo
{
    //maybe we don't want to expand as the default in the future, then we could get rid of this constructor!!! performance++
    public FlexContainerInfo()
    {
        WidthValue = 100;
        HeightValue = 100;
    }

    //----- Data ------
    public int ZIndex;
    public bool Focusable;
    public bool IsNew;
    public ColorDefinition? Color;
    public float BlurRadius;
    public ColorDefinition BorderColor;
    public Quadrant Padding;
    public float Rotation;
    public int Gap;
    public float Radius;
    public RotationPivot RotationPivot;
    public int BorderWidth;
    public ScrollConfig ScrollConfigX;
    public ScrollConfig ScrollConfigY;
    public FlexContainer? ClipToIgnore;
    public Dir Direction;
    public MAlign MainAlignment;
    public XAlign CrossAlignment;
    public bool AutoFocus; //todo
    public ColorDefinition ShadowColor;
    public Vector2 ShadowOffset;
    public float ShadowBlur;
    public float ShadowSpread;
    public bool Hidden;
    public bool BlockHit;
    public bool Interactable;
    public bool IsClipped;
    public float WidthValue;
    public SizeKind WidthKind;
    public float MinWidth;
    public float MinHeight;
    public float HeightValue;
    public SizeKind HeightKind;

    //----- Methods ------
    public float PaddingSizeMain()
    {
        var padding =  Padding.SumInDirection(Direction);
        if (ScrollConfigFromDirection(Direction).TakesUpSpace())
        {
            padding += ScrollbarSettings.Default.Width; //todo don't hardcode default settings
        }

        return padding;
    }

    public float PaddingSizeCross()
    {
        var padding = Padding.SumInDirection(Direction.Other());
        if (ScrollConfigFromDirection(Direction.Other()).TakesUpSpace())
        {
            padding += ScrollbarSettings.Default.Width; //todo don't hardcode default settings
        }

        return padding;
    }

    public ScrollConfig ScrollConfigFromDirection(Dir dir)
    {
        return dir switch
        {
            Dir.Vertical => ScrollConfigX,
            Dir.Horizontal => ScrollConfigY,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }

    public float GetMainSize()
    {
        return GetSizeInDirection(Direction);
    }

    public float GetSizeInDirection(Dir dir)
    {
        return dir switch
        {
            Dir.Horizontal => WidthValue,
            Dir.Vertical => HeightValue,
            _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null)
        };
    }

    public SizeKind GetMainSizeKind(Dir dir)
    {
        return dir switch
        {
            Dir.Horizontal => WidthKind,
            Dir.Vertical => HeightKind,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }
}
