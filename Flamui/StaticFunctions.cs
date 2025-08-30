using System.Numerics;
using Flamui.Drawing;
using Flamui.Layouting;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Flamui;

//pls explain to me why we can't have standalone functions in c#!!!!!!!!!
public static class StaticFunctions
{
    public static CommandBuffer Render(UiTree uiTree, Matrix4X4<float> mat)
    {
        var renderContext = uiTree._renderContext;
        renderContext.Reset();

        renderContext.PushMatrix(mat);
        uiTree.RootContainer.Render(renderContext, new Point());

        renderContext.PrintCommands(uiTree.Ui);

        return uiTree._renderContext.GetRenderInstructions();
    }

    public static void ExecuteRenderInstructions(CommandBuffer commands, Renderer renderer, Arena arena)
    {
        // var canvas = new GlCanvas(renderer, arena);

        renderer.BeforeFrame();

        var arenaList = new ArenaList<RectInfo>(arena, commands.InnerBuffers.Sum(x => x.Value.Count));

        Matrix4X4<float> currentMatrix = Matrix4X4<float>.Identity;

        foreach (var (_, value) in commands.InnerBuffers.OrderBy(x => x.Key))
        {
            foreach (var command in value)
            {
                if (command.Type == CommandType.Rect)
                {
                    arenaList.Add(new RectInfo
                    {
                        TopLeft = command.RectCommand.Bounds.TopLeft().Multiply(currentMatrix),
                        BottomRight = command.RectCommand.Bounds.BottomRight().Multiply(currentMatrix),
                        Color = command.RectCommand.Color.ToVec4(),
                        BorderWidth = command.RectCommand.BorderWidth,
                        CornerRadius = command.RectCommand.Radius.Multiply(currentMatrix)
                    });
                }else if (command.Type == CommandType.Matrix)
                {
                    currentMatrix = command.MatrixCommand.Matrix;
                }else if (command.Type == CommandType.ClipRect)
                {
                    GlCanvas2.IssueDrawCall(renderer, arenaList.AsSlice().ReadonlySpan);
                    arenaList.Clear();

                    renderer.Gl.Enable(EnableCap.ScissorTest);

                    var bounds = command.ClipRectCommand.Bounds.Multiply(currentMatrix);
                    if (command.ClipRectCommand.ClipMode == ClipMode.OnlyDrawWithin)
                    {
                        renderer.Gl.Scissor((int)bounds.X, (int)(renderer.Window.Size.Y - (bounds.Y + bounds.H)), (uint)bounds.W, (uint)bounds.H);
                    }
                }
                else if (command.Type == CommandType.ClearClip)
                {
                    GlCanvas2.IssueDrawCall(renderer, arenaList.AsSlice().ReadonlySpan);
                    arenaList.Clear();

                    renderer.Gl.Disable(EnableCap.ScissorTest);
                }

                // switch (command.Type)
                // {
                //     case CommandType.Rect:
                //         canvas.Paint.Color = command.RectCommand.Color;
                //         canvas.Paint.BlurRadius = command.RectCommand.BlurRadius;
                //         if(command.RectCommand.Radius == 0)
                //             canvas.DrawRect(command.RectCommand.Bounds.X, command.RectCommand.Bounds.Y, command.RectCommand.Bounds.W, command.RectCommand.Bounds.H);
                //         else
                //             canvas.DrawRoundedRect(command.RectCommand.Bounds.X, command.RectCommand.Bounds.Y, command.RectCommand.Bounds.W, command.RectCommand.Bounds.H, command.RectCommand.Radius);
                //         break;
                //     case CommandType.ClipRect:
                //         if(command.ClipRectCommand.Radius == 0)
                //             canvas.ClipRect(command.ClipRectCommand.Bounds.X, command.ClipRectCommand.Bounds.Y, command.ClipRectCommand.Bounds.W, command.ClipRectCommand.Bounds.H, command.ClipRectCommand.ClipMode);
                //         else
                //             canvas.ClipRoundedRect(command.ClipRectCommand.Bounds.X, command.ClipRectCommand.Bounds.Y, command.ClipRectCommand.Bounds.W, command.ClipRectCommand.Bounds.H, command.ClipRectCommand.Radius, command.ClipRectCommand.ClipMode);
                //         break;
                //     case CommandType.Text:
                //         canvas.Paint.Font = new ScaledFont(command.TextCommand.Font.Get(), command.TextCommand.FontSize);
                //         canvas.Paint.Color = command.TextCommand.Color;
                //         canvas.DrawText(command.TextCommand.String.AsSpan(), command.TextCommand.Bounds.X, command.TextCommand.Bounds.Y);
                //         break;
                //     case CommandType.Matrix:
                //         canvas.SetMatrix(command.MatrixCommand.Matrix);
                //         break;
                //     case CommandType.Picture:
                //         // canvas.DrawBitmap(command.Bitmap, command.Bounds);
                //         break;
                //     case CommandType.TinyVG:
                //         canvas.DrawTinyVG(command.TinyVGCommand.VGId, command.TinyVGCommand.VGData, command.TinyVGCommand.Bounds);
                //         break;
                //     case CommandType.ClearClip:
                //         canvas.ClearClip();
                //         break;
                //     default:
                //         throw new ArgumentOutOfRangeException(command.Type.ToString());
                // }
            }

        }

        GlCanvas2.IssueDrawCall(renderer, arenaList.AsSlice().ReadonlySpan);
        renderer.Gl.Flush();

        // canvas.Flush();

        renderer.DisplayRenderTextureOnScreen(renderer.mainRenderTexture);
    }
}