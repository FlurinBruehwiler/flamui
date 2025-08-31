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
                switch (command.Type)
                {
                    case CommandType.Rect:
                        arenaList.Add(new RectInfo
                        {
                            TopLeft = command.RectCommand.Bounds.TopLeft().Multiply(currentMatrix),
                            BottomRight = command.RectCommand.Bounds.BottomRight().Multiply(currentMatrix),
                            Color = command.RectCommand.Color.ToVec4(),
                            BorderWidth = command.RectCommand.BorderWidth.Multiply(currentMatrix),
                            CornerRadius = command.RectCommand.Radius.Multiply(currentMatrix),
                            ShadowBlur = command.RectCommand.ShadowBlur
                        });
                        break;
                    case CommandType.Matrix:
                        currentMatrix = command.MatrixCommand.Matrix;
                        break;
                    case CommandType.ClipRect:
                    {
                        GlCanvas2.IssueDrawCall(renderer, arenaList.AsSlice().ReadonlySpan);
                        arenaList.Clear();

                        renderer.Gl.Enable(EnableCap.ScissorTest);

                        var bounds = command.ClipRectCommand.Bounds.Multiply(currentMatrix);
                        if (command.ClipRectCommand.ClipMode == ClipMode.OnlyDrawWithin)
                        {
                            renderer.Gl.Scissor((int)bounds.X, (int)(renderer.Window.Size.Y - (bounds.Y + bounds.H)), (uint)bounds.W, (uint)bounds.H);
                        }

                        break;
                    }
                    case CommandType.ClearClip:
                        GlCanvas2.IssueDrawCall(renderer, arenaList.AsSlice().ReadonlySpan);
                        arenaList.Clear();

                        renderer.Gl.Disable(EnableCap.ScissorTest);
                        break;
                    case CommandType.Text:
                    {
                        var resolutionMultiplier = new Vector2(1, 1).Multiply(currentMatrix.GetScale()).Y;

                        var fontAtlas = renderer.GetFontAtlas(new ScaledFont(command.TextCommand.Font.Get()!, command.TextCommand.FontSize));

                        var xCoord = command.TextCommand.Bounds.X;

                        foreach (var c in command.TextCommand.String.AsSpan())
                        {
                            var glyphInfo = fontAtlas.FindGlyphEntry(c, resolutionMultiplier);

                            float x = xCoord + glyphInfo.LeftSideBearing;
                            float y = command.TextCommand.Bounds.Y + fontAtlas.Font.Ascent + glyphInfo.YOff;

                            // Console.WriteLine($"{c}: {fontAtlas.Font.Ascent}: {glyphInfo.YOff}, {glyphInfo.AtlasHeight}, {glyphInfo.Height}");
                            // DrawGlyph(fontAtlas, glyphInfo, fontAtlas.GpuTexture, xCoord + glyphInfo.LeftSideBearing, command.TextCommand.Bounds.Y + fontAtlas.Font.Ascent + glyphInfo.YOff);
                            var uvXOffset = (1 / (float)fontAtlas.AtlasWidth) * glyphInfo.AtlasX;
                            var uvYOffset = (1 / (float)fontAtlas.AtlasHeight) * glyphInfo.AtlasY;
                            var uvWidth = (1 / (float)fontAtlas.AtlasWidth) * glyphInfo.AtlasWidth;
                            var uvHeight = (1 / (float)fontAtlas.AtlasHeight) * glyphInfo.AtlasHeight;

                            arenaList.Add(new RectInfo
                            {
                                TopLeft = new Vector2(x, y).Multiply(currentMatrix),
                                BottomRight = (new Vector2(x, y) + new Vector2(glyphInfo.Width, glyphInfo.Height)).Multiply(currentMatrix),
                                Color = command.TextCommand.Color.ToVec4(),
                                BorderWidth = 0,
                                CornerRadius = 0,
                                TextureCoordinate = new Vector4(uvXOffset, uvYOffset, uvWidth, uvHeight),
                                TextureHandle = fontAtlas.GpuTexture.TextureHandle
                            });

                            xCoord += glyphInfo.AdvanceWidth;
                            // Console.WriteLine($"Metrics: {c}:{glyphInfo.AdvanceWidth}:{glyphInfo.LeftSideBearing}");
                        }
                        break;
                    }
                    case CommandType.TinyVG:
                    {
                        renderer.VgAtlas ??= new VgAtlas(renderer);

                        var bounds = command.TinyVGCommand.Bounds.Multiply(currentMatrix);

                        var resolutionMultiplier = new Vector2(1, 1).Multiply(currentMatrix.GetScale()).Y;
                        var entry = renderer.VgAtlas.GetAtlasEntry(command.TinyVGCommand.VGId, command.TinyVGCommand.VGData.Span, (uint)(bounds.W * resolutionMultiplier),
                            (uint)(bounds.H * resolutionMultiplier));

                        var entryBounds = new Bounds(new Vector2(entry.X, entry.Y) / 1000, new Vector2(entry.Width, entry.Height) / 1000);

                        arenaList.Add(new RectInfo
                        {
                            TopLeft = bounds.TopLeft(),
                            BottomRight = bounds.BottomRight(),
                            Color = default,
                            BorderWidth = 0,
                            CornerRadius = 0,
                            TextureCoordinate = new Vector4(entryBounds.X, entryBounds.H, entryBounds.W, entryBounds.H),
                            TextureHandle = renderer.VgAtlas.GpuTexture.TextureHandle
                        });
                        break;
                    }
                }
            }
        }

        GlCanvas2.IssueDrawCall(renderer, arenaList.AsSlice().ReadonlySpan);
        renderer.Gl.Flush();

        renderer.DisplayRenderTextureOnScreen(renderer.mainRenderTexture);
    }
}