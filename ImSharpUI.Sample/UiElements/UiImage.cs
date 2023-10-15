﻿using SkiaSharp;

namespace ImSharpUISample.UiElements;

public class UiImage : UiElement
{
    public string Src { get; set; } = null!;

    public override void Render(SKCanvas canvas)
    {
        if (!ImgCache.TryGetValue(Src, out var img))
        {
            img = SKBitmap.Decode(Src);

            ImgCache.Add(Src, img);
        }

        if (img is null)
            return;

        var availableRatio = PComputedWidth / PComputedHeight;
        var currentRatio = img.Width / img.Height;

        float destHeight;
        float destWidth;

        if (availableRatio > currentRatio) //Height is the limiting factor
        {
            destHeight = PComputedHeight;
            destWidth = currentRatio * PComputedHeight;
        }
        else //Width is the limiting factor
        {
            destWidth = PComputedWidth;
            destHeight = PComputedWidth / currentRatio;
        }

        canvas.DrawBitmap(ImgCache[Src], new SKRect(PComputedX, PComputedY, PComputedX + destWidth, PComputedY + destHeight), Paint);
    }

    private static readonly SKPaint Paint = new()
    {
        IsAntialias = true
    };

    public override void Layout(Window window)
    {

    }

    public override bool LayoutHasChanged()
    {
        throw new NotImplementedException();
    }

    public override bool HasChanges()
    {
        throw new NotImplementedException();
    }

    private static readonly Dictionary<string, SKBitmap> ImgCache = new();
}