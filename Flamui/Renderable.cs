using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.InteropServices;
using Flamui.Drawing;
using Flamui.UiElements;
using Silk.NET.Maths;
using Varena;

namespace Flamui;

public class RenderContext
{
    public Stack<int> ZIndexes = new();

    public RenderContext()
    {
        ZIndexes.Push(0);
        Reset();
    }

    private Arena _arena;
    public Dictionary<int, GrowableArenaBuffer<Command>> CommandBuffers = [];

    private static VirtualArenaManager manager = new();

    public void Reset()
    {
        if (_arena == null)
        {
            var virtualBuffer = manager.CreateBuffer("PerFrameArena", (UIntPtr)1_000_000);
            _arena = new Arena(virtualBuffer);
        }

        CommandBuffers.Clear();
        _arena.Reset();
    }

    public void AddRect(Bounds bounds, UiElement uiElement, ColorDefinition color, float radius = 0)
    {
        var cmd = new Command();
        cmd.UiElement.Set(_arena, uiElement);
        cmd.Bounds = bounds;
        cmd.Radius = radius;
        cmd.Type = CommandType.Rect;
        cmd.Color = color;

        Add(cmd);
    }

    public void AddClipRect(Bounds bounds, float radius = 0)
    {
        var cmd = new Command();
        cmd.Bounds = bounds;
        cmd.Radius = radius;
        cmd.Type = CommandType.ClipRect;

        Add(cmd);
    }

    public void AddText(Bounds bounds, string text, ColorDefinition color, Font font, float fontPixelSize)
    {
        var cmd = new Command();
        cmd.Bounds = bounds;
        cmd.Type = CommandType.Text;
        cmd.String.Set(_arena, text);
        cmd.Color = color;
        cmd.Font.Set(_arena, font);
        cmd.FontPixelSize = fontPixelSize;

        Add(cmd);
    }

    public void AddMatrix(Matrix4X4<float> matrix)
    {
        var cmd = new Command();
        cmd.Matrix = matrix;
        cmd.Type = CommandType.Matrix;

        Add(cmd);
    }


    public void Add(Command command)
    {
        if (!CommandBuffers.TryGetValue(ZIndexes.Peek(), out var commandBuffer))
        {
            commandBuffer = new GrowableArenaBuffer<Command>(_arena, 20);
            CommandBuffers.Add(ZIndexes.Peek(), commandBuffer);
        }

        commandBuffer.Add(command);
    }

    public bool RequiresRerender(RenderContext lastRenderContext)
    {
        //maybe go first through everything and check if the sizes match up, and only then go ahead and compare the actual contents
        foreach (var (key, currentRenderSection) in CommandBuffers)
        {
            if (!lastRenderContext.CommandBuffers.TryGetValue(key, out var lastRenderSection))
            {
                return true;
            }

            // if (lastRenderSection.Count != currentRenderSection.Count)
            // {
            //     return true;
            // }

            //memcmp...
            // if (!currentRenderSection.MemCompare(currentRenderSection))
            //     return false;
        }

        return false;
    }

    private static int rerenderCount;

    public void Rerender(Renderer renderer)
    {
        var canvas = new GlCanvas(renderer, _arena);

        var sections = CommandBuffers.OrderBy(x => x.Key).ToList();

        foreach (var (_, value) in sections)
        {
            foreach (var command in value)
            {
                switch (command.Type)
                {
                    case CommandType.Rect:
                        canvas.Paint.Color = command.Color;
                        if(command.Radius == 0)
                            canvas.DrawRect(command.Bounds.X, command.Bounds.Y, command.Bounds.W, command.Bounds.H);
                        else
                            canvas.DrawRoundedRect(command.Bounds.X, command.Bounds.Y, command.Bounds.W, command.Bounds.H, command.Radius);
                        break;
                    case CommandType.ClipRect:
                        if(command.Radius == 0)
                            canvas.ClipRect(command.Bounds.X, command.Bounds.Y, command.Bounds.W, command.Bounds.H);
                        else
                            canvas.ClipRoundedRect(command.Bounds.X, command.Bounds.Y, command.Bounds.W, command.Bounds.H, command.Radius);
                        break;
                    case CommandType.Text:
                        canvas.Paint.Font = command.Font.Get();
                        canvas.Paint.Color = command.Color;
                        canvas.Paint.FontPixelSize = command.FontPixelSize;
                        canvas.DrawText(command.String.Get(), command.Bounds.X, command.Bounds.Y);
                        break;
                    case CommandType.Matrix:
                        canvas.SetMatrix(command.Matrix);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(command.Type.ToString());
                }
            }
        }

        canvas.Flush();
    }

    public void SetIndex(int idx)
    {
        ZIndexes.Push(idx);
    }

