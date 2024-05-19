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

    public FlexContainerInfo Info;
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
        if (Info.ZIndex != 0)
        {
            renderContext.SetIndex(Info.ZIndex);
        }

        if (Info.ClipToIgnore is not null)
        {
            renderContext.Add(new Restore());
        }

        if (Info.Color is { } color)
        {
            //shadow
            if (Info.PShadowColor is { } blurColor)
            {
                float borderRadius = Info.Radius + Info.BorderWidth;

                //todo replace with readable code or something
                renderContext.Add(new Rect
                {
                    UiElement = this,
                    Bounds = new Bounds
                    {
                        X = Info.BorderWidth + Info.ShaddowOffset.Left,
                        Y = Info.BorderWidth + Info.ShaddowOffset.Top,
                        H = BoxSize.Height + 2 * Info.BorderWidth - Info.ShaddowOffset.Top - Info.ShaddowOffset.Bottom,
                        W = BoxSize.Width + 2 * Info.BorderWidth - Info.ShaddowOffset.Left - Info.ShaddowOffset.Right,
                    },
                    Radius = Info.Radius == 0 ? 0 : borderRadius,
                    RenderPaint = new ShadowPaint
                    {
                        ShadowSigma = Info.ShadowSigma,
                        SkColor = blurColor.ToSkColor()
                    }
                });
            }

            renderContext.Add(new Rect
            {
                UiElement = this,
                Bounds = BoxSize.ToBounds(),
                Radius = Info.Radius,
                RenderPaint = new PlaintPaint
                {
                    SkColor = color.ToSkColor()
                }
            });
        }

        if (Info.BorderWidth != 0 && Info.BorderColor is {} borderColor)
        {
            renderContext.Add(new Save());

            float borderRadius = Info.Radius + Info.BorderWidth;

            renderContext.Add(new RectClip
            {
                Bounds = BoxSize.ToBounds(),
                Radius = Info.Radius,
                ClipOperation = SKClipOperation.Difference
            });

            renderContext.Add(new Rect
            {
                UiElement = this,
                Bounds = new Bounds
                {
                    X = Info.BorderWidth,
                    Y = Info.BorderWidth,
                    W = BoxSize.Width + 2 * Info.BorderWidth,
                    H = BoxSize.Height + 2 * Info.BorderWidth,
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
            if (childElement is FlexContainer { Info.Hidden: true })
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
        Info.ClipToIgnore?.ClipContent(renderContext);

        if (Info.ZIndex != 0)
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
                Bounds = BoxSize.ToBounds(),
                Radius = Info.Radius,
                ClipOperation = SKClipOperation.Intersect
            });
        }
    }

    private bool NeedsClip()
    {
        return PCanScroll || Info.IsClipped;
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
            // ComputedBounds = ComputedBounds,
            // PmAlign = EnumMAlign.FlexEnd,
            // Direction = EnumDir.Horizontal
        };

        shadowParent.AddChild(_scrollBarContainer.UiElement);
        // shadowParent.Layout();

        return _scrollBarContainer.UiElement.BoxSize.Width;
    }

    public override void PrepareLayout()
    {
        if (Info.GetMainSizeKind(Info.Direction) == SizeKind.Percentage)
        {
            FlexibleChildConfig = new FlexibleChildConfig
            {
                Percentage = Info.GetMainSize(Info.Direction)
            };
        }
        else
        {
            FlexibleChildConfig = null;
        }
    }

    public override BoxSize Layout(BoxConstraint constraint)
    {
        TightenConstraint(ref constraint);

        BoxSize = FlexSizeCalculator.ComputeSize(constraint, Children, Info.Direction);

        var actualSizeTakenUpByChildren = FlexPositionCalculator.ComputePosition(Children, Info.MainAlignment, Info.CrossAlignment, Info.Direction, BoxSize);

        return BoxSize;
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
