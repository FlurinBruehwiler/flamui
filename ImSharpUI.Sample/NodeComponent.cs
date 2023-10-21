using System.Numerics;
using ImSharpUISample.UiElements;
using static SDL2.SDL;
using static ImSharpUISample.Ui;

namespace ImSharpUISample;

public class NodeComponent
{
    private Node _node = null!;
    private GraphSample _graphSample = null!;
    private bool _dragHasHappened;

    public void Build(Node node, GraphSample graphSample)
    {
        _node = node;
        _graphSample = graphSample;
        DivStart(out var nodeDiv, LastKey).Shadow(5, top: 5).ShadowColor(0, 0, 0).Clip().BorderColor(16, 16, 16).BorderWidth(2).Absolute(disablePositioning: true).Color(48, 48, 48).Radius(10).Width(300).Height(200);

            if (_node.IsSelected)
            {
                nodeDiv.BorderColor(255, 255, 255);
            }

            //Header
            DivStart(out var headerDiv).Color(29, 29, 29).Height(50).Dir(Dir.Horizontal);
                HandleMovement(_node, nodeDiv, headerDiv);

                DivStart().Width(50);
                    SvgImage("expand_more.svg");
                DivEnd();
                DivStart();
                    Text("Group Input").Size(25).VAlign(TextAlign.Center).Color(224, 224, 224);
                DivEnd();
            DivEnd();

            //Border
            DivStart().Height(2).Color(24, 24, 24);
            DivEnd();

            //Body
            DivStart();
                //Port
                DivStart(out var port).Color(0, 214, 163).Width(20).Height(20).Radius(10).BorderColor(255, 255, 255);

                    //Port Drag start
                    if (Window.IsMouseButtonPressed(MouseButtonKind.Left) && port.IsHovered)
                    {
                        // SDL_CaptureMouse(SDL_bool.SDL_TRUE);
                        _graphSample.DragStart = node;
                        _graphSample.DragStartPos = GetCenter(port);
                    }

                    //Snap to Target Port
                    if (_graphSample.DragStart is not null && _graphSample.DragStart != _node)
                    {
                        var mousePos = _graphSample.Camera.ScreenToWorld(Window.MousePosition);
                        var portCenter = GetCenter(port);

                        if (Vector2.Distance(mousePos, portCenter) < 40)
                        {
                            _graphSample.DragEnd = node;
                            _graphSample.DragEndPos = GetCenter(port);
                        }
                    }

                    //update positions (todo rework)
                    foreach (var connection in _graphSample.Connections)
                    {
                        if (connection.NodeA == node)
                        {
                            connection.PortA = (UiElement)port;
                        }
                        if (connection.NodeB == node)
                        {
                            connection.PortB = (UiElement)port;
                        }
                    }
                DivEnd();
            DivEnd();

        DivEnd();
    }

    private Vector2 GetCenter(IUiContainerBuilder port)
    {
        return new Vector2(port.ComputedX + port.ComputedWidth / 2,
            port.ComputedY + port.ComputedHeight / 2);
    }

    private void HandleMovement(Node node, IUiContainerBuilder nodeDiv, IUiContainerBuilder headerDiv)
    {
        var mousePos = _graphSample.Camera.ScreenToWorld(Window.MousePosition);

        if (Window.IsMouseButtonPressed(MouseButtonKind.Left) && headerDiv.IsHovered)
        {
            SDL_CaptureMouse(SDL_bool.SDL_TRUE);
            if (!node.IsSelected)
            {
                SelectNode(node);
            }
            node.IsClicked = true;
        }

        if (Window.IsMouseButtonReleased(MouseButtonKind.Left) && headerDiv.IsHovered)
        {
            SDL_CaptureMouse(SDL_bool.SDL_FALSE);
            if (node.IsSelected && !_dragHasHappened)
            {
                SelectNode(node);
            }
        }

        if (_node.IsDragging && Window.IsMouseButtonReleased(MouseButtonKind.Left))
        {
            _dragHasHappened = false;
            _node.IsDragging = false;
            // SDL_CaptureMouse(SDL_bool.SDL_FALSE);
        }

        if (_node.IsDragging)
        {
            if (Window.MouseDelta != Vector2.Zero)
            {
                _dragHasHappened = true;
            }
            _node.Pos = mousePos + _node.DragOffset;
        }

        _node.Width = nodeDiv.ComputedWidth;
        _node.Height = nodeDiv.ComputedHeight;

        nodeDiv.ComputedX = _node.Pos.X;
        nodeDiv.ComputedY = _node.Pos.Y;
    }

    private void SelectNode(Node node)
    {
        if (Window.IsKeyDown(SDL_Scancode.SDL_SCANCODE_LSHIFT))
        {
            node.IsSelected = true;
        }
        else
        {
            foreach (var otherNode in _graphSample.Nodes)
            {
                otherNode.IsSelected = false;
            }

            node.IsSelected = true;
        }
    }
}
