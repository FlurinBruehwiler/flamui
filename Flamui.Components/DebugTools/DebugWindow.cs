using Flamui.UiElements;

namespace Flamui.Components.DebugTools;

public class DebugWindow() : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        ui.Window.IsDebugWindow = true;

        // var otherWindow = EventLoop.Windows.First(x => x != ui.Window);
        return;
        // using (ui.Div().Padding(3).ShrinkHeight())
        // {
        //     if (ui.Button("Select"))
        //     {
        //         Ui.DebugSelectionModelEnabled = !Ui.DebugSelectionModelEnabled;
        //     }
        // }
        //
        // using (ui.Div().Direction(Dir.Horizontal).Padding(10).Gap(10).Color(ColorPalette.BackgroundColor))
        // {
        //     using (ui.Div().Gap(5).ScrollVertical())
        //     {
        //         DisplayUiElement(ui, otherWindow.RootContainer, 39210, 0);
        //     }
        //
        //     using (ui.Div().ScrollVertical())
        //     {
        //         if (Ui.DebugSelectedUiElement is not null)
        //         {
        //             DisplayDetail(ui, Ui.DebugSelectedUiElement);
        //         }
        //     }
        // }
    }

    private void DisplayUiElement(Ui ui, UiElement flexContainer, int parentHash, int indentationLevel)
    {
        // var hashCode = flexContainer.Id.GetHashCode() + parentHash;
        // var key = Ui.S(hashCode);
        //
        // using (var div = ui.Div(key).PaddingLeft(indentationLevel * 20).Height(20).Rounded(2).Direction(Dir.Horizontal)
        //            .Gap(5).CrossAlign(XAlign.Center))
        // {
        //     if (flexContainer is UiElementContainer { Children.Count: > 0 })
        //     {
        //         using (ui.Div().Height(15).Width(15).Color(C.Blue5))
        //         {
        //         }
        //     }
        //     else
        //     {
        //         using (ui.Div().Height(15).Width(15))
        //         {
        //         }
        //     }
        //
        //     ui.Text(ToString(flexContainer)).Color(ColorPalette.TextColor);
        //
        //     if (div.IsClicked)
        //     {
        //         // Ui.DebugSelectedUiElement = flexContainer;
        //     }

            //
            // if (Ui.DebugSelectedUiElement == flexContainer)
            // {
            //     div.Color(C.Black / 5);
            // }
            // else if (div.IsHovered)
            // {
            //     div.Color(C.Black / 8);
            // }
            // else
            // {
            //     div.Color(C.Transparent);
            // }
        // }

        // if (flexContainer is UiElementContainer container)
        // {
        //     foreach (var containerChild in container.Children)
        //     {
        //         DisplayUiElement(ui, containerChild, hashCode, indentationLevel + 1);
        //     }
        // }
    }

    private void DisplayDetail(Ui ui, UiElement flexContainer)
    {
        foreach (var propertyInfo in flexContainer.GetType().GetProperties())
        {
            using (ui.Div(propertyInfo.Name).Height(20))
            {
                ui.Text($"{propertyInfo.Name}: {propertyInfo.GetValue(flexContainer)}").Color(ColorPalette.TextColor);
            }
        }
    }

    private string ToString(UiElement flexContainer)
    {
        if (flexContainer is FlexContainer)
        {
            return nameof(FlexContainer);
        }

        if (flexContainer is UiText uiText)
        {
            // return Ui.S(uiText.UiTextInfo.Content, x => $"{nameof(uiText)}: {x}");
        }

        if (flexContainer is UiImage uiImage)
        {
            // return Ui.S(uiImage.Src, x => $"{nameof(uiText)}: {x}");
        }

        if (flexContainer is UiSvg uiSvg)
        {
            // return Ui.S(uiSvg.Src, x => $"{nameof(uiText)}: {x}");
        }

        return flexContainer.ToString() ?? string.Empty;
    }
}
