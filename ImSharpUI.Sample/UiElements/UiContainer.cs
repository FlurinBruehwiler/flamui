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

    public bool Clicked
    {
        get
        {
            if (Ui.Window is null)
                throw new Exception();

            if (Ui.Window.ClickPos is not { } clickPos)
                return false;

            if (DivContainsPoint(clickPos.X, clickPos.Y))
            {
                return true;
            }

            return false;
        }
    }

    public int PZIndex { get; set; }
    public bool PFocusable { get; set; }
    public bool IsNew { get; set; } = true;
    public ColorDefinition? PColor { get; set; }
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
    public UiContainer? AbsoluteContainer { get; set; }

    public bool PHidden { get; set; }

    public Quadrant PAbsolutePosition { get; set; } = new(0, 0, 0, 0);

    public bool IsHovered
    {
        get
        {
            if (Ui.Window is null)
                throw new Exception();

            if (DivContainsPoint(Ui.Window.MousePosition.X, Ui.Window.MousePosition.Y))
            {
                return true;
            }

            return false;
        }
    }

    public bool HasFocusWithin
    {
        get
        {
            if (IsActive)
                return true;

            foreach (var uiElement in OldChildrenById)
            {
                if (uiElement.Value is UiContainer { IsActive: true })
                    return true;
            }

            return false;
        }
    }


    public bool IsActive { get; set; }
    public bool PCanScroll { get; set; }
    public float ScrollPos { get; set; }
    public bool IsClipped { get; set; }

    private bool DivContainsPoint(double x, double y)
    {
        return PComputedX <= x && PComputedX + PComputedWidth >= x && PComputedY <= y &&
               PComputedY + PComputedHeight >= y;
    }

    public void OpenElement()
    {
        OldChildrenById.Clear();
        foreach (var uiElementClass in Children)
        {
            OldChildrenById.Add(uiElementClass.Id, uiElementClass);
        }

        Children.Clear();
    }

    public T AddChild<T>(UiElementId uiElementId) where T : UiElement, new()
    {
        if (OldChildrenById.TryGetValue(uiElementId, out var child))
        {
            Children.Add(child);
            return (T)child;
        }

        var newChild = new T
        {
            Id = uiElementId
        };

        Children.Add(newChild);
        return newChild;
    }

    public object? Data;

    public override void Render(SKCanvas canvas)
    {
        if (PColor is { } color)
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
        if (PBorderWidth != 0 && PBorderColor is {} borderColor)
        {
            canvas.Save();

            if (PRadius != 0)
            {
                float borderRadius = PRadius + PBorderWidth;

                canvas.ClipRoundRect(
                    new SKRoundRect(SKRect.Create(PComputedX, PComputedY, PComputedWidth, PComputedHeight), PRadius), SKClipOperation.Difference,
                    antialias: true);

                canvas.DrawRoundRect(PComputedX - PBorderWidth,
                    PComputedY - PBorderWidth,
                    PComputedWidth + 2 * PBorderWidth,
                    PComputedHeight + 2 * PBorderWidth,
                    borderRadius,
                    borderRadius,
                    GetColor(borderColor));
            }
            else
            {
                canvas.ClipRect(SKRect.Create(PComputedX, PComputedY, PComputedWidth, PComputedHeight), SKClipOperation.Difference, true);

                canvas.DrawRect(PComputedX - PBorderWidth, PComputedY - PBorderWidth,
                    PComputedWidth + 2 * PBorderWidth, PComputedHeight + 2 * PBorderWidth,
                    GetColor(borderColor));
            }

            canvas.Restore();
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
            if (childElement is UiContainer { PHidden: true })
            {
                continue;
            }

            //if differenz Z-index, defer rendering
            if (childElement is UiContainer uiContainer && uiContainer.PZIndex != 0)
            {
                Ui.DeferedRenderedContainers.Add(uiContainer);
                continue;
            }

            childElement.Render(canvas);
        }

        canvas.Restore();
    }


    public override void Layout(Window window)
    {
        IsNew = false;

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
            if (childElement is UiContainer { PHidden: true })
            {
                continue;
            }

            childElement.PComputedY -= ScrollPos;
            childElement.Layout(window);
        }
    }

    public bool ContainsPoint(double x, double y)
    {
        return PComputedX <= x && PComputedX + PComputedWidth >= x && PComputedY <= y &&
               PComputedY + PComputedHeight >= y;
    }
}
