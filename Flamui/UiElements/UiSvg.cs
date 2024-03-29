﻿using System.Drawing;
using SkiaSharp;
using Svg;
using Svg.Model;
using Svg.Skia;

namespace Flamui.UiElements;

//Todo, we should probably offload svg loading onto a separate thread, or do them async etc., or maybe preload them
public class UiSvg : UiElement
{
    public string Src { get; set; } = null!;
    public ColorDefinition? ColorDefinition { get; set; }
    private static readonly Dictionary<string, SKSvg> SSvgCache = new();

    public override void Render(RenderContext renderContext)
    {
        if (!SSvgCache.TryGetValue(Src, out var svg))
        {
            Console.WriteLine($"Loading {Src}");
            svg = new SKSvg();
            var svgDoc = SvgExtensions.Open(Src);
            if (ColorDefinition is { } cd)
            {
                svgDoc!.Fill = new SvgColourServer(Color.FromArgb(cd.Alpha, cd.Red, cd.Green, cd.Blue));
            }
            svg.FromSvgDocument(svgDoc);
            SSvgCache.Add(Src, svg);
        }

        if (svg.Picture is null)
            throw new Exception("unable to load svg");

        var svgSize = svg.Picture.CullRect;

        var availableRatio = ComputedBounds.W / ComputedBounds.H;
        var currentRatio = svgSize.Width / svgSize.Height;

        float factor;

        if (availableRatio > currentRatio) //Height is the limiting factor
        {
            factor = ComputedBounds.H / svgSize.Height;
        }
        else //Width is the limiting factor
        {
            factor = ComputedBounds.W / svgSize.Width;
        }

        var matrix = SKMatrix.CreateScale(factor, factor);

        matrix.TransX = ComputedBounds.X;
        matrix.TransY = ComputedBounds.Y;

        renderContext.Add(new Picture
        {
            SkPicture = SSvgCache[Src].Picture!,
            SkMatrix = matrix,
            Src = Src
        });
    }

    public UiSvg()
    {
        CleanElement();
    }

    public override void Layout()
    {

    }

    public override void CleanElement()
    {
        Src = null;
        ColorDefinition = null;
        PHeight = new(100, SizeKind.Percentage);
        PWidth = new(100, SizeKind.Percentage);
    }
}
