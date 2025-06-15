using Silk.NET.Input;

namespace Flamui;

public sealed class ScrollbarSettings
{
    public static ScrollbarSettings Default = new()
    {
        Width = 10,
        MinTrackSize = 50,
        ThumbColor = new ColorDefinition(92, 92, 92),
        TrackColor = C.Transparent,
        Padding = 5, //ToDo padding doesn't work
        ThumbHoverColor = new ColorDefinition(92, 92, 92),
        TrackHoverColor = new ColorDefinition(52, 53, 56, 100)
    };

    public float Width;
    public int ThumbRadius;
    public ColorDefinition TrackColor;
    public ColorDefinition TrackHoverColor;
    public ColorDefinition ThumbColor;
    public ColorDefinition ThumbHoverColor;
    public float MinTrackSize;
    public int Padding;
}

public sealed class Scrollbar(ScrollService scrollService, ScrollbarSettings settings)
{
    private bool _isDragging;

    public void Build(Ui ui)
    {
        scrollService.MinBarSize = settings.MinTrackSize;

        if (!scrollService.IsScrolling)
            return;

        if (_isDragging)
        {
            if (scrollService.Dir == Dir.Vertical)
            {
                scrollService.ApplyBarDelta(ui.Tree.MouseDelta.Y);
            }
            else
            {
                scrollService.ApplyBarDelta(ui.Tree.MouseDelta.X);
            }
        }

        using (var track = ui.Rect().Padding(settings.Padding))
        {
            if (scrollService.Dir == Dir.Vertical)
            {
                track.Width(settings.Width);
            }
            else
            {
                track.Height(settings.Width);
            }

            track.Color(track.IsHovered || _isDragging ? settings.TrackHoverColor : settings.TrackColor);

            using (var thumb = ui.Rect().Rounded(settings.ThumbRadius))
            {
                thumb.Color(thumb.IsHovered || _isDragging ? settings.ThumbHoverColor : settings.ThumbColor);

                if (scrollService.Dir == Dir.Vertical)
                {
                    thumb.AbsoluteSize(widthOffsetParent: 0);
                    thumb.AbsolutePosition(top: scrollService.BarStart);
                    thumb.Height(scrollService.BarSize);
                }
                else
                {
                    thumb.AbsoluteSize(heightOffsetParent: 0);
                    thumb.AbsolutePosition(left: scrollService.BarStart);
                    thumb.Width(scrollService.BarSize);
                }

                if (thumb.IsClicked)
                {
                    _isDragging = true;
                    // SDL_CaptureMouse(SDL_bool.SDL_TRUE); //TODO
                }

                if(_isDragging && ui.Tree.IsMouseButtonReleased(MouseButton.Left))
                {
                    _isDragging = false;
                    // SDL_CaptureMouse(SDL_bool.SDL_FALSE); //TODO
                }
            }
        }
    }
}
