using SkiaSharp;
using EnumXAlign = ImSharpUISample.XAlign;
using EnumMAlign = ImSharpUISample.MAlign;
using EnumDir = ImSharpUISample.Dir;

namespace ImSharpUISample.UiElements;

public partial class UiContainer : UiElement, IUiContainerBuilder
{
    public List<UiElement> Children { get; set; } = new();
    public Dictionary<UiElementId, UiElement> OldChildrenById { get; set; } = new();
    public bool FocusIn { get; set; }
    public bool FocusOut { get; set; }
    public bool Clicked { get; set; }

    public bool ClickedWithin //todo we can to a lot of optimization by caching this value and resetting it, only when the user clicks somewhere
    {
        get
        {
            var numOfCheckedElements = 0;

            foreach (var child in OldChildrenById)
            {
                if (child.Value is UiContainer uiContainer)
                {
                    numOfCheckedElements++;
                    if(uiContainer.ClickedWithin)
                        return true;
                }
            }

            if (numOfCheckedElements == 0)
                return Clicked;

            return false;
        }
    }

    public ColorDefinition? PColor { get; set; }
    public ColorDefinition? PHoverColor { get; set; }
    public ColorDefinition? PBorderColor { get; set; }
    public Quadrant PPadding { get; set; } = new(0, 0, 0, 0);
    public int PGap { get; set; }
    public int PRadius { get; set; }
    public int PBorderWidth { get; set; }
    public EnumDir PDir { get; set; } = EnumDir.Vertical;
    public MAlign PmAlign { get; set; } = EnumMAlign.FlexStart;
    public XAlign PxAlign { get; set; } = EnumXAlign.FlexStart;
    public Action? POnClick { get; set; }
    public bool PAutoFocus { get; set; }
    public bool PAbsolute { get; set; }
    public Quadrant PAbsolutePosition { get; set; } = new(0, 0, 0, 0);

    public bool IsHovered { get; set; }
    public bool IsActive { get; set; }
    public bool PCanScroll { get; set; }
    public float ScrollPos { get; set; }
    public bool IsClipped { get; set; }


    public override void Render(SKCanvas canvas)
    {
        if (GetColor() is { } color)
        {
            if (PBorderWidth != 0)
            {
                if (PRadius != 0)
                {
                    float borderRadius = PRadius + PBorderWidth;

                    canvas.DrawRoundRect(PComputedX - PBorderWidth,
                        PComputedY - PBorderWidth,
                        PComputedWidth + 2 * PBorderWidth,
                        PComputedHeight + 2 * PBorderWidth,
                        borderRadius,
                        borderRadius,
                        GetColor(PBorderColor ?? color));
                    canvas.DrawRoundRect(PComputedX,
                        PComputedY,
                        PComputedWidth,
                        PComputedHeight,
                        PRadius,
                        PRadius,
                        GetColor(color));
                }
                else
                {
                    canvas.DrawRect(PComputedX - PBorderWidth, PComputedY - PBorderWidth,
                        PComputedWidth + 2 * PBorderWidth, PComputedHeight + 2 * PBorderWidth,
                        GetColor(PBorderColor ?? color));
                    canvas.DrawRect(PComputedX, PComputedY, PComputedWidth, PComputedHeight,
                        GetColor(color));
                }
            }
            else
            {
                if (PRadius != 0)
                {
                    canvas.DrawRoundRect(PComputedX, PComputedY, PComputedWidth, PComputedHeight, PRadius, PRadius,
                        GetColor(color));
                }
                else
                {
                    canvas.DrawRect(PComputedX, PComputedY, PComputedWidth, PComputedHeight,
                        GetColor(color));
                }
            }
        }

        canvas.Save();

        if (PCanScroll || IsClipped)
        {
            if (PRadius != 0)
            {
                canvas.ClipRoundRect(
                    new SKRoundRect(SKRect.Create(PComputedX, PComputedY, PComputedWidth, PComputedHeight), PRadius),
                    antialias: true);
            }
            else
            {
                canvas.ClipRect(SKRect.Create(PComputedX, PComputedY, PComputedWidth, PComputedHeight));
            }
        }

        foreach (var childElement in Children)
        {
            childElement.Render(canvas);
        }

        canvas.Restore();
    }


    public override void Layout(Window window)
    {
        ComputeSize();

        var contentSize = ComputePosition();

        if (PCanScroll)
        {
            if (contentSize > PComputedHeight)
            {
                ScrollPos = Math.Clamp(ScrollPos + window.ScrollDelta * 20, 0, contentSize - PComputedHeight);
            }
            else
            {
                ScrollPos = 0;
            }
        }

        foreach (var childElement in Children)
        {
            childElement.PComputedY -= ScrollPos;
            childElement.Layout(window);
        }
    }
}