    public void RestoreZIndex()
    {
        ZIndexes.Pop();
    }
}

// public struct Save : IRenderFragment
// {
//     public void Render(SKCanvas canvas)
//     {
//         canvas.Save();
//     }
//
//     public bool UiEquals(IRenderFragment renderFragment)
//     {
//         return renderFragment is Save;
//     }
// }

// public struct Restore : IRenderFragment
// {
//     public void Render(SKCanvas canvas)
//     {
//         canvas.Restore();
//     }
//
//     public bool UiEquals(IRenderFragment renderFragment)
//     {
//         return renderFragment is Restore;
//     }
// }

public struct Bounds
{
    public required float X;
    public required float Y;
    public required float W;
    public required float H;

    public float Right => X + W;

    [Pure]
    public bool ContainsPoint(Vector2 point)
    {
        var withinX = point.X >= X && point.X <= X + W;
        var withinY = point.Y >= Y && point.Y <= Y + H;

        return withinX && withinY;
    }
    //
    // public SKRect ToRect()
    // {
    //     return SKRect.Create(X, Y, W, H);
    // }

    public Bounds Inflate(float x, float y)
    {
        return new Bounds
        {
            X = X - x,
            Y = Y - y,
            W = W + 2 * x,
            H = H + 2 * y
        };
    }

    public override string ToString()
    {
        return $"x:{X}, y:{Y}, w:{W}, h:{H}";
    }
}

public enum CommandType : byte
{
    Rect,
    ClipRect,
    Text,
    Matrix
}

public struct Command
{
    public CommandType Type;
    public ManagedRef<UiElement> UiElement;
    public ManagedRef<string> String;
    public Bounds Bounds;
    public float Radius;
    public ManagedRef<Font> Font;
    public float FontPixelSize;
    public ColorDefinition Color;
    public Matrix4X4<float> Matrix;
}

public struct ManagedRef<T> where T : class
{
    private GCHandle handle;

    public void Set(Arena arena, T value)
    {
        handle = arena.AddReference(value);
    }

    [Pure]
    public T Get()
    {
        if (handle.Target != null)
        {
            return (T)handle.Target;
        }

        throw new Exception("You sure this should be null????");
    }
}

// public struct Bitmap : IRenderFragment
// {
//     public required Bounds Bounds;
//     public required SKBitmap SkBitmap;
//     private static readonly SKPaint Paint = Helpers.GetNewAntialiasedPaint();
//
//     public void Render(SKCanvas canvas)
//     {
//         canvas.DrawBitmap(SkBitmap, Bounds.ToRect(), Paint);
//     }
//
//     public bool UiEquals(IRenderFragment renderFragment)
//     {
//         if (renderFragment is not Bitmap bitmap)
//             return false;
//
//         return bitmap.Bounds.BoundsEquals(Bounds);
//     }
// }

// public struct Picture : IRenderFragment //ToDo, should also be clickable
// {
//     public required SKPicture SkPicture;
//     public required SKMatrix SkMatrix;
//     public required string Src;
//
//     public void Render(SKCanvas canvas)
//     {
//         canvas.DrawPicture(SkPicture, ref SkMatrix);
//     }
//
//     public bool UiEquals(IRenderFragment renderFragment)
//     {
//         if (renderFragment is not Picture pic)
//             return false;
//
//         return pic.Src == Src && SkMatrix == pic.SkMatrix;
//     }
// }


// public struct Circle : IRenderFragment //ToDo, should also be clickable
// {
//     public required SKPaint SkPaint;
//     public required SKPoint Pos;
//     public required float Radius;
//
//     public void Render(SKCanvas canvas)
//     {
//         canvas.DrawCircle(Pos, Radius, SkPaint);
//     }
//
//     public bool UiEquals(IRenderFragment renderFragment)
//     {
//         if (renderFragment is not Circle circle)
//             return false;
//
//         return circle.Pos == Pos && circle.Radius == Radius;
//     }
// }

// public struct Path : IRenderFragment //ToDo maybe should also be clickable
// {
//     public required SKPoint Start;
//     public required SKPoint StartHandle;
//     public required SKPoint EndHandle;
//     public required SKPoint End;
//     public required SKPaint SkPaint;
//
//     private static SKPath _path = new();
//
//     public void Render(SKCanvas canvas)
//     {
//         _path.MoveTo(Start);
//         _path.CubicTo(StartHandle, EndHandle, End);
//
//         canvas.DrawPath(_path, SkPaint);
//
//         _path.Reset();
//     }
//
//     public bool UiEquals(IRenderFragment renderFragment)
//     {
//         if (renderFragment is not Path path)
//             return false;
//
//         return path.Start == Start && path.StartHandle == StartHandle && path.EndHandle == EndHandle &&
//                path.End == End;
//     }
// }