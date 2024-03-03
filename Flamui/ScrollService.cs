using Flamui.UiElements;

namespace Flamui;

//ToDo: rethink this abstraction!!!

public struct ScrollService(UiContainer uiContainer)
{
    /// <summary>
    /// The size in pixels of the cutout of the content that is actually beeing shown to the user.
    /// </summary>
    public float CutoutSize => uiContainer.ComputedBounds.H; //ToDo, use the scroll direction

    /// <summary>
    /// The size in pixels of the content that can be scrolled
    /// </summary>
    public float ContentSize => uiContainer.ContentSize.Height; //ToDo, use the scroll direction

    /// <summary>
    /// The scroll position in pixels. 0 = start, n = end
    /// </summary>
    public float ScrollPos => uiContainer.ScrollPos;

    /// <summary>
    /// Scroll progress represented as a float. 0 = start, 1 = end
    /// </summary>
    public float ScrollProgress => ScrollPos / (ContentSize - CutoutSize);

    /// <summary>
    /// Where to start to draw the scroll bar. An offset in pixels to the start
    /// </summary>
    public float BarStart => (CutoutSize - BarSize) * ScrollProgress;

    /// <summary>
    /// The size of the scroll bar in pixels.
    /// </summary>
    public float BarSize => Math.Max(CutoutSize / (ContentSize / CutoutSize), MinBarSize);

    /// <summary>
    /// Defines the minimum scroll bar size
    /// </summary>
    public float MinBarSize { get; set; }

    /// <summary>
    /// Wether there is actually enought content for the scroll bar to be active.
    /// </summary>
    public bool IsScrolling => ContentSize > CutoutSize;

    public void ApplyBarDelta(float delta)
    {
        if (delta == 0)
            return;

        var maxBarStart = CutoutSize - BarSize;

        var newBarStart = Math.Clamp(BarStart + delta, 0, maxBarStart);
        var newScrollProgress = newBarStart / (CutoutSize - BarSize);
        var newScrollPos = newScrollProgress * (ContentSize - CutoutSize);
        uiContainer.ScrollPos = newScrollPos;
    }
}
