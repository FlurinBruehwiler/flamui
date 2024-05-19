namespace Flamui.UiElements;

public struct FlexContainerInfo
{
    public int ZIndex;
    public bool Focusable;
    public bool IsNew;
    public ColorDefinition? Color;
    public ColorDefinition? BorderColor;
    public Quadrant Padding;
    public int Gap;
    public int Radius;
    public int BorderWidth;
    public FlexContainer? ClipToIgnore;
    public Dir Direction;
    public MAlign MainAlignment;
    public XAlign CrossAlignment;
    public bool AutoFocus;
    public bool Absolute;
    public bool DisablePositioning;
    public FlexContainer? AbsoluteContainer;
    public ColorDefinition? PShadowColor;
    public Quadrant ShaddowOffset;
    public float ShadowSigma;
    public bool Hidden;
    public bool BlockHit;
    public bool IsClipped;
    public AbsolutePosition AbsolutePosition;
    public float WidthValue;
    public SizeKind WidthKind;
    public float HeightValue;
    public SizeKind HeightKind;
    public bool ShrinkWidth;
    public bool ShrinkHeight;

    public float GetMainSize(Dir dir)
    {
        return dir switch
        {
            Dir.Horizontal => WidthValue,
            Dir.Vertical => HeightValue,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
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

public enum SizeKind
{
    Pixel,
    Percentage
}
