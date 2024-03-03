namespace Flamui;

public class ScrollbarSettings
{
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

            using (var thumb = ui.Div().Height(scrollService.BarSize).Absolute(top: scrollService.BarStart).Rounded(settings.ThumbRadius))
            {
                thumb.Color(thumb.IsHovered || _isDragging ? settings.ThumbHoverColor : settings.ThumbColor);

                if (thumb.IsHovered && ui.Window.IsMouseButtonPressed(MouseButtonKind.Left))
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
