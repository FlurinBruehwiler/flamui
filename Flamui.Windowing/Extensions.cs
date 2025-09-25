namespace Flamui.Windowing;

public static class Extensions
{
    public static Silk.NET.GLFW.CursorShape ToSilk(this CursorShape cursorShape)
    {
        return cursorShape switch
        {
            CursorShape.Default => Silk.NET.GLFW.CursorShape.Arrow,
            CursorShape.Arrow => Silk.NET.GLFW.CursorShape.Arrow,
            CursorShape.IBeam => Silk.NET.GLFW.CursorShape.IBeam,
            CursorShape.Crosshair => Silk.NET.GLFW.CursorShape.Crosshair,
            CursorShape.Hand => Silk.NET.GLFW.CursorShape.Hand,
            CursorShape.HResize => Silk.NET.GLFW.CursorShape.HResize,
            CursorShape.VResize => Silk.NET.GLFW.CursorShape.VResize,
            CursorShape.NwseResize => Silk.NET.GLFW.CursorShape.NwseResize,
            CursorShape.NeswResize => Silk.NET.GLFW.CursorShape.NeswResize,
            CursorShape.ResizeAll => Silk.NET.GLFW.CursorShape.AllResize,
            CursorShape.NotAllowed => Silk.NET.GLFW.CursorShape.NotAllowed,
            CursorShape.Wait => Silk.NET.GLFW.CursorShape.Arrow,
            CursorShape.WaitArrow => Silk.NET.GLFW.CursorShape.Arrow,
            _ => throw new ArgumentOutOfRangeException(nameof(cursorShape), cursorShape, null)
        };
    }
}