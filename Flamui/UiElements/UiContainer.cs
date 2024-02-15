using SkiaSharp;
using EnumXAlign = Flamui.XAlign;
using EnumMAlign = Flamui.MAlign;
using EnumDir = Flamui.Dir;

namespace Flamui.UiElements;

public partial class UiContainer : UiElementContainer
{
    public bool FocusIn { get; set; }
    public bool FocusOut { get; set; }

    public bool IsClicked
    {
        get
        {
            if (Window is null)
                throw new Exception();

            if (!Window.IsMouseButtonPressed(MouseButtonKind.Left))
                return false;

            if (Window.HoveredElements.Contains(this))
            {
                return true;
            }

            return false;
        }
    }

    public bool IsDoubleClicked
    {
        get
        {
            return true;
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
    public UiContainer? ClipToIgnore { get; set; }
    public EnumDir PDir { get; set; } = EnumDir.Vertical;
    public MAlign PmAlign { get; set; } = EnumMAlign.FlexStart;
    public XAlign PxAlign { get; set; } = EnumXAlign.FlexStart;
    public Action? POnClick { get; set; }
    public bool PAutoFocus { get; set; }
    public bool PAbsolute { get; set; }
    public bool DisablePositioning { get; set; }
    public UiContainer? AbsoluteContainer { get; set; }
    public ColorDefinition? PShadowColor { get; set; }
    public Quadrant ShaddowOffset { get; set; }
    public float ShadowSigma { get; set; }
    public bool PHidden { get; set; }
    public bool PBlockHit { get; set; }

    public AbsolutePosition PAbsolutePosition { get; set; }

    public bool IsHovered
    {
        get
        {
            if (Window is null)
                throw new Exception();

            return Window.HoveredElements.Contains(this);
        }
    }

    public bool IsNewlyHovered
    {
        get
        {
            if (Window is null)
                throw new Exception();

            return !Window.OldHoveredElements.Contains(this) && Window.HoveredElements.Contains(this);
        }
    }

    public bool IsNewlyUnHovered
    {
        get
        {
            if (Window is null)
                throw new Exception();

            return Window.OldHoveredElements.Contains(this) && !Window.HoveredElements.Contains(this);
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

    public override void Render(RenderContext renderContext)
    {
        if (PZIndex != 0)
        {
            renderContext.SetIndex(PZIndex);
        }

        if (ClipToIgnore is not null)
        {
            renderContext.Add(new Restore());
        }

        if (PColor is { } color)
        {
            //shadow
            if (PShadowColor is { } blurColor)
            {
                float borderRadius = PRadius + PBorderWidth;

                //todo replace with readable code or something
                renderContext.Add(new Rect
                {
                    UiElement = this,
                    Bounds = new Bounds
                    {
                        X = ComputedBounds.X - PBorderWidth + ShaddowOffset.Left,
                        Y = ComputedBounds.Y - PBorderWidth + ShaddowOffset.Top,
                        H = ComputedBounds.H + 2 * PBorderWidth - ShaddowOffset.Top - ShaddowOffset.Bottom,
                        W = ComputedBounds.W + 2 * PBorderWidth - ShaddowOffset.Left - ShaddowOffset.Right,
                    },
                    Radius = PRadius == 0 ? 0 : borderRadius,
                    RenderPaint = new ShadowPaint
                    {
                        ShadowSigma = ShadowSigma,
                        SkColor = blurColor.ToSkColor()
                    }
                });
            }

            renderContext.Add(new Rect
            {
                UiElement = this,
                Bounds = ComputedBounds,
                Radius = PRadius,
                RenderPaint = new PlaintPaint
                {
                    SkColor = color.ToSkColor()
                }
            });
        }

        if (PBorderWidth != 0 && PBorderColor is {} borderColor)
        {
            renderContext.Add(new Save());

            float borderRadius = PRadius + PBorderWidth;

            renderContext.Add(new RectClip
            {
                Bounds = ComputedBounds,
                Radius = PRadius,
                ClipOperation = SKClipOperation.Difference
            });

            renderContext.Add(new Rect
            {
                UiElement = this,
                Bounds = new Bounds
                {
                    X = ComputedBounds.X - PBorderWidth,
                    Y = ComputedBounds.Y - PBorderWidth,
                    W = ComputedBounds.W + 2 * PBorderWidth,
                    H = ComputedBounds.H + 2 * PBorderWidth,
                },
                Radius = borderRadius,
                RenderPaint = new PlaintPaint
                {
                    SkColor = borderColor.ToSkColor()
                }
            });

            renderContext.Add(new Restore());
        }

        ClipContent(renderContext);

        foreach (var childElement in Children)
        {
            if (childElement is UiContainer { PHidden: true })
            {
                continue;
            }

            childElement.Render(renderContext);
        }

        if (NeedsClip())
        {
            renderContext.Add(new Restore());
        }

        //reapply clip
        ClipToIgnore?.ClipContent(renderContext);

        if (PZIndex != 0)
        {
            renderContext.RestoreZIndex();
        }
    }

    private void ClipContent(RenderContext renderContext)
    {
        if (NeedsClip())
        {
            renderContext.Add(new Save());

            renderContext.Add(new RectClip
            {
                Bounds = ComputedBounds,
                Radius = PRadius,
                ClipOperation = SKClipOperation.Intersect
            });
        }
    }

    private bool NeedsClip()
    {
        return PCanScroll || IsClipped;
    }

    public override void Layout()
    {
        IsNew = false;

        ComputeSize();

        var contentSize = ComputePosition();

        if (PHeight.Kind == SizeKind.Shrink)
        {
            ComputedBounds.H = contentSize.Heght;
        }

        if (PWidth.Kind == SizeKind.Shrink)
        {
            ComputedBounds.W = contentSize.Width;
        }

        if (PCanScroll)
        {
            if (contentSize.Heght > ComputedBounds.H)
            {
                ScrollPos = Math.Clamp(ScrollPos + Window.ScrollDelta * 20, 0, contentSize.Heght - ComputedBounds.H);
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

            childElement.ComputedBounds.Y -= ScrollPos;
        }
    }
}
