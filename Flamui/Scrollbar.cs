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
        Padding = 2, //ToDo padding doesn't work
        ThumbHoverColor = new ColorDefinition(92, 92, 92),
        TrackHoverColor = C.Transparent,
        ThumbRadius = 3
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

public static class Scrollbar
{
    public static bool IsFirst = true;

    public static void Build(Ui ui, ScrollService scrollService, ScrollbarSettings settings)
    {
        scrollService.MinBarSize = settings.MinTrackSize;

        if (!scrollService.IsScrolling)
            return;

        ref var isDragging = ref ui.Get(false);

        if (isDragging)
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

        using (var track = ui.Rect().Tag("Track"))
        {
            if (scrollService.Dir == Dir.Vertical)
            {
                track.Width(settings.Width);
            }
            else
            {
                track.Height(settings.Width);
            }

            track.Color(track.IsHovered || isDragging ? settings.TrackHoverColor : settings.TrackColor);

            using (var thumb = ui.Rect().Rounded(3).Tag("Thumb"))
            {
                thumb.Color(thumb.IsHovered || isDragging ? settings.ThumbHoverColor : settings.ThumbColor);

                if (scrollService.Dir == Dir.Vertical)
                {
                    thumb.AbsoluteSize(widthOffsetParent: - settings.Padding * 2); //this is a hacky way to do padding, should rework the absolute system, so we can just apply padding to the parent
                    thumb.AbsolutePosition(top: scrollService.BarStart + settings.Padding, left: settings.Padding);
                    thumb.Height(scrollService.BarSize - settings.Padding * 2);
                }
                else
                {
                    thumb.AbsoluteSize(heightOffsetParent: 0);
                    thumb.AbsolutePosition(left: scrollService.BarStart);
                    thumb.Width(scrollService.BarSize);
                }

                if (thumb.IsClicked)
                {
                    isDragging = true;
                    // SDL_CaptureMouse(SDL_bool.SDL_TRUE); //TODO
                }

                if(isDragging && ui.Tree.IsMouseButtonReleased(MouseButton.Left))
                {
                    isDragging = false;
                    // SDL_CaptureMouse(SDL_bool.SDL_FALSE); //TODO
                }
            }
        }
    }
}
