using System.Numerics;
using Flamui.Drawing;
using Flamui.Layouting;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

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
                        var topLeft = command.RectCommand.Bounds.TopLeft().Multiply(currentMatrix);
                        var bottomRight = command.RectCommand.Bounds.BottomRight().Multiply(currentMatrix);

                        var info = new RectInfo
                        {
                            TopLeft = topLeft,
                            BottomRight = bottomRight,
                            Color = command.RectCommand.Color.ToVec4(),
                            BorderWidth = command.RectCommand.BorderWidth.Multiply(currentMatrix),
                            CornerRadius = command.RectCommand.Radius.Multiply(currentMatrix),
                            ShadowBlur = command.RectCommand.ShadowBlur,
                            TextureSlot = -1
                        };

                        if (command.RectCommand.BlurRadius != 0)
                        {
                            GlCanvas2.IssueDrawCall(renderer, arenaList.AsSlice().ReadonlySpan);
                            arenaList.Clear();
                            info.Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                            info.TextureSlot = renderer.ProduceBlurTexture(command.RectCommand.BlurRadius).TextureSlot;
                            info.TextureCoordinate = new Vector4(topLeft.X / renderer.UiTreeHost.GetSize().width,
                                ((topLeft.Y + (bottomRight.Y - topLeft.Y)) / renderer.UiTreeHost.GetSize().height),
                                (bottomRight.X - topLeft.X) / renderer.UiTreeHost.GetSize().width,
                                -(bottomRight.Y - topLeft.Y) / renderer.UiTreeHost.GetSize().height
                            ) ;
                        }

                        arenaList.Add(info);
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
                            renderer.Gl.Scissor((int)bounds.X, (int)(renderer.UiTreeHost.GetSize().height - (bounds.Y + bounds.H)), (uint)bounds.W, (uint)bounds.H);
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

                        var fontAtlas = renderer.FontAtlas;

                        var xCoord = command.TextCommand.Bounds.X;

                        var font = new ScaledFont(command.TextCommand.Font.Get()!, command.TextCommand.FontSize);

                        foreach (var c in command.TextCommand.String.AsSpan())
                        {
                            var glyphInfo = fontAtlas.FindGlyphEntry(font, c, resolutionMultiplier);

                            float x = xCoord + glyphInfo.LeftSideBearing;
                            float y = command.TextCommand.Bounds.Y + font.Ascent + glyphInfo.YOff;

                            //ok, i think know what the issue is now, there are multiple texture atlases apparently, and they get confused

                            // Console.WriteLine($"{c}: {fontAtlas.Font.Ascent}: {glyphInfo.YOff}, {glyphInfo.AtlasHeight}, {glyphInfo.Height}");
                            // DrawGlyph(fontAtlas, glyphInfo, fontAtlas.GpuTexture, xCoord + glyphInfo.LeftSideBearing, command.TextCommand.Bounds.Y + fontAtlas.Font.Ascent + glyphInfo.YOff);
                            var uvXOffset = (1 / (float)fontAtlas.AtlasWidth) * glyphInfo.AtlasX;
                            var uvYOffset = (1 / (float)fontAtlas.AtlasHeight) * (glyphInfo.AtlasY);// + (100 - glyphInfo.AtlasHeight));
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
                                TextureSlot = fontAtlas.GpuTexture.TextureSlot
                            });

                            xCoord += glyphInfo.AdvanceWidth;
                            // Console.WriteLine($"Metrics: {c}:{glyphInfo.AdvanceWidth}:{glyphInfo.LeftSideBearing}");
                        }
                        break;
                    }
                    case CommandType.TinyVG:
                    {
                        var bounds = command.TinyVGCommand.Bounds;

                        var resolutionMultiplier = new Vector2(1, 1).Multiply(currentMatrix.GetScale()).Y;
                        var entry = renderer.VgAtlas.GetAtlasEntry(command.TinyVGCommand.VGId, command.TinyVGCommand.VGData.Span, (uint)(bounds.W * resolutionMultiplier),
                            (uint)(bounds.H * resolutionMultiplier));

                        var entryBounds = new Bounds(new Vector2(entry.X, entry.Y) / 1000, new Vector2(entry.Width, entry.Height) / 1000);

                        arenaList.Add(new RectInfo
                        {
                            TopLeft = bounds.TopLeft().Multiply(currentMatrix),
                            BottomRight = bounds.BottomRight().Multiply(currentMatrix),
                            Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                            BorderWidth = 0,
                            CornerRadius = 0,
                            TextureCoordinate = new Vector4(entryBounds.X, entryBounds.Y, entryBounds.W, entryBounds.H),
                            TextureSlot = renderer.VgAtlas.GpuTexture.TextureSlot
                        });
                        break;
                    }
                    case CommandType.Picture:
                        //separate drawcall...
                        GlCanvas2.IssueDrawCall(renderer, arenaList.AsSlice().ReadonlySpan);
                        arenaList.Clear();

                        var pictureCommand = command.PictureCommand;
                        const int textureSlot = 2;

                        if (!renderer.GpuImageCache.TryGetValue(pictureCommand.Bitmap, out var texture))
                        {
                            texture = renderer.UploadTexture(pictureCommand.Bitmap, textureSlot);
                            renderer.GpuImageCache.Add(pictureCommand.Bitmap, texture);
                        }

                        renderer.Gl.ActiveTexture(GLEnum.Texture0 + textureSlot);
                        renderer.Gl.BindTexture(TextureTarget.Texture2D, texture.TextureId);
                        renderer.Gl.Uniform1(renderer.MainProgram.U_PictureTexture, textureSlot);

                        renderer.CheckError();

                        arenaList.Add(new RectInfo
                        {
                            TopLeft = pictureCommand.Bounds.TopLeft().Multiply(currentMatrix),
                            BottomRight = pictureCommand.Bounds.BottomRight().Multiply(currentMatrix),
                            TextureSlot = 3,
                            TextureCoordinate = new Vector4(0, 0, 1.0f, 1.0f),
                            Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        });
                        break;
                }
            }
        }

        GlCanvas2.IssueDrawCall(renderer, arenaList.AsSlice().ReadonlySpan);
        renderer.Gl.Flush();

        renderer.DisplayRenderTextureOnScreen(renderer.mainRenderTexture);
    }
}