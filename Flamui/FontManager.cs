using Flamui.Drawing;

namespace Flamui;

public class FontManager
{
    private readonly Renderer? _renderer;
    public Dictionary<string, Font> fontCache = [];

    public FontManager(Renderer? renderer)
    {
        _renderer = renderer;
    }

    public Font GetFont(string fontName)
    {
        if (fontCache.TryGetValue(fontName, out var font))
        {
            return font;
        }

        font = LoadFont(fontName);
        fontCache.Add(fontName, font);

        return font;
    }

    private Font LoadFont(string fontToLoad)
    {
        var font = FontLoader.LoadFont("JetBrainsMono-Regular.ttf", 18);

        if(_renderer != null)
            font.GpuTexture = _renderer.UploadTexture(font.AtlasBitmap, (uint)font.AtlasWidth, (uint)font.AtlasHeight);

        return font;
    }
}