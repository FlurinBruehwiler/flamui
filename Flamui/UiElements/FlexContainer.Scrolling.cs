using Flamui.Layouting;

namespace Flamui.UiElements;

public partial class FlexContainer
{
    private float _scrollDelay;
    private float _targetScrollPos;
    private float _startScrollPos;

    public float ScrollPos { get; set; }
    private float ScrollBarWidth;

    public readonly EmptyStack _scrollBarContainer = new();

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

        var size = _scrollBarContainer.UiElement.Layout(new BoxConstraint(0, BoxSize.Width, 0, BoxSize.Height));
        _scrollBarContainer.UiElement.ParentData = new ParentData
        {
            Position = new Point(BoxSize.Width - size.Width, 0)
        };

        return _scrollBarContainer.UiElement.BoxSize.Width;
    }


    private void CalculateScrollPos()
    {
        if (ActualContentSize.Height <= BoxSize.Height)
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

        ScrollPos = Math.Clamp(ScrollPos, 0, ActualContentSize.Height - BoxSize.Height);
    }

    private static float Lerp(float from, float to, float progress)
    {
        return from * (1 - progress) + to * progress;
    }
}
