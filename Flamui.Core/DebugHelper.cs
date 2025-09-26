using System.Diagnostics;

namespace Flamui;

public static class DebugHelper
{
    public static void BreakIfKeyHit(Ui ui, Key key)
    {
        if (ui.Tree.IsKeyPressed(key))
        {
            Debugger.Break();
        }
    }
}