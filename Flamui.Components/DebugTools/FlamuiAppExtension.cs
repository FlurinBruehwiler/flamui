using Silk.NET.Input;

namespace Flamui.Components.DebugTools;

public static class FlamuiAppExtension
{
    public static FlamuiApp AddDebugWindow(this FlamuiApp flamuiApp)
    {
        flamuiApp.RegisterOnAfterInput(window =>
        {
            if (window.IsDebugWindow)
                return;

            if (window.IsKeyPressed(Key.F12))
            {
                flamuiApp.CreateWindow<DebugWindow>("Debug Window");
            }
        });
        return flamuiApp;
    }
}
