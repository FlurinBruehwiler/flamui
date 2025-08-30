using Flamui.Drawing;
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

public sealed class RenderContext
{
    public Stack<int> ZIndexes = new();

    public RenderContext()
    {
        ZIndexes.Push(0);
    }

    public Dictionary<int, ArenaChunkedList<Command>> CommandBuffers = [];
    private Stack<Matrix4X4<float>> MatrixStack = [];
    private Stack<(Matrix4X4<float>, Command)> ClipStack = [];

    public void Reset()
    {
        CommandBuffers.Clear();
        MatrixStack.Clear();
    }

    /// <summary>
    /// If a bordeWidth is provided, only the outline of a rect will be rendered
    /// </summary>
    public void AddRect(Bounds bounds, UiElement? uiElement, ColorDefinition color, float radius = 0, float blurRadius = 0, float borderWidth = default)
    {
        var cmd = new Command
        {
            Type = CommandType.Rect,
            UiElementId = uiElement?.Id ?? 0,
            RectCommand = new RectCommand
            {
                Bounds = bounds,
                Radius = radius,
                Color = color,
                BlurRadius = blurRadius,
                BorderWidth = borderWidth
            }
        };

        Add(cmd);
    }

    public void AddText(UiElement uiElement, Bounds bounds, ArenaString text, ColorDefinition color, ScaledFont scaledFont)
    {
        var cmd = new Command
        {
            UiElementId = uiElement.Id,
            Type = CommandType.Text,
            TextCommand = new TextCommand
            {
                Bounds = bounds,
                String = text,
                Color = color,
                Font = Ui.Arena.AddReference(scaledFont.Font),
                FontSize = scaledFont.PixelSize,
            }
        };

        Add(cmd);
    }

    public void AddVectorGraphics(int vgId, Slice<byte> vgData, Bounds bounds)
    {
        var cmd = new Command
        {
            Type = CommandType.TinyVG,
            TinyVGCommand = new TinyVGCommand
            {
                Bounds = bounds,
                VGId = vgId,
                VGData = vgData
            },
            UiElementId = 0
        };

        Add(cmd);
    }

    public void PushClip(Bounds bounds, ClipMode clipMode, float radius = 0)
    {
        var cmd = new Command
        {
            Type = CommandType.ClipRect,
            ClipRectCommand = new ClipRectCommand
            {
                Bounds = bounds,
                Radius = radius,
                ClipMode = clipMode
            },
            UiElementId = 0
        };

        ClipStack.Push((GetCurrentMatrix(), cmd));

        Add(cmd);
    }

    public void PopClip()
    {
        ClipStack.Pop();

        if (ClipStack.TryPeek(out var cmd))
        {
            PushMatrix(cmd.Item1, multiply: false);
            Add(cmd.Item2);
            PopMatrix();
        }
        else
        {
            var c = new Command
            {
                Type = CommandType.ClearClip,
                ClearClipCommand = new ClearClipCommand(),
                UiElementId = 0
            };
            Add(c);
        }
    }

    /// <summary>
    /// Multiplies the current matrix with the new matrix
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="multiply"></param>
    public void PushMatrix(Matrix4X4<float> matrix, bool multiply = true)
    {
        var prevMat = Matrix4X4<float>.Identity;
        if (MatrixStack.TryPeek(out var x))
        {
            prevMat = x;
        }

        var finalMat = multiply ? Matrix4X4.Multiply(matrix, prevMat) : matrix;

        MatrixStack.Push(finalMat);

        var cmd = new Command
        {
            Type = CommandType.Matrix,
            MatrixCommand = new MatrixCommand
            {
                Matrix = finalMat,
            },
            UiElementId = 0
        };

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

        var cmd = new Command
        {
            Type = CommandType.Matrix,
            MatrixCommand = new MatrixCommand
            {
                Matrix = mat
            },
            UiElementId = 0
        };

        Add(cmd);
    }

    private Matrix4X4<float> GetCurrentMatrix()
    {
        var mat = Matrix4X4<float>.Identity;
        if (MatrixStack.TryPeek(out var x))
        {
            mat = x;
        }

        return mat;
    }

    public void Add(Command command)
    {
        if (!CommandBuffers.TryGetValue(ZIndexes.Peek(), out var commandBuffer))
        {
            commandBuffer = new ArenaChunkedList<Command>(Ui.Arena, 20);
            CommandBuffers.Add(ZIndexes.Peek(), commandBuffer);
        }

        commandBuffer.Add(command);
    }

    public bool RequiresRerender(RenderContext lastRenderContext)
    {
        //todo readd

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

    public void PrintCommands(Ui ui)
    {
        return;
        Console.WriteLine("---------------------------------------");
        var sections = CommandBuffers.OrderBy(x => x.Key).ToList();
        foreach (var (_, value) in sections)
        {
            Console.WriteLine("Section:");
            foreach (var command in value)
            {
                Console.Write($"ID: {command.UiElementId}, ");
                // if(command.GetAssociatedUiElement(ui)?.UiElementInfo.DebugTag != null)
                //     Console.Write($"Tag: {command.GetAssociatedUiElement(ui)?.UiElementInfo.DebugTag}, ");
                switch (command.Type)
                {
                    case CommandType.Rect:
                        Console.WriteLine($"Rect: {command.RectCommand.Bounds}");
                        break;
                    case CommandType.ClipRect:
                        Console.WriteLine($"ClipRect: {command.ClipRectCommand.Bounds}");
                        break;
                    case CommandType.Text:
                        Console.WriteLine($"Text: {command.TextCommand.String}, {command.TextCommand.Bounds}");
                        break;
                    case CommandType.Matrix:
                        Console.WriteLine($"Matrix: {command.MatrixCommand.Matrix}");
                        break;
                    case CommandType.TinyVG:
                        Console.WriteLine($"VG: {command.TinyVGCommand.VGId}");
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