using System.Runtime.CompilerServices;

namespace Flamui.Components;

public static partial class UiExtensions
{
    public static void DropDown<T>(this Ui ui, Span<T> options, ref T selectedOption, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
    {
        using var _ = ui.CreateIdScope(file, lineNumber);
        ui.GetObj<DropDown<T>>().Build(ui, ref selectedOption, options);
    }
}