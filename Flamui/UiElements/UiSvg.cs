using System.Numerics;
using Flamui.Drawing;
using Flamui.Layouting;
using Point = Flamui.Layouting.Point;

namespace Flamui.UiElements;

//Todo, we should probably offload svg loading onto a separate thread, or do them async etc., or maybe preload them
public class UiSvg : UiElement
{
    private float factor;
    public string Src { get; set; } = null!;
    public ColorDefinition? ColorDefinition { get; set; }
    private static readonly Dictionary<string, Bitmap> SSvgCache = new();

    public override void Render(RenderContext renderContext, Point offset)
    {
        var bitmap = GetBitmap();

        renderContext.AddBitmap(bitmap, new Bounds(new Vector2(offset.X, offset.Y), new Vector2(bitmap.Width, bitmap.Height) * factor));
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
        var svg = GetBitmap();

        var svgRatio = svg.Width / svg.Height;

        //try to be as big as possible given the constraints
        var availableRatio = constraint.MaxWidth / constraint.MaxHeight;

        if (availableRatio > svgRatio) //Height is the limiting factor
        {
            factor = constraint.MaxHeight / svg.Height;
        }
        else //Width is the limiting factor
        {
            factor = constraint.MaxWidth / svg.Width;
        }

        Rect = new BoxSize(svg.Width * factor, svg.Height * factor);
        return Rect;
    }


    private unsafe Bitmap GetBitmap()
    {
        if (!SSvgCache.TryGetValue(Src, out var bitmap))
        {
            var bytes = File.ReadAllBytes(Src);

            fixed (byte* tvg = bytes)
            {
                TinyvgBitmap tinyvgBitmap = default;
                var err = TinyVG.tinyvg_render_bitmap(tvg, bytes.Length, TinyvgAntiAlias.X16, 100, 100, ref tinyvgBitmap);
                if (err != TinyvgError.Success)
                {
                    throw new Exception($"Error rendering svg: {err}");
                }

                bitmap = new Bitmap
                {
                    Width = tinyvgBitmap.Width,
                    Height = tinyvgBitmap.Height,
                    Data = new Slice<byte>((byte*)tinyvgBitmap.Pixels, (int)(tinyvgBitmap.Width * tinyvgBitmap.Height * 4)),
                    BitmapFormat = BitmapFormat.RGBA
                };
            }

            SSvgCache.Add(Src, bitmap);
        }

        return bitmap;
    }
}
