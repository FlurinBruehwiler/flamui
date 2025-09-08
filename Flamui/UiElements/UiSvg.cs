using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Flamui.Drawing;
using Flamui.Layouting;
using Point = Flamui.Layouting.Point;

namespace Flamui.UiElements;

public struct SvgInfo
{
    public float Height;
    public float Width;
    public ArenaString Src;
    public ColorDefinition? ColorDefinition;
}

//Todo, we should probably offload svg loading onto a separate thread, or do them async etc., or maybe preload them
public sealed class UiSvg : UiElement
{
    private static readonly Dictionary<int, (Slice<byte>, float aspectRatio)> SSvgCache = new();
    public SvgInfo Info;

    public UiSvg Height(float height)
    {
        Info.Height = height;
        return this;
    }

    public UiSvg Width(float width)
    {
        Info.Width = width;
        return this;
    }

    public UiSvg Color(ColorDefinition color)
    {
        Info.ColorDefinition = color;
        return this;
    }

    public override void Render(RenderContext renderContext, Point offset)
    {
        var bitmap = GetBitmap();

        renderContext.AddVectorGraphics(Info.Src.GetHashCode(), bitmap.Item1, new Bounds(offset.X, offset.Y, Rect.Width, Rect.Height));
    }

    public override void PrepareLayout(Dir dir)
    {
        if (Info.Width == 0 && Info.Height == 0)
        {
            FlexibleChildConfig = new FlexibleChildConfig
            {
                Percentage = 100
            };
        }
        base.PrepareLayout(dir);
    }

    public override BoxSize Layout(BoxConstraint constraint)
    {
        var (_, svgRatio) = GetBitmap();

        var height = constraint.MaxHeight;
        var width = constraint.MaxWidth;

        if (Info.Height != 0)
            height = Math.Min(height, Info.Height);

        if (Info.Width != 0)
            width = Math.Min(width, Info.Width);

        //try to be as big as possible given the constraints
        var availableRatio = width / height;

        if (availableRatio > svgRatio) //Height is the limiting factor
        {
            Rect = new BoxSize(height * svgRatio, height);
        }
        else //Width is the limiting factor
        {
            Rect = new BoxSize(width, width / svgRatio);
        }

        return Rect;
    }


    private unsafe (Slice<byte>, float aspectRatio) GetBitmap()
    {
        if (!SSvgCache.TryGetValue(Info.Src.GetHashCode(), out var entry))
        {
            var tvgPath = Path.Combine(Directory.GetParent(typeof(UiSvg).Assembly.Location)!.FullName, "Icons/TVG", Info.Src.ToString() + ".tvg");

            byte[] bytes = [];
            if (File.Exists(tvgPath))
            {
                bytes = File.ReadAllBytes(tvgPath);
            }
            else
            {
#if DEBUG
                var svgPath = Path.Combine(Renderer.DebugRootDirectory, "Icons/SVG", Info.Src.ToString() + ".svg");

                var tvgtPath = Path.Combine(Renderer.DebugRootDirectory, "Icons/SVG/temp.tvg");
                var debugTvgPath = Path.Combine(Renderer.DebugRootDirectory, "Icons/TVG/", Info.Src.ToString() + ".tvg");

                var svg2tvgProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "svg2tvgt",
                        Arguments = $"\"{svgPath}\" -o \"{tvgtPath}\"",
                        CreateNoWindow = true
                    }
                };
                svg2tvgProcess.Start();

                var tvgTextProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "tvg-text",
                        Arguments = $"-O tvg \"{tvgtPath}\" -o \"{debugTvgPath}\"",
                        CreateNoWindow = true
                    }
                };
                tvgTextProcess.Start();

                if (File.Exists(debugTvgPath))
                {
                    bytes = File.ReadAllBytes(debugTvgPath);
                }
#else
                Console.WriteLine("Unable to find TVG ${Info.Src.ToString()}")
#endif
            }

            var (width, height) = TinyVG.ParseHeader(bytes);

            var ptr = (byte*)Marshal.AllocHGlobal(bytes.Length);
            var dest = new Span<byte>(ptr, bytes.Length);
            bytes.AsSpan().CopyTo(dest);

            entry = (new Slice<byte>(ptr, bytes.Length), (float)width / (float)height);
            SSvgCache.Add(Info.Src.GetHashCode(), entry);
        }

        return entry;
    }

    public override void Reset()
    {
        Info = default;
        base.Reset();
    }
}
