namespace Flamui.Components;

public static partial class UiExtensions
{
    public static Popup GetPopup(this Ui ui, bool allowDismissing = true)
    {
        var popup = new Popup
        {
            Visible = ref ui.Get(false) //when setting this, we would only want it to take effect in the next frame....., this avoids all the problems with the order...
        };

        if (!popup.Visible)
            return popup;

        using (var backgorund = ui.Rect().SetParent(ui.Root).AbsoluteSize(0, 0)
                   .Center().BlockHit().Color(C.Black / 4))
        {
            if (backgorund.IsClicked && allowDismissing)
            {
                popup.Visible = false;
                popup.WasDismissed = true;
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
    public bool WasDismissed;
}