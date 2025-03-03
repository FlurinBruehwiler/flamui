using System.Numerics;
using Flamui.Drawing;
using Flamui.UiElements;

namespace Flamui;

public partial class Ui
{
    public void DrawRect(float x, float y, float width, float height, ColorDefinition colorDefinition)
    {
        Window.RenderContext.AddRect(new Bounds(new Vector2(x, y), new Vector2(width, height)), OpenElement as UiElement, colorDefinition);
    }

    public void DrawPath(GlPath path, ColorDefinition colorDefinition)
    {
        Window.RenderContext.AddPath(path, colorDefinition);
    }
}