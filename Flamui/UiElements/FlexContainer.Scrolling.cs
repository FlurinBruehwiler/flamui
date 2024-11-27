using Flamui.Layouting;

namespace Flamui.UiElements;

public partial class FlexContainer
{
    private float _scrollDelay;
    private float _targetScrollPos;
    private float _startScrollPos;

    public float ScrollPosY;
    public float ScrollPosX;

    public readonly EmptyStack _scrollBarContainer = new();

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

        var scrollbar = Window.Ui.GetData(Id, (container: this, dir: dir), static (_, _, hi) =>
        {
            var settings = ScrollbarSettings.Default;

            if (!hi.container.Info.ScrollConfigY.OverlayScrollbar)
            {
                settings.TrackColor = settings.TrackHoverColor;
            }

            var comp = new Scrollbar(new ScrollService(hi.container, hi.dir), settings);
            comp.OnInitialized();
            return comp;
        });

        _scrollBarContainer.DataStore.Reset();

        Window.Ui.OpenElementStack.Push(_scrollBarContainer);
        scrollbar.Build(Window.Ui);
        Window.Ui.OpenElementStack.Pop();

        if (_scrollBarContainer.UiElement is null)
            return 0;

        var size = _scrollBarContainer.UiElement.Layout(new BoxConstraint(0, Rect.Width, 0, Rect.Height));
        if (dir == Dir.Vertical)
        {
            _scrollBarContainer.UiElement.ParentData = new ParentData
            {
                Position = new Point(Rect.Width - size.Width, 0)
            };
        }
        else
        {
            _scrollBarContainer.UiElement.ParentData = new ParentData
            {
                Position = new Point(0, Rect.Height - size.Height)
            };
        }

        return _scrollBarContainer.UiElement.Rect.Width;
    }


    private void CalculateScrollPos(ref float scrollPos, Dir dir)
    {
        if (ActualContentSize.GetDirection(dir) <= Rect.GetDirection(dir))
        {
            scrollPos = 0;
            return;
        }

        const float smoothScrollDelay = 150;

        if (Window.ScrollDelta != 0 && IsHovered)
        {
            _scrollDelay = smoothScrollDelay;
            _startScrollPos = scrollPos;
            _targetScrollPos += Window.ScrollDelta * 65;
        }

        if (_scrollDelay > 0)
        {
            scrollPos = Lerp(_startScrollPos, _targetScrollPos, 1 - _scrollDelay / smoothScrollDelay);
            _scrollDelay -= 16.6f;
        }
        else
        {
            _startScrollPos = scrollPos;
            _targetScrollPos = scrollPos;
        }

        scrollPos = Math.Clamp(scrollPos, 0, ActualContentSize.GetDirection(dir) - Rect.GetDirection(dir));
    }

    private static float Lerp(float from, float to, float progress)
    {
        return from * (1 - progress) + to * progress;
    }
}
