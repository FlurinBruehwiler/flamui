using System.ComponentModel;
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

    public static RenderTexture ExecuteRenderInstructions(CommandBuffer commands, Renderer renderer, int width, int height, bool isExternal)
    {
        var arena = Ui.Arena;

        renderer.BeforeFrame(width, height, isExternal);

        var arenaList = new ArenaList<RectInfo>(arena, commands.InnerBuffers.Sum(x => x.Value.Count));

        Matrix4X4<float> currentMatrix = Matrix4X4<float>.Identity;

        uint currentlyBoundArbitraryTexture = 743823452; //random number :)

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
                            GlCanvas2.IssueDrawCall(renderer, arenaList.AsSlice().ReadonlySpan, width, height);
                            arenaList.Clear();
                            var blurTexture = renderer.ProduceBlurTexture(command.RectCommand.BlurRadius);
                            BindArbitraryTexture(blurTexture);

                            info.Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                            info.TextureSlot = (int)TextureSlot.ArbitraryBitmap;
                            info.TextureCoordinate = new Vector4(topLeft.X / width,
                                ((topLeft.Y + (bottomRight.Y - topLeft.Y)) / height),
                                (bottomRight.X - topLeft.X) / width,
                                -(bottomRight.Y - topLeft.Y) / height
                            );
                        }

                        arenaList.Add(info);
                        break;
                    case CommandType.Matrix:
                        currentMatrix = command.MatrixCommand.Matrix;
                        break;
                    case CommandType.ClipRect:
                    {
                        GlCanvas2.IssueDrawCall(renderer, arenaList.AsSlice().ReadonlySpan, width, height);
                        arenaList.Clear();

                        renderer.Gl.Enable(EnableCap.ScissorTest);

                        var bounds = command.ClipRectCommand.Bounds.Multiply(currentMatrix);
                        if (command.ClipRectCommand.ClipMode == ClipMode.OnlyDrawWithin)
                        {
                            renderer.Gl.Scissor((int)bounds.X, (int)(height - (bounds.Y + bounds.H)), (uint)bounds.W, (uint)bounds.H);
                        }

                        break;
                    }
                    case CommandType.ClearClip:
                        GlCanvas2.IssueDrawCall(renderer, arenaList.AsSlice().ReadonlySpan, width, height);
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
                            var glyphInfo = fontAtlas.FindGlyphEntry(renderer.Gl, font, c, resolutionMultiplier);

                            float x = xCoord + glyphInfo.LeftSideBearing;
                            float y = command.TextCommand.Bounds.Y + font.Ascent + glyphInfo.YOff;

                            //ok, i think know what the issue is now, there are multiple texture atlases apparently, and they get confused

                            // Console.WriteLine($"{c}: {fontAtlas.Font.Ascent}: {glyphInfo.YOff}, {glyphInfo.AtlasHeight}, {glyphInfo.Height}");
                            // DrawGlyph(fontAtlas, glyphInfo, fontAtlas.GpuTexture, xCoord + glyphInfo.LeftSideBearing, command.TextCommand.Bounds.Y + fontAtlas.Font.Ascent + glyphInfo.YOff);
                            var uvXOffset = (1 / (float)fontAtlas.AtlasWidth) * glyphInfo.AtlasX;
                            var uvYOffset = (1 / (float)fontAtlas.AtlasHeight) * (glyphInfo.AtlasY); // + (100 - glyphInfo.AtlasHeight));
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
                                TextureSlot = (int)TextureSlot.FontAtlas
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
                            TextureSlot = (int)TextureSlot.IconAtlas
                        });
                        break;
                    }
                    case CommandType.Bitmap:

                        var pictureCommand = command.BitmapCommand;
                        if (!renderer.GpuImageCache.TryGetValue(pictureCommand.Bitmap, out var texture))
                        {
                            texture = renderer.UploadTexture(pictureCommand.Bitmap);
                            renderer.GpuImageCache.Add(pictureCommand.Bitmap, texture);
                        }

                        if (texture.TextureId != currentlyBoundArbitraryTexture)
                        {
                            GlCanvas2.IssueDrawCall(renderer, arenaList.AsSlice().ReadonlySpan, width, height);
                            arenaList.Clear();

                            BindArbitraryTexture(texture);
                        }

                        arenaList.Add(new RectInfo
                        {
                            TopLeft = pictureCommand.Bounds.TopLeft().Multiply(currentMatrix),
                            BottomRight = pictureCommand.Bounds.BottomRight().Multiply(currentMatrix),
                            TextureSlot = (int)TextureSlot.ArbitraryBitmap,
                            TextureCoordinate = new Vector4(
                                1.0f / pictureCommand.Bitmap.Width * pictureCommand.SubImage.X,
                                1.0f / pictureCommand.Bitmap.Height * pictureCommand.SubImage.Y,
                                1.0f / pictureCommand.Bitmap.Width * pictureCommand.SubImage.W,
                                1.0f / pictureCommand.Bitmap.Height * pictureCommand.SubImage.H),
                            Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        });
                        break;
                    case CommandType.GpuTexture:
                        var gpuTextureCommand = command.GpuTextureCommand;

                        if (gpuTextureCommand.GpuTexture.TextureId != currentlyBoundArbitraryTexture)
                        {
                            GlCanvas2.IssueDrawCall(renderer, arenaList.AsSlice().ReadonlySpan, width, height);
                            arenaList.Clear();

                            BindArbitraryTexture(gpuTextureCommand.GpuTexture);
                        }

                        arenaList.Add(new RectInfo
                        {
                            TopLeft = gpuTextureCommand.Bounds.TopLeft().Multiply(currentMatrix),
                            BottomRight = gpuTextureCommand.Bounds.BottomRight().Multiply(currentMatrix),
                            TextureSlot = (int)TextureSlot.ArbitraryBitmap,
                            TextureCoordinate =
                                new Vector4(
                                    1.0f / gpuTextureCommand.GpuTexture.Width * gpuTextureCommand.SubImage.X,
                                    1.0f / gpuTextureCommand.GpuTexture.Height * gpuTextureCommand.SubImage.Y,
                                    1.0f / gpuTextureCommand.GpuTexture.Width * gpuTextureCommand.SubImage.W,
                                    1.0f / gpuTextureCommand.GpuTexture.Height * gpuTextureCommand.SubImage.H),
                            Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        });
                        break;
                }
            }
        }

        GlCanvas2.IssueDrawCall(renderer, arenaList.AsSlice().ReadonlySpan, width, height);
        renderer.Gl.Flush();

        if (!isExternal)
        {
            renderer.DisplayRenderTextureOnScreen(renderer.mainRenderTexture, width, height);
        }

        renderer.AfterFrame(isExternal);

        return renderer.mainRenderTexture;

        void BindArbitraryTexture(GpuTexture gpuTexture)
        {
            renderer.Gl.ActiveTexture(GLEnum.Texture0 + (int)TextureSlot.ArbitraryBitmap);
            renderer.Gl.BindTexture(TextureTarget.Texture2D, gpuTexture.TextureId);
            currentlyBoundArbitraryTexture = gpuTexture.TextureId;
        }
    }
}