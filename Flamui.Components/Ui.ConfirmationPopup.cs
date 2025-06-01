namespace Flamui.Components;

public static partial class UiExtensions
{
    public static ConfirmPopup GetConfirmationPopup(this Ui ui, ArenaString title, ArenaString subTitle)
    {
        var popup = ui.GetPopup(false);

        var confirmPopup = new ConfirmPopup();
        confirmPopup.Result = ConfirmationPopupResult.Undecided;
        confirmPopup.IsShowing = ref popup.Visible;

        if (popup.Visible)
        {
            using (popup.Body.Enter())
            {
                using (ui.Rect().Width(200).Height(100).Color(ColorPalette.BackgroundColor).Border(2, ColorPalette.BackgroundColor).BlockHit())
                {
                    ui.Text(title);
                    ui.Text(subTitle);

                    using (ui.Rect().Direction(Dir.Horizontal))
                    {
                        if (ui.Button("Cancel"))
                        {
                            confirmPopup.Result = ConfirmationPopupResult.Cancel;
                            popup.Visible = false;
                        }

                        if (ui.Button("Ok", primary: true))
                        {
                            confirmPopup.Result = ConfirmationPopupResult.Ok;
                            popup.Visible = false;
                        }
                    }
                }
            }
        }
        // else if (popup.WasDismissed)
        // {
        //     confirmPopup.Result = ConfirmationPopupResult.Cancel;
        //     popup.Visible = false;
        // }

        return confirmPopup;
    }
}

public ref struct ConfirmPopup
{
    public ConfirmationPopupResult Result;
    public ref bool IsShowing;

    public void Show()
    {
        IsShowing = true;
    }
}

public enum ConfirmationPopupResult
{
    Undecided,
    Ok,
    Cancel
}