using Flamui.Drawing;
using Flamui.PerfTrace;
using Flamui.UiElements;
using Silk.NET.Maths;

namespace Flamui;

public enum ClipMode : byte
{
    OnlyDrawWithin,
    OnlyDrawOutside
}

/*

Think again what exactly a RenderContext is....




 */

public class RenderContext
{
    public Stack<int> ZIndexes = new();

    public RenderContext()
    {
        ZIndexes.Push(0);
        Arena = new Arena("PerFrameArena", 1_000_000);
    }

    public Arena Arena;
    public Dictionary<int, ArenaChunkedList<Command>> CommandBuffers = [];
    private Stack<Matrix4X4<float>> MatrixStack = [];
    private Stack<Command> ClipStack = [];

    public void Reset()
    {
        CommandBuffers.Clear();
        Arena.Reset();
        MatrixStack.Clear();
    }

    public void AddRect(Bounds bounds, UiElement? uiElement, ColorDefinition color, float radius = 0)
    {
        var cmd = new Command();
        cmd.UiElement = uiElement != null ? Arena.AddReference(uiElement) : default;
        cmd.Bounds = bounds;
        cmd.Radius = radius;
        cmd.Type = CommandType.Rect;
        cmd.Color = color;

        Add(cmd);
    }

    public void AddPath(GlPath path, ColorDefinition color)
    {
        var cmd = new Command();
        cmd.Path = path;
        cmd.Color = color;

        Add(cmd);
    }

    public void AddText(UiElement uiElement, Bounds bounds, ArenaString text, ColorDefinition color, ScaledFont scaledFont)
    {
        var cmd = new Command();
        cmd.Bounds = bounds;
        cmd.Type = CommandType.Text;
        cmd.String = text;
        cmd.Color = color;
        cmd.UiElement = Arena.AddReference(uiElement);
        cmd.Font = Arena.AddReference(scaledFont.Font);
        cmd.FontSize = scaledFont.PixelSize;

        Add(cmd);
    }

    public void AddBitmap(Bitmap bitmap, Bounds bounds)
    {
        var cmd = new Command();
        cmd.Bounds = bounds;
        cmd.Type = CommandType.Picture;
        cmd.Bitmap = bitmap;

        Add(cmd);
    }

    public void AddVectorGraphics(int vgId, Slice<byte> vgData, Bounds bounds)
    {
        var cmd = new Command();
        cmd.Type = CommandType.TinyVG;
        cmd.Bounds = bounds;
        cmd.VGId = vgId;
        cmd.VGData = vgData;

        Add(cmd);
    }

    public void PushClip(Bounds bounds, ClipMode clipMode, float radius = 0)
    {
        var cmd = new Command();
        cmd.Bounds = bounds;
        cmd.Radius = radius;
        cmd.Type = CommandType.ClipRect;
        cmd.ClipMode = clipMode;

        ClipStack.Push(cmd);

        Add(cmd);
    }

    public void PopClip()
    {
        ClipStack.Pop();

        if (ClipStack.TryPeek(out var cmd))
        {
            Add(cmd);
        }
        else
        {
            cmd = new Command();
            cmd.Type = CommandType.ClearClip;
            Add(cmd);
        }
    }

    /// <summary>
    /// Multiplies the current matrix with the new matrix
    /// </summary>
    /// <param name="matrix"></param>
    public void PushMatrix(Matrix4X4<float> matrix)
    {
        var prevMat = Matrix4X4<float>.Identity;
        if (MatrixStack.TryPeek(out var x))
        {
            prevMat = x;
        }

        var finalMat = Matrix4X4.Multiply(prevMat, matrix);

        MatrixStack.Push(finalMat);

        var cmd = new Command();
        cmd.Matrix = finalMat;
        cmd.Type = CommandType.Matrix;

        Add(cmd);
    }

    /// <summary>
    /// Resets the matrix to what it was before it was pushed
    /// </summary>
    public void PopMatrix()
    {
        MatrixStack.Pop();

        var mat = Matrix4X4<float>.Identity;
        if (MatrixStack.TryPeek(out var x))
        {
            mat = x;
        }

        var cmd = new Command();
        cmd.Matrix = mat;
        cmd.Type = CommandType.Matrix;

        Add(cmd);
    }

    public void Add(Command command)
    {
        if (!CommandBuffers.TryGetValue(ZIndexes.Peek(), out var commandBuffer))
        {
            commandBuffer = new ArenaChunkedList<Command>(Arena, 20);
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

    public void PrintCommands()
    {
        Console.WriteLine("---------------------------------------");
        var sections = CommandBuffers.OrderBy(x => x.Key).ToList();
        foreach (var (_, value) in sections)
        {
            Console.WriteLine("Section:");
            foreach (var command in value)
            {
                switch (command.Type)
                {
                    case CommandType.Rect:
                        Console.WriteLine($"Rect: {command.Bounds}, Line: {command.UiElement.Get().Id.Line}");
                        break;
                    case CommandType.ClipRect:
                        Console.WriteLine($"ClipRect: {command.Bounds}");
                        break;
                    case CommandType.Text:
                        Console.WriteLine($"Text: {command.String}, {command.Bounds}");
                        break;
                    case CommandType.Matrix:
                        Console.WriteLine($"Matrix: {command.Matrix}");
                        break;
                    case CommandType.TinyVG:
                        Console.WriteLine("VG:");
                        break;
                    case CommandType.Picture:
                        Console.WriteLine("Picture:");
                        break;
                    case CommandType.ClearClip:
                        Console.WriteLine("ClearClip:");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(command.Type.ToString());
                }
            }
        }

        Console.WriteLine("-----------------------------------------");
    }

    public CommandBuffer GetRenderInstructions()
    {
        return new CommandBuffer
        {
            InnerBuffers = CommandBuffers
        };
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