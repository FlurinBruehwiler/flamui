using System.Diagnostics;
using Silk.NET.Input;

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