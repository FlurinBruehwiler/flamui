using System.Reflection;
using System.Runtime.InteropServices;
using Flamui.Drawing;
using Flamui.Layouting;
using Point = Flamui.Layouting.Point;

namespace Flamui.UiElements;

//Todo, we should probably offload svg loading onto a separate thread, or do them async etc., or maybe preload them
public sealed class UiSvg : UiElement
{
    public ArenaString Src { get; set; } = default;
    public ColorDefinition? ColorDefinition { get; set; }
    private static readonly Dictionary<int, (Slice<byte>, float aspectRatio)> SSvgCache = new();

    public override void Render(RenderContext renderContext, Point offset)
    {
        var bitmap = GetBitmap();

        renderContext.AddVectorGraphics(Src.GetHashCode(), bitmap.Item1, new Bounds(offset.X, offset.Y, Rect.Width, Rect.Height));
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
        var (_, svgRatio) = GetBitmap();

        //try to be as big as possible given the constraints
        var availableRatio = constraint.MaxWidth / constraint.MaxHeight;

        if (availableRatio > svgRatio) //Height is the limiting factor
        {
            Rect = new BoxSize(constraint.MaxHeight * svgRatio, constraint.MaxHeight);
        }
        else //Width is the limiting factor
        {
            Rect = new BoxSize(constraint.MaxWidth, constraint.MaxWidth / svgRatio);
        }

        return Rect;
    }


    private unsafe (Slice<byte>, float aspectRatio) GetBitmap()
    {
        if (!SSvgCache.TryGetValue(Src.GetHashCode(), out var entry))
        {
            var bytes = File.ReadAllBytes(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location)!.FullName, Src.ToString()));

            var (width, height) = TinyVG.ParseHeader(bytes);

            var ptr = (byte*)Marshal.AllocHGlobal(bytes.Length);
            var dest = new Span<byte>(ptr, bytes.Length);
            bytes.AsSpan().CopyTo(dest);

            entry = (new Slice<byte>(ptr, bytes.Length), (float)width / (float)height);
            SSvgCache.Add(Src.GetHashCode(), entry);
        }

        return entry;
    }

    public override void Reset()
    {
        Src = default;
        base.Reset();
    }
}
