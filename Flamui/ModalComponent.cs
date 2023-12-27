using System.Numerics;
using Flamui.UiElements;

namespace Flamui;

public interface IData
{
    public UiElementId Id { get; set; }
}

[AttributeUsage(AttributeTargets.Method)]
public class BuilderAttribute : Attribute
{

}

public class ModalComponent
{
    private bool _wasShown;
    private bool _isDragging;
    private Vector2 _dragOffset;

    [Builder]
    public void StartModal()
    {

    }

    [Builder]
    public void EndModal(ref bool show, string title, List<UiElement> children)
    {
        if(!show)
            return;

        DivStart().Absolute(Root).XAlign(XAlign.Center).MAlign(MAlign.Center).ZIndex(1).Hidden(!show);

            DivStart(out var modalDiv).Clip().Color(39, 41, 44).Width(400).Height(200).Rounded(10).BorderWidth(2).BorderColor(58, 62, 67);

                if (_wasShown && Window.IsMouseButtonPressed(MouseButtonKind.Left))
                {
                    if(!modalDiv.ContainsPoint(Window.MousePosition))
                        show = false;
                }

                _wasShown = show;

                //Header
                DivStart(out var headerDiv).Height(25).Dir(Dir.Horizontal).MAlign(MAlign.SpaceBetween).PaddingLeft(10);

                    var mousePos = Window.MousePosition;

                    if (show && Window.IsMouseButtonPressed(MouseButtonKind.Left) && headerDiv.ContainsPoint(mousePos))
                    {
                        _isDragging = true;
                        _dragOffset = new Vector2(modalDiv.ComputedBounds.X - mousePos.X, modalDiv.ComputedBounds.Y - mousePos.Y);
                        SDL_CaptureMouse(SDL_bool.SDL_TRUE);
                    }

                    if (_isDragging && Window.IsMouseButtonReleased(MouseButtonKind.Left))
                    {
                        _isDragging = false;
                        SDL_CaptureMouse(SDL_bool.SDL_FALSE);
                    }

                    if (_isDragging)
                    {
                        modalDiv.Absolute(disablePositioning: true);
                        modalDiv.ComputedBounds.X = mousePos.X + _dragOffset.X;
                        modalDiv.ComputedBounds.Y = mousePos.Y + _dragOffset.Y;
                    }

                    //Title
                    DivStart();
                        Text(title).VAlign(TextAlign.Center);
                    DivEnd();

                    //Close button
                    DivStart(out var closeButton).Width(25);
                        if (closeButton.Clicked)
                            show = false;
                        SvgImage("close.svg");
                    DivEnd();
                DivEnd();

                //Main Content
                DivStart().Padding(15);

                    //start user content
                    DivStart(out var userContentWrapper);

                    ((UiContainer)userContentWrapper).Children = children.ToList();

                    //End user content
                    DivEnd();

                    //Footer
                    DivStart().Height(25).Dir(Dir.Horizontal).MAlign(MAlign.FlexEnd).Gap(10);
                        DivStart().Width(70).Color(53, 116, 240).Rounded(3);
                            Text("Next").VAlign(TextAlign.Center).HAlign(TextAlign.Center).Color(230, 230, 230);
                        DivEnd();
                        DivStart(out var cancelButton).Width(70).Color(43, 45, 48).Rounded(3).BorderWidth(1).BorderColor(100, 100, 100);
                            if (cancelButton.Clicked)
                                show = false;

                            Text("Cancel").VAlign(TextAlign.Center).HAlign(TextAlign.Center).Color(230, 230, 230);;
                        DivEnd();
                    DivEnd();

                DivEnd();
            DivEnd();
        DivEnd();
    }
}

public class DropDownComponent
{
    [Builder]
    public static void StartDropDown()
    {

    }

    [Builder]
    public static void DropDownEntry()
    {

    }

    [Builder]
    public static void EndDropDown()
    {

    }
}
