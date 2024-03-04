using SkiaSharp;
using EnumXAlign = Flamui.XAlign;
using EnumMAlign = Flamui.MAlign;
using EnumDir = Flamui.Dir;

namespace Flamui.UiElements;

public partial class UiContainer : UiElementContainer
{
    public bool FocusIn { get; set; } //todo
    public bool FocusOut { get; set; } //todo

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

    public bool IsDoubleClicked //todo
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

            foreach (var uiElement in DataStore.OldDataById) //ToDo, maybe old children again
            {
                if (uiElement.Value is UiContainer { IsActive: true })
                    return true;
            }

            return false;
        }
    }


    public bool IsActive { get; set; }
    public bool PCanScroll { get; set; }
    private float ScrollBarWidth;
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

        if (PCanScroll)
        {
            _scrollBarContainer.UiElement?.Render(renderContext);

            renderContext.Add(new Matrix
            {
                SkMatrix = SKMatrix.CreateTranslation(0, -ScrollPos)
            });
        }

        foreach (var childElement in Children)
        {
            if (childElement is UiContainer { PHidden: true })
            {
                continue;
            }

            childElement.Render(renderContext);
        }

        if (PCanScroll)
        {
            renderContext.Add(new Matrix
            {
                SkMatrix = SKMatrix.CreateTranslation(0, ScrollPos)
            });
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
    public Size ContentSize;

    private readonly EmptyStack _scrollBarContainer = new();


    private float LayoutScrollbar()
    {
        //TODO pls refactor this very ugly code!!!!!!!!!!!!!!!

        var scrollbar = Window.Ui.GetData(Id, this, static (_, _, scrollContainer) =>
        {
            var comp = new Scrollbar(new ScrollService(scrollContainer), new ScrollbarSettings
            {
                Width = 10,
                MinTrackSize = 50,
                ThumbColor = new ColorDefinition(92, 92, 92),
                TrackColor = C.Transparent,
                Padding = 5, //ToDo padding doesn't work
                ThumbHoverColor = new ColorDefinition(92, 92, 92),
                TrackHoverColor = new ColorDefinition(52, 52, 52)
            });
            comp.OnInitialized();
            return comp;
        });

        _scrollBarContainer.DataStore.Reset();

        Window.Ui.OpenElementStack.Push(_scrollBarContainer);
        scrollbar.Build(Window.Ui);
        Window.Ui.OpenElementStack.Pop();

        if (_scrollBarContainer.UiElement is null)
            return 0;

        var shadowParent = new UiContainer
        {
            Id = default,
            Window = Window,
            ComputedBounds = ComputedBounds,
            PmAlign = EnumMAlign.FlexEnd,
            PDir = EnumDir.Horizontal
        };

        shadowParent.AddChild(_scrollBarContainer.UiElement);
        shadowParent.Layout();

        return _scrollBarContainer.UiElement.ComputedBounds.W;
    }

    public override void Layout()
    {
        IsNew = false;

        if (PCanScroll)
        {
            ScrollBarWidth = LayoutScrollbar();
        }

        ComputeSize();

        ContentSize = ComputePosition();

        if (PHeight.Kind == SizeKind.Shrink)
        {
            ComputedBounds.H = ContentSize.Height;
        }

        if (PWidth.Kind == SizeKind.Shrink)
        {
            ComputedBounds.W = ContentSize.Width;
        }

        if (PCanScroll)
        {
            CalculateScrollPos();
        }

        foreach (var child in Children)
        {
            if (child.GetMainAxisSize().Kind != SizeKind.Shrink)
            {
                child.Layout();
            }
        }

        // foreach (var childElement in Children)
        // {
        //     if (childElement is UiContainer { PHidden: true })
        //     {
        //         continue;
        //     }
        //
        //     childElement.ComputedBounds.Y -= ScrollPos;
        // }
    }

    private float _scrollDelay;
    private float _targetScrollPos;
    private float _startScrollPos;

    private void CalculateScrollPos()
    {
        if (ContentSize.Height <= ComputedBounds.H)
        {
            ScrollPos = 0;
            return;
        }

        const float smoothScrollDelay = 150;

        if (Window.ScrollDelta != 0 && IsHovered)
        {
            _scrollDelay = smoothScrollDelay;
            _startScrollPos = ScrollPos;
            _targetScrollPos += Window.ScrollDelta * 65;
        }

        if (_scrollDelay > 0)
        {
            ScrollPos = Lerp(_startScrollPos, _targetScrollPos, 1 - _scrollDelay / smoothScrollDelay);
            _scrollDelay -= 16.6f;
        }
        else
        {
            _startScrollPos = ScrollPos;
            _targetScrollPos = ScrollPos;
        }

        ScrollPos = Math.Clamp(ScrollPos, 0, ContentSize.Height - ComputedBounds.H);
    }

    private float Lerp(float from, float to, float progress)
    {
        return from * (1 - progress) + to * progress;
    }
}

public class EmptyStack : IStackItem
{
    public DataStore DataStore { get; } = new();

    public UiElement? UiElement { get; set; }

    public void AddChild(object obj)
    {
        if (obj is UiElement uiElement)
        {
            UiElement = uiElement;
        }
    }
}
