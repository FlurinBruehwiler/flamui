using System.Numerics;
using Flamui;
using Flamui.UiElements;

namespace Sample;

public class NodeComponent
{
    private Node _node = null!;
    private NodeGraph _nodeGraph = null!;
    private bool _dragHasHappened;

    public void Build(Node node, NodeGraph nodeGraph)
    {
        _node = node;
        _nodeGraph = nodeGraph;
        DivStart(out var nodeDiv, LastKey).Shadow(5, top: 5).ShadowColor(0, 0, 0).Clip().BlockHit()
            .BorderColor(16, 16, 16).BorderWidth(2).Absolute(disablePositioning: true).Color(48, 48, 48)
            .Rounded(10).Width(300).Height(150);

            if (_node.IsSelected)
            {
                nodeDiv.BorderColor(255, 255, 255);
            }

            //Header
            DivStart().Color(29, 29, 29).Height(50).Dir(Dir.Horizontal);
                HandleMovement(_node, nodeDiv);

                DivStart().Width(50);
                    SvgImage("./Icons/expand_more.svg");
                DivEnd();
                DivStart();
                    Text("Group Input").Size(25).VAlign(TextAlign.Center).Color(224, 224, 224);
                DivEnd();
            DivEnd();

            //Border
            DivStart().Height(3).Color(24, 24, 24);
            DivEnd();

            //Body
            DivStart();

                //Item
                DivStart().Height(50).PaddingLeft(20);

                    //Port
                    DivStart().Absolute(left: -10).MAlign(MAlign.Center);
                        DivStart(out var portLeft, nodeDiv.Id.Key).BlockHit().BorderColor(0, 0, 0).BorderWidth(2).IgnoreClipFrom(nodeDiv).Color(0, 214, 163).Width(20).Height(20).Rounded(10);
                            if (portLeft.IsHovered)
                                portLeft.Color(0, 255, 195);

                        DivEnd();
                    DivEnd();

                    DivStart().Absolute(right: -10).MAlign(MAlign.Center);
                        DivStart(out var portRight, nodeDiv.Id.Key).BlockHit().BorderColor(0, 0, 0).BorderWidth(2).IgnoreClipFrom(nodeDiv).Color(0, 214, 163).Width(20).Height(20).Rounded(10);
                            if (portRight.IsHovered)
                                portRight.Color(0, 255, 195);

                        DivEnd();
                    DivEnd();

                    HandlePort(portLeft, portRight, PortDirection.Left);
                    HandlePort(portLeft, portRight, PortDirection.Right);

                    Text("Geometry").VAlign(TextAlign.Center).Size(20);

                DivEnd();

            DivEnd();

        DivEnd();
    }

    private void HandlePort(UiContainer leftPort, UiContainer rightPort, PortDirection portDirection)
    {
        var activePort = portDirection == PortDirection.Left ? leftPort : rightPort;

        //Port Drag start
        if (Window.IsMouseButtonPressed(MouseButtonKind.Left) && activePort.IsHovered)
        {
            // SDL_CaptureMouse(SDL_bool.SDL_TRUE);
            _nodeGraph.DragStart = new ConnectionTarget
            {
                LeftPort = new Port(leftPort, PortDirection.Left),
                RightPort = new Port(rightPort, PortDirection.Right),
                ActivePortDirection = portDirection
            };
        }

        //Snap to Target Port
        if (_nodeGraph.DragStart is not null && _nodeGraph.DragStart.Value.LeftPort.PortElement != leftPort && _nodeGraph.DragStart.Value.RightPort.PortElement != rightPort)
        {
            var mousePos = _nodeGraph.Camera.ScreenToWorld(Window.MousePosition);
            var portCenter = GetCenter(activePort);

            if (Vector2.Distance(mousePos, portCenter) < 40)
            {
                _nodeGraph.DragEnd = new ConnectionTarget
                {
                    LeftPort = new Port(leftPort, PortDirection.Left),
                    RightPort = new Port(rightPort, PortDirection.Right),
                    ActivePortDirection = portDirection
                };;
            }
        }
    }

    private Vector2 GetCenter(UiContainer port)
    {
        return new Vector2(port.ComputedBounds.X + port.ComputedBounds.W / 2,
            port.ComputedBounds.Y + port.ComputedBounds.H / 2);
    }

    private void HandleMovement(Node node, UiContainer nodeDiv)
    {
        var mousePos = _nodeGraph.Camera.ScreenToWorld(Window.MousePosition);

        if (nodeDiv.Clicked)
        {
            SDL_CaptureMouse(SDL_bool.SDL_TRUE);
            if (!node.IsSelected)
            {
                SelectNode(node);
            }
            node.IsClicked = true;
        }

        if (Window.IsMouseButtonReleased(MouseButtonKind.Left) && nodeDiv.IsHovered)
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

        _node.Width = nodeDiv.ComputedBounds.W;
        _node.Height = nodeDiv.ComputedBounds.H;

        nodeDiv.ComputedBounds.X = _node.Pos.X;
        nodeDiv.ComputedBounds.Y = _node.Pos.Y;
    }

    private void SelectNode(Node node)
    {
        if (Window.IsKeyDown(SDL_Scancode.SDL_SCANCODE_LSHIFT))
        {
            node.IsSelected = true;
        }
        else
        {
            foreach (var otherNode in _nodeGraph.Nodes)
            {
                otherNode.IsSelected = false;
            }

            node.IsSelected = true;
        }
    }
}
