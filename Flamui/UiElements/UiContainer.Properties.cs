using EnumXAlign = Flamui.XAlign;
using EnumMAlign = Flamui.MAlign;
using EnumDir = Flamui.Dir;

namespace Flamui.UiElements;

public partial class UiContainer
{
    public int PZIndex { get; set; }
    public bool PFocusable { get; set; }
    public bool IsNew { get; set; }
    public ColorDefinition? PColor { get; set; }
    public ColorDefinition? PBorderColor { get; set; }
    public Quadrant PPadding { get; set; }
    public int PGap { get; set; }
    public int PRadius { get; set; }
    public int PBorderWidth { get; set; }
    public UiContainer? ClipToIgnore { get; set; }
    public EnumDir PDir { get; set; }
    public MAlign PmAlign { get; set; }
    public XAlign PxAlign { get; set; }

    public bool PAutoFocus { get; set; }//what is this?
    public bool PAbsolute { get; set; }
    public bool DisablePositioning { get; set; }
    public UiContainer? AbsoluteContainer { get; set; }
    public ColorDefinition? PShadowColor { get; set; }
    public Quadrant ShaddowOffset { get; set; }
    public float ShadowSigma { get; set; }
    public bool PHidden { get; set; }
    public bool PBlockHit { get; set; }

    public AbsolutePosition PAbsolutePosition { get; set; }

    public override void CleanElement()
    {
        PZIndex = 0;
        PFocusable = false;
        IsNew = true;
        PColor = null;
        PBorderColor = null;
        PPadding = new Quadrant(0, 0, 0, 0);
        PGap = 0;
        PRadius = 0;
        PBorderWidth = 0;
        ClipToIgnore = null;
        PDir = EnumDir.Vertical;
        PmAlign = EnumMAlign.FlexStart;
        PxAlign = EnumXAlign.FlexStart;
        PAbsolute = false;
        DisablePositioning = false;
        AbsoluteContainer = null;
        PShadowColor = null;
        ShaddowOffset = new Quadrant(0, 0, 0, 0);
        ShadowSigma = 0;
        PHidden = false;
        PBlockHit = false;
        PAbsolutePosition = new AbsolutePosition();
        PWidth = new(100, SizeKind.Percentage);
        PHeight = new(100, SizeKind.Percentage);

        foreach (var uiElement in Children)
        {
            uiElement.CleanElement();
        }
    }
}
