using Flamui.Drawing;
using Flamui.Layouting;
using Silk.NET.Maths;

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

        return uiTree._renderContext.GetRenderInstructions();
    }

    public static void ExecuteRenderInstructions(CommandBuffer commands, Renderer renderer, Arena arena)
    {
        var canvas = new GlCanvas(renderer, arena);

        foreach (var (_, value) in commands.InnerBuffers.OrderBy(x => x.Key))
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
                            canvas.ClipRect(command.Bounds.X, command.Bounds.Y, command.Bounds.W, command.Bounds.H, command.ClipMode);
                        else
                            canvas.ClipRoundedRect(command.Bounds.X, command.Bounds.Y, command.Bounds.W, command.Bounds.H, command.Radius, command.ClipMode);
                        break;
                    case CommandType.Text:
                        canvas.Paint.Font = new ScaledFont(command.Font.Get(), command.FontSize);
                        canvas.Paint.Color = command.Color;
                        canvas.DrawText(command.String.AsSpan(), command.Bounds.X, command.Bounds.Y);
                        break;
                    case CommandType.Matrix:
                        canvas.SetMatrix(command.Matrix);
                        break;
                    case CommandType.Picture:
                        canvas.DrawBitmap(command.Bitmap, command.Bounds);
                        break;
                    case CommandType.TinyVG:
                        canvas.DrawTinyVG(command.VGId, command.VGData, command.Bounds);
                        break;
                    case CommandType.ClearClip:
                        canvas.ClearClip();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(command.Type.ToString());
                }
            }
        }

        canvas.Flush();
    }
}