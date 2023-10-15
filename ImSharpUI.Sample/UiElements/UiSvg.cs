﻿using SkiaSharp;
using Svg.Skia;

namespace ImSharpUISample.UiElements;

//Todo, we should probably offload svg loading onto a separate thread, or do them async etc., or maybe preload them
public class UiSvg : UiElement
{
    public string Src { get; set; } = null!;

    private static readonly Dictionary<string, SKSvg> SSvgCache = new();

    public override void Render(SKCanvas canvas)
    {
        if (!SSvgCache.TryGetValue(Src, out var svg))
        {
            svg = new SKSvg();
            svg.Load(Src);
            SSvgCache.Add(Src, svg);
        }

        if (svg.Picture is null)
            throw new Exception("unable to load svg");

        var svgSize = svg.Picture.CullRect;

        var availableRatio = PComputedWidth / PComputedHeight;
        var currentRatio = svgSize.Width / svgSize.Height;

        float factor;

        if (availableRatio > currentRatio) //Height is the limiting factor
        {
            factor = PComputedHeight / svgSize.Height;
        }
        else //Width is the limiting factor
        {
            factor = PComputedWidth / svgSize.Width;
        }

        var matrix = SKMatrix.CreateScale(factor, factor);

        matrix.TransX = PComputedX;
        matrix.TransY = PComputedY;

        canvas.DrawPicture(SSvgCache[Src].Picture, ref matrix);
    }

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
}