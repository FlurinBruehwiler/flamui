using Flamui.UiElements;

namespace Flamui.Components;

public class DebugWindow(EventLoop eventLoop) : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        ui.Window.IsDebugWindow = true;

        var otherWindow = eventLoop.Windows.First(x => x != ui.Window);

        using (ui.Div().Padding(3).ShrinkHeight())
        {
            if (ui.Button("Select"))
            {
                Ui.DebugSelectionModelEnabled = !Ui.DebugSelectionModelEnabled;
            }
        }

        using (ui.Div().Dir(Dir.Horizontal).Padding(10).Gap(10).Color(ColorPalette.BackgroundColor))
        {
            using (ui.Div().Gap(5).Scroll())
            {
                DisplayUiElement(ui, otherWindow.RootContainer, 39210, 0);
            }

            using (ui.Div())
            {
                if (Ui.DebugSelectedUiElement is not null)
                {
                    DisplayDetail(ui, Ui.DebugSelectedUiElement);
                }
            }
        }
    }

    private void DisplayUiElement(Ui ui, UiElement uiElement, int parentHash, int indentationLevel)
    {
        var key = Ui.S(uiElement.Id.GetHashCode() + parentHash);

        using (var div = ui.Div(key).PaddingLeft(indentationLevel * 20).Height(20).Rounded(2).Dir(Dir.Horizontal)
                   .Gap(5).XAlign(XAlign.Center))
        {
            if (uiElement is UiElementContainer { Children.Count: > 0 })
            {
                using (ui.Div().Height(15).Width(15).Color(C.Blue500))
                {
                }
            }
            else
            {
                using (ui.Div().Height(15).Width(15))
                {
                }
            }

            ui.Text(ToString(uiElement));

            if (div.IsClicked)
            {
                Ui.DebugSelectedUiElement = uiElement;
            }


            if (Ui.DebugSelectedUiElement == uiElement)
            {
                div.Color(C.Black / 5);
            }
            else if (div.IsHovered)
            {
                div.Color(C.Black / 8);
            }
            else
            {
                div.Color(C.Transparent);
            }
        }

        if (uiElement is UiElementContainer container)
        {
            foreach (var containerChild in container.Children)
            {
                DisplayUiElement(ui, containerChild, uiElement.Id.GetHashCode(), indentationLevel + 1);
            }
        }
    }

    private void DisplayDetail(Ui ui, UiElement uiElement)
    {
        foreach (var propertyInfo in uiElement.GetType().GetProperties())
        {
            using (ui.Div(propertyInfo.Name).Height(20))
            {
                ui.Text($"{propertyInfo.Name}: {propertyInfo.GetValue(uiElement)}");
            }
        }
    }

    private string ToString(UiElement uiElement)
    {
        if (uiElement is UiContainer)
        {
            return nameof(UiContainer);
        }

        if (uiElement is UiText uiText)
        {
            return Ui.S(uiText.Content, x => $"{nameof(uiText)}: {x}");
        }

        if (uiElement is UiImage uiImage)
        {
            return Ui.S(uiImage.Src, x => $"{nameof(uiText)}: {x}");
        }

        if (uiElement is UiSvg uiSvg)
        {
            return Ui.S(uiSvg.Src, x => $"{nameof(uiText)}: {x}");
        }

        return uiElement.ToString() ?? string.Empty;
    }
}
