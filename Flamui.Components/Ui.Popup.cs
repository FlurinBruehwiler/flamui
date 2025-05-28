namespace Flamui.Components;

public static partial class UiExtensions
{
    public static Popup GetPopup(this Ui ui)
    {
        var popup = new Popup
        {
            Visible = ref ui.Get(false) //when setting this, we would only want it to take effect in the next frame....., this avoids all the problems with the order...
        };

        if (!popup.Visible)
            return popup;

        using (var backgorund = ui.Rect().SetParent(ui.Root).AbsoluteSize(0, 0).Center().BlockHit())
        {
            if (backgorund.IsClicked)
            {
                popup.Visible = false;
            }

            popup.Body = ui.CreateLayoutScope();

            return popup;
        }
    }
}

public ref struct Popup
{
    public ref bool Visible;
    public LayoutScope Body;
}