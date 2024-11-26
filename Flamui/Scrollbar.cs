using Flamui.UiElements;

namespace Flamui;

public class ScrollbarSettings
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

public class Scrollbar(ScrollService scrollService, ScrollbarSettings settings) : FlamuiComponent
{
    private bool _isDragging;

    public override void Build(Ui ui)
    {
        scrollService.MinBarSize = settings.MinTrackSize;

        if (!scrollService.IsScrolling)
            return;

        if (_isDragging)
        {
            scrollService.ApplyBarDelta(ui.Window.MouseDelta.Y);
        }

        using (var track = ui.Div().Width(settings.Width).Padding(settings.Padding))
        {
            track.Color(track.IsHovered || _isDragging ? settings.TrackHoverColor : settings.TrackColor);

            using (var thumb = ui.Div().Height(scrollService.BarSize)
                       .AbsolutePosition(top: scrollService.BarStart)
                       .AbsoluteSize(widthOffsetParent:0).Rounded(settings.ThumbRadius))
            {
                thumb.Color(thumb.IsHovered || _isDragging ? settings.ThumbHoverColor : settings.ThumbColor);

                if (thumb.IsClicked)
                {
                    _isDragging = true;
                    SDL_CaptureMouse(SDL_bool.SDL_TRUE);
                }

                if(_isDragging && ui.Window.IsMouseButtonReleased(MouseButtonKind.Left))
                {
                    _isDragging = false;
                    SDL_CaptureMouse(SDL_bool.SDL_FALSE);
                }
            }
        }
    }
}
