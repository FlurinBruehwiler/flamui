namespace Flamui.UiElements;

public partial class FlexContainer
{
    private float _scrollDelay;
    private float _targetScrollPos;
    private float _startScrollPos;

    public float ScrollPos { get; set; }
    public bool PCanScroll { get; set; }
    private float ScrollBarWidth;


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
}
