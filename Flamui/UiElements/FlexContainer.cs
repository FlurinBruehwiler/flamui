using Flamui.Layouting;

namespace Flamui.UiElements;

public partial class FlexContainer : UiElementContainer
{
    public bool FocusIn { get; } //todo
    public bool FocusOut { get; } //todo

    public FlexContainerInfo Info = new();

    public override void Reset()
    {
        Info = new();
        base.Reset();
    }

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

    public override void Render(RenderContext renderContext, Point offset)
    {
        FlexContainerRenderer.Render(renderContext, this, offset);
    }

    public override void PrepareLayout(Dir dir)
    {
        if (Info.GetMainSizeKind(dir) == SizeKind.Percentage)
        {
            FlexibleChildConfig = new FlexibleChildConfig
            {
                Percentage = Info.GetSizeInDirection(dir)
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

        Rect = FlexSizeCalculator.ComputeSize(constraint, Children, Info).ApplyConstraint(constraint);

        ActualContentSize = FlexPositionCalculator.ComputePosition(Children, Rect, Info);

        if (Info.ScrollConfigY.CanScroll && Info.ScrollConfigX.CanScroll)
        {
            if (Window.IsKeyDown(SDL_Scancode.SDL_SCANCODE_LSHIFT))
            {
                CalculateScrollPos(ref ScrollPosX, Dir.Vertical, Window.ScrollDelta);
            }
            else
            {
                CalculateScrollPos(ref ScrollPosY, Dir.Vertical, Window.ScrollDelta);
            }

            LayoutScrollbar(Dir.Vertical);
            LayoutScrollbar(Dir.Horizontal);
        }
        else if (Info.ScrollConfigY.CanScroll)
        {
            CalculateScrollPos(ref ScrollPosY, Dir.Vertical, Window.ScrollDelta);
            LayoutScrollbar(Dir.Vertical);
        }
        else if (Info.ScrollConfigX.CanScroll)
        {
            CalculateScrollPos(ref ScrollPosX, Dir.Horizontal, Window.ScrollDelta);
            LayoutScrollbar(Dir.Horizontal);
        }

        AbsoluteLayouter.LayoutAbsoluteChildren(Children, Rect);

        return Rect;
    }

    private void TightenConstraint(ref BoxConstraint constraint)
    {
        //width
        if (!constraint.IsWidthTight())
        {
            if (UiElementInfo.AbsoluteInfo is { Size.WidthOffsetFromParent: { } widthOffsetFromParent })
            {
                var width = Parent.Rect.Width + widthOffsetFromParent;
                constraint.MaxWidth = width;
                constraint.MinWidth = width;
            }
            else if(Info.WidthKind == SizeKind.Percentage && !float.IsInfinity(constraint.MaxWidth))
            {
                var width = constraint.MaxWidth * (0.01f * Info.WidthValue);
                constraint.MaxWidth = width;
                constraint.MinWidth = width;

                //todo check that we don't comply with the constraints
            }
            else if (Info.WidthKind == SizeKind.Pixel)
            {
                var width = Info.WidthValue;
                constraint.MaxWidth = width;
                constraint.MinWidth = width;

                //todo check that we don't comply with the constraints
            }
            else if (Info.WidthKind == SizeKind.Shrink)
            {
                constraint.MinWidth = Info.MinWidth;
            }
        }

        //height
        if (!constraint.IsHeightTight())
        {
            if (UiElementInfo.AbsoluteInfo is { Size.HeightOffsetFromParent: { } heightOffset })
            {
                var height = Parent.Rect.Height + heightOffset;
                constraint.MaxHeight = height;
                constraint.MinHeight = height;
            }
            else if(Info.HeightKind == SizeKind.Percentage && !float.IsInfinity(constraint.MaxHeight))
            {
                var height = constraint.MaxHeight * (0.01f * Info.HeightValue);
                constraint.MaxHeight = height;
                constraint.MinHeight = height;

                //todo check that we don't comply with the constraints
            }
            else if (Info.HeightKind == SizeKind.Pixel)
            {
                var height = Info.HeightValue;
                constraint.MaxHeight = height;
                constraint.MinHeight = height;

                //todo check that we don't comply with the constraints
            }
            else if (Info.HeightKind == SizeKind.Shrink)
            {
                constraint.MinHeight = Info.MinHeight;
            }
        }
    }
}
