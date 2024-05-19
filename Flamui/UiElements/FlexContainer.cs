using Flamui.Layouting;
using SkiaSharp;
using EnumXAlign = Flamui.XAlign;
using EnumMAlign = Flamui.MAlign;
using EnumDir = Flamui.Dir;

namespace Flamui.UiElements;

public partial class FlexContainer : UiElementContainer
{
    public bool FocusIn { get; set; } //todo
    public bool FocusOut { get; set; } //todo

    public FlexContainerInfo FlexContainerInfo;
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
                if (uiElement.Value is FlexContainer { IsActive: true })
                    return true;
            }

            return false;
        }
    }

    public override void Render(RenderContext renderContext)
    {
        if (FlexContainerInfo.ZIndex != 0)
        {
            renderContext.SetIndex(FlexContainerInfo.ZIndex);
        }

        if (FlexContainerInfo.ClipToIgnore is not null)
        {
            renderContext.Add(new Restore());
        }

        if (FlexContainerInfo.Color is { } color)
        {
            //shadow
            if (FlexContainerInfo.PShadowColor is { } blurColor)
            {
                float borderRadius = FlexContainerInfo.Radius + FlexContainerInfo.BorderWidth;

                //todo replace with readable code or something
                renderContext.Add(new Rect
                {
                    UiElement = this,
                    Bounds = new Bounds
                    {
                        X = ComputedBounds.X - FlexContainerInfo.BorderWidth + FlexContainerInfo.ShaddowOffset.Left,
                        Y = ComputedBounds.Y - FlexContainerInfo.BorderWidth + FlexContainerInfo.ShaddowOffset.Top,
                        H = ComputedBounds.H + 2 * FlexContainerInfo.BorderWidth - FlexContainerInfo.ShaddowOffset.Top - FlexContainerInfo.ShaddowOffset.Bottom,
                        W = ComputedBounds.W + 2 * FlexContainerInfo.BorderWidth - FlexContainerInfo.ShaddowOffset.Left - FlexContainerInfo.ShaddowOffset.Right,
                    },
                    Radius = FlexContainerInfo.Radius == 0 ? 0 : borderRadius,
                    RenderPaint = new ShadowPaint
                    {
                        ShadowSigma = FlexContainerInfo.ShadowSigma,
                        SkColor = blurColor.ToSkColor()
                    }
                });
            }

            renderContext.Add(new Rect
            {
                UiElement = this,
                Bounds = ComputedBounds,
                Radius = FlexContainerInfo.Radius,
                RenderPaint = new PlaintPaint
                {
                    SkColor = color.ToSkColor()
                }
            });
        }

        if (FlexContainerInfo.BorderWidth != 0 && FlexContainerInfo.BorderColor is {} borderColor)
        {
            renderContext.Add(new Save());

            float borderRadius = FlexContainerInfo.Radius + FlexContainerInfo.BorderWidth;

            renderContext.Add(new RectClip
            {
                Bounds = ComputedBounds,
                Radius = FlexContainerInfo.Radius,
                ClipOperation = SKClipOperation.Difference
            });

            renderContext.Add(new Rect
            {
                UiElement = this,
                Bounds = new Bounds
                {
                    X = ComputedBounds.X - FlexContainerInfo.BorderWidth,
                    Y = ComputedBounds.Y - FlexContainerInfo.BorderWidth,
                    W = ComputedBounds.W + 2 * FlexContainerInfo.BorderWidth,
                    H = ComputedBounds.H + 2 * FlexContainerInfo.BorderWidth,
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
            if (childElement is FlexContainer { FlexContainerInfo.Hidden: true })
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
        FlexContainerInfo.ClipToIgnore?.ClipContent(renderContext);

        if (FlexContainerInfo.ZIndex != 0)
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
                Radius = FlexContainerInfo.Radius,
                ClipOperation = SKClipOperation.Intersect
            });
        }
    }

    private bool NeedsClip()
    {
        return PCanScroll || FlexContainerInfo.IsClipped;
    }
    public BoxSize ContentSize;

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

        var shadowParent = new FlexContainer
        {
            Id = default,
            Window = Window,
            ComputedBounds = ComputedBounds,
            // PmAlign = EnumMAlign.FlexEnd,
            // Direction = EnumDir.Horizontal
        };

        shadowParent.AddChild(_scrollBarContainer.UiElement);
        // shadowParent.Layout();

        return _scrollBarContainer.UiElement.ComputedBounds.W;
    }

    public override BoxSize Layout(BoxConstraint constraint)
    {
        TightenConstraint(ref constraint);

        Size = FlexSizeCalculator.ComputeSize(constraint, Children, FlexContainerInfo.Direction);

        var actualSizeTakenUpByChildren = FlexPositionCalculator.ComputePosition(Children, FlexContainerInfo.MainAlignment, FlexContainerInfo.CrossAlignment, FlexContainerInfo.Direction, Size);

        return Size;
    }

    private void TightenConstraint(ref BoxConstraint constraint)
    {
        if (FlexibleChildConfig == null)
        {
            // var mainSize = Direction.GetMain(FixedWith, FixedHeight);
            // constraint.SetMain(Direction, mainSize, mainSize);
        }
    }

    private float Lerp(float from, float to, float progress)
    {
        return from * (1 - progress) + to * progress;
    }
}
