using System.Drawing;
using Flamui.Layouting;
using SkiaSharp;
using Svg;
using Svg.Model;
using Svg.Skia;
using Point = Flamui.Layouting.Point;

namespace Flamui.UiElements;

//Todo, we should probably offload svg loading onto a separate thread, or do them async etc., or maybe preload them
public class UiSvg : UiElement
{
    private float factor;
    public string Src { get; set; } = null!;
    public ColorDefinition? ColorDefinition { get; set; }
    private static readonly Dictionary<string, SKSvg> SSvgCache = new();

    public override void Render(RenderContext renderContext, Point offset)
    {
        // Console.WriteLine(factor);

        var matrix = SKMatrix.CreateScale(factor, factor);

        matrix.TransX = offset.X;
        matrix.TransY = offset.Y;

        renderContext.Add(new Picture
        {
            SkPicture = GetSvg().Picture!,
            SkMatrix = matrix,
            Src = Src
        });
    }

    public override void PrepareLayout(Dir dir)
    {
        FlexibleChildConfig = new FlexibleChildConfig
        {
            Percentage = 100
        };
        base.PrepareLayout(dir);
    }

    public override BoxSize Layout(BoxConstraint constraint)
    {
        var svg = GetSvg();

        var svgSize = svg.Picture!.CullRect;

        var svgRatio = svgSize.Width / svgSize.Height;

        //try to be as big as possible given the constraints
        var availableRatio = constraint.MaxWidth / constraint.MaxHeight;

        if (availableRatio > svgRatio) //Height is the limiting factor
        {
            factor = constraint.MaxHeight / svgSize.Height;
        }
        else //Width is the limiting factor
        {
            factor = constraint.MaxWidth / svgSize.Width;
        }

        BoxSize = new BoxSize(svgSize.Width * factor, svgSize.Height * factor);
        return BoxSize;
    }


    private SKSvg GetSvg()
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

        return svg;
    }
}
