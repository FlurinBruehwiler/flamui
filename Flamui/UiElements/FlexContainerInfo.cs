using EnumDir = Flamui.Dir;

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
    public EnumDir Direction;
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
}
