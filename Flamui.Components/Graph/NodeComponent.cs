using System.Numerics;
using Flamui.UiElements;

namespace Flamui.Components.Graph;

public record struct NcParam(Node Node, NodeGraph NodeGraph);

public class NodeComponent : OpenCloseComponent<NcParam>
{
    private bool _dragHasHappened;
    private UiContainer _nodeDiv;
    
    public override void Open()
    {
        DivStart(out _nodeDiv, LastKey).Shadow(5, top: 5).ShadowColor(0, 0, 0).Clip().BlockHit()
            .BorderColor(16, 16, 16).BorderWidth(2).Absolute(disablePositioning: true).Color(48, 48, 48)
            .Rounded(10).Width(300).Height(550); //todo auto hight calculation

            if (Parameteres.Node.IsSelected)
            {
                _nodeDiv.BorderColor(255, 255, 255);
            }

            //Header
            DivStart().Color(29, 29, 29).Height(50).Dir(Dir.Horizontal);

                DivStart().Width(50);
                    SvgImage("./Icons/expand_more.svg");
                DivEnd();
                DivStart();
                    Text(Parameteres.Node.Name).Size(25).VAlign(TextAlign.Center).Color(224, 224, 224);
                DivEnd();
            DivEnd();

            //Border
            DivStart().Height(3).Color(24, 24, 24);
            DivEnd();

            //Body
            DivStart();
    }

    public override void Close()
    {
            //End Body
            DivEnd();

        DivEnd();
    }

    public void Update()
    {
        HandleMovement(Parameteres.Node, _nodeDiv);
    }

    public void AddField(string content, string key)
    {
        DivStart(key).Height(50).PaddingLeft(20);

            Text($"{key} {content}").VAlign(TextAlign.Center).Size(20);

        DivEnd();
    }

    public void AddConnectionField(string content, string key)
    {
        var connectionTarget = new ConnectionTarget
        {
            Id = key
        };

        DivStart(key).Height(50).PaddingLeft(20);

            //Port
            DivStart(out var left).Absolute(left: -10).MAlign(MAlign.Center);
                connectionTarget.LeftPort = new Port(left, PortDirection.Left);

                DivStart(out var portLeft).BlockHit().BorderColor(0, 0, 0).BorderWidth(2).IgnoreClipFrom(_nodeDiv).Color(0, 214, 163).Width(20).Height(20).Rounded(10);
                    if (portLeft.IsHovered)
                        portLeft.Color(0, 255, 195);

                DivEnd();
            DivEnd();

            DivStart(out var right).Absolute(right: -10).MAlign(MAlign.Center);
                connectionTarget.RightPort = new Port(right, PortDirection.Right);

                DivStart(out var portRight).BlockHit().BorderColor(0, 0, 0).BorderWidth(2).IgnoreClipFrom(_nodeDiv).Color(0, 214, 163).Width(20).Height(20).Rounded(10);
                    if (portRight.IsHovered)
                        portRight.Color(0, 255, 195);

                DivEnd();
            DivEnd();

            HandlePort(portLeft, portRight, PortDirection.Left);
            HandlePort(portLeft, portRight, PortDirection.Right);

            Text($"{key} {content}").VAlign(TextAlign.Center).Size(20);

        DivEnd();

        Parameteres.Node.ConnectionTargets.Add(connectionTarget);
    }

    private void HandlePort(UiContainer leftPort, UiContainer rightPort, PortDirection portDirection)
    {
        var activePort = portDirection == PortDirection.Left ? leftPort : rightPort;

        //Port Drag start
        if (Window.IsMouseButtonPressed(MouseButtonKind.Left) && activePort.IsHovered)
        {
            // SDL_CaptureMouse(SDL_bool.SDL_TRUE);
            Parameteres.NodeGraph.DragStart = new ConnectionTarget
            {
                LeftPort = new Port(leftPort, PortDirection.Left),
                RightPort = new Port(rightPort, PortDirection.Right),
                ActivePortDirection = portDirection
            };
        }

        //Snap to Target Port
        if (Parameteres.NodeGraph.DragStart is not null && Parameteres.NodeGraph.DragStart.Value.LeftPort.PortElement != leftPort && Parameteres.NodeGraph.DragStart.Value.RightPort.PortElement != rightPort)
        {
            var mousePos = Parameteres.NodeGraph.Camera.ScreenToWorld(Window.MousePosition);
            var portCenter = GetCenter(activePort);

            if (Vector2.Distance(mousePos, portCenter) < 40)
            {
                Parameteres.NodeGraph.DragEnd = new ConnectionTarget
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
        var mousePos = Parameteres.NodeGraph.Camera.ScreenToWorld(Window.MousePosition);

        if (nodeDiv.IsClicked)
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

        if (Parameteres.Node.IsDragging && Window.IsMouseButtonReleased(MouseButtonKind.Left))
        {
            _dragHasHappened = false;
            Parameteres.Node.IsDragging = false;
            // SDL_CaptureMouse(SDL_bool.SDL_FALSE);
        }

        if (Parameteres.Node.IsDragging)
        {
            if (Window.MouseDelta != Vector2.Zero)
            {
                _dragHasHappened = true;
            }
            Parameteres.Node.Pos = mousePos + Parameteres.Node.DragOffset;
        }

        Parameteres.Node.Width = nodeDiv.ComputedBounds.W;
        Parameteres.Node.Height = nodeDiv.ComputedBounds.H;

        nodeDiv.ComputedBounds.X = Parameteres.Node.Pos.X;
        nodeDiv.ComputedBounds.Y = Parameteres.Node.Pos.Y;
    }

    private void SelectNode(Node node)
    {
        if (Window.IsKeyDown(SDL_Scancode.SDL_SCANCODE_LSHIFT))
        {
            node.IsSelected = true;
        }
        else
        {
            foreach (var otherNode in Parameteres.NodeGraph.Nodes)
            {
                otherNode.IsSelected = false;
            }

            node.IsSelected = true;
        }
    }
}
