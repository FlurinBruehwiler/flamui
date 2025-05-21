namespace Flamui.Components;

public static partial class UiExtensions
{
    public static void DropDown<T>(this Ui ui, Span<T> options, ref T selectedOption)
    {
        ui.GetObj(new DropDown<T>()).Build(ui, ref selectedOption, options);
    }
}