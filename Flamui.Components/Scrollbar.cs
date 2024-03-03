namespace Flamui.Components;

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
    public override void Build(Ui ui)
    {
        scrollService.MinBarSize = settings.MinTrackSize;

        if (!scrollService.IsScrolling)
            return;

        using (var track = ui.Div().Width(settings.Width).Padding(settings.Padding))
        {
            track.Color(track.IsHovered ? settings.TrackHoverColor : settings.TrackColor);

            using (var thumb = ui.Div().Height(scrollService.BarSize).Absolute(top: scrollService.BarStart).Rounded(settings.ThumbRadius))
            {
                thumb.Color(thumb.IsHovered ? settings.ThumbHoverColor : settings.ThumbColor);
            }
        }
    }
}
