using Flamui.Drawing;

namespace Flamui;

//font, a ttf file i guess
//a font atlas, which is basically a font + size

public sealed class FontManager
{
    public Dictionary<string, Font> fontCache = [];

    public Font GetFont(string fontName)
    {
        if (fontCache.TryGetValue(fontName, out var font))
        {
            return font;
        }

        font = FontLoader.LoadFont(fontName);
        fontCache.Add(fontName, font);

        return font;
    }
}