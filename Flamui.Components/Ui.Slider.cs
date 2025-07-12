using System.Runtime.CompilerServices;
using Silk.NET.Input;

namespace Flamui.Components;

public static partial class UiExtensions
{
    public static void Slider(this Ui ui, float min, float max, ref float currentValue, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
    {
        using var _ = ui.CreateIdScope(file, lineNumber);

        //bounding box
        using (var boundingBox = ui.Rect().Height(30).MainAlign(MAlign.Center).Focusable().Color(C.Transparent))
        {
            //track
            using (var track = ui.Rect().Height(5).Color(ColorPalette.BorderColor).Rounded(2))
            {
                //head positioner
                using (ui.Rect().Color(ColorPalette.AccentColor).WidthFraction(MapRange(currentValue == 0 ? 0.0001f /* not sure why we need this hack, there seems to be a bug */ : currentValue, min, max, 0, 100)).CrossAlign(XAlign.End).Rounded(2))
                {
                    //head
                    using (ui.Rect().Width(20).Height(20))
                    {
                        using (ui.Rect().Circle(10).Color(C.Blue5).AbsolutePosition(left: 10, top: -6.5f))
                        {
                            if (ui.Tree.IsMouseButtonDown(MouseButton.Left) && boundingBox.HasFocusWithin)
                            {
                                var headPos = ui.Tree.MousePosition.X - track.FinalOnScreenSize.X;
                                currentValue = MapRange(headPos, 0, track.FinalOnScreenSize.W, min, max);
                                currentValue = Math.Clamp(currentValue, min, max);
                            }
                        }
                    }
                }
            }
        }
    }

    private static float MapRange(float value, float inMin, float inMax, float outMin, float outMax)
    {
        if (inMax == inMin)
            throw new ArgumentException("Input range cannot be zero.");

        float normalized = (value - inMin) / (inMax - inMin);
        return outMin + normalized * (outMax - outMin);
    }
}