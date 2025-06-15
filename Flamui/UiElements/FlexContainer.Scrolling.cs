namespace Flamui.UiElements;

public struct ScrollInputInfo
{
    public float ScrollDelay;
    public float TargetScrollPos;
    public float StartScrollPos;

    public const float smoothScrollDelay = 150;

    public void ApplyInput(ref float scrollPos, float scrollDelta)
    {
        if (scrollDelta != 0)
        {
            ScrollDelay = smoothScrollDelay;
            StartScrollPos = scrollPos;
            TargetScrollPos += scrollDelta * 65;
        }
    }

    public void Smooth(ref float scrollPos)
    {
        if (ScrollDelay > 0)
        {
            scrollPos = Lerp(StartScrollPos, TargetScrollPos, 1 - ScrollDelay / smoothScrollDelay);
            ScrollDelay -= 16.6f;
        }
        else
        {
            StartScrollPos = scrollPos;
            TargetScrollPos = scrollPos;
        }
    }

    private static float Lerp(float from, float to, float progress)
    {
        return from * (1 - progress) + to * progress;
    }
}

public sealed partial class FlexContainer
{

    public float ScrollPosY;
    public float ScrollPosX;
    public ScrollInputInfo ScrollInputInfoX;
    public ScrollInputInfo ScrollInputInfoY;

    // public readonly EmptyStack _scrollBarContainerY = new();
    // public readonly EmptyStack _scrollBarContainerX = new();

    public float GetScrollPosInDir(Dir dir)
    {
        return dir switch
        {
            Dir.Vertical => ScrollPosY,
            Dir.Horizontal => ScrollPosX,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }

    private float LayoutScrollbar(Dir dir)
    {
        //TODO pls refactor this very ugly code!!!!!!!!!!!!!!!

        //todo id
        var scrollbar = Tree.Ui.GetData((container: this, dir: dir), static (_, hi) =>
        {
            var settings = ScrollbarSettings.Default;

            if (!hi.container.Info.ScrollConfigY.OverlayScrollbar)
            {
                settings.TrackColor = settings.TrackHoverColor;
            }

            return new Scrollbar(new ScrollService(hi.container, hi.dir), settings);
        });

        //todo re-add scrolling
/*
        var scrollBarContainer = dir == Dir.Horizontal ? _scrollBarContainerX : _scrollBarContainerY;

        scrollBarContainer.UiElement = null;
        scrollBarContainer.DataStore.Reset();

        // Tree.Ui.OpenElementStack.Push(scrollBarContainer);
        scrollbar.Build(Tree.Ui);
        // Tree.Ui.OpenElementStack.Pop();

        if (scrollBarContainer.UiElement is null)
            return 0;

        var size = scrollBarContainer.UiElement.Layout(new BoxConstraint(0, Rect.Width, 0, Rect.Height));
        if (dir == Dir.Vertical)
        {
            scrollBarContainer.UiElement.ParentData = new ParentData
            {
                Position = new Point(Rect.Width - size.Width, 0)
            };
        }
        else
        {
            scrollBarContainer.UiElement.ParentData = new ParentData
            {
                Position = new Point(0, Rect.Height - size.Height)
            };
        }

        return scrollBarContainer.UiElement.Rect.Width;
        */
        return 0;
    }


    private void CalculateScrollPos(ref float scrollPos, Dir dir, float wheelDelta)
    {
        if (ActualContentSize.GetDirection(dir) <= Rect.GetDirection(dir))
        {
            scrollPos = 0;
            return;
        }

        if (dir == Dir.Horizontal)
        {
            if (IsHovered)
            {
                ScrollInputInfoY.ApplyInput(ref scrollPos, wheelDelta);
            }
            ScrollInputInfoY.Smooth(ref scrollPos);
        }
        if (dir == Dir.Vertical)
        {
            if (IsHovered)
            {
                ScrollInputInfoX.ApplyInput(ref scrollPos, wheelDelta);
            }
            ScrollInputInfoX.Smooth(ref scrollPos);
        }

        scrollPos = Math.Clamp(scrollPos, 0, ActualContentSize.GetDirection(dir) - Rect.GetDirection(dir));
    }
}
