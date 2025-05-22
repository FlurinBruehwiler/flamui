namespace Flamui.Components;

public static partial class UiExtensions
{
    public static void DropDown<T>(this Ui ui, Span<T> options, ref T selectedOption)
    {
        ui.GetObj<DropDown<T>>().Build(ui, ref selectedOption, options);
    }
}