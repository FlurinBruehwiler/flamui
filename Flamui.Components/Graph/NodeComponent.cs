using System.Numerics;
using Flamui.UiElements;
using SDL2;

namespace Flamui.Components.Graph;

public class NodeComponent : FlamuiComponent
{
    private bool _dragHasHappened;
    private UiContainer _nodeDiv;

    [Parameter]
    public required Node Node { get; set; }

    [Parameter]
    public required NodeGraph NodeGraph { get; set; }
    
    public override void Build(Ui ui)
    {
        ui.DivStart(out _nodeDiv, ui.LastKey).Shadow(5, top: 5).ShadowColor(0, 0, 0).Clip().BlockHit()
            .BorderColor(16, 16, 16).BorderWidth(2).Absolute(disablePositioning: true).Color(48, 48, 48)
            .Rounded(10).Width(300).Height(550); //todo auto hight calculation

            if (Node.IsSelected)
            {
                _nodeDiv.BorderColor(255, 255, 255);
            }

            //Header
            ui.DivStart().Color(29, 29, 29).Height(50).Dir(Dir.Horizontal);

            ui.DivStart().Width(50);
            ui.SvgImage("./Icons/expand_more.svg");
            ui.DivEnd();
            ui.DivStart();
            ui.Text(Node.Name).Size(25).VAlign(TextAlign.Center).Color(224, 224, 224);
            ui.DivEnd();
            ui.DivEnd();

            //Border
            ui.DivStart().Height(3).Color(24, 24, 24);
            ui.DivEnd();

            //Body
            ui.DivStart();

            //End Body
            ui.DivEnd();

            ui.DivEnd();
    }

    public void Update(Ui ui)
    {
        HandleMovement(ui, Node, _nodeDiv);
    }

    public void AddField(Ui ui, string content, string key)
    {
        ui.DivStart(key).Height(50).PaddingLeft(20);

        ui.Text($"{key} {content}").VAlign(TextAlign.Center).Size(20);

        ui.DivEnd();
    }

    public void AddConnectionField(Ui ui, string content, string key)
    {
        var connectionTarget = new ConnectionTarget
        {
            Id = key
        };

        ui.DivStart(key).Height(50).PaddingLeft(20);

            //Port
            ui.DivStart(out var left).Absolute(left: -10).MAlign(MAlign.Center);
                connectionTarget.LeftPort = new Port(left, PortDirection.Left);

                ui.DivStart(out var portLeft).BlockHit().BorderColor(0, 0, 0).BorderWidth(2).IgnoreClipFrom(_nodeDiv).Color(0, 214, 163).Width(20).Height(20).Rounded(10);
                    if (portLeft.IsHovered)
                        portLeft.Color(0, 255, 195);

                    ui.DivEnd();
                    ui.DivEnd();

                    ui.DivStart(out var right).Absolute(right: -10).MAlign(MAlign.Center);
                connectionTarget.RightPort = new Port(right, PortDirection.Right);

                ui.DivStart(out var portRight).BlockHit().BorderColor(0, 0, 0).BorderWidth(2).IgnoreClipFrom(_nodeDiv).Color(0, 214, 163).Width(20).Height(20).Rounded(10);
                    if (portRight.IsHovered)
                        portRight.Color(0, 255, 195);

                    ui.DivEnd();
                    ui.DivEnd();

            HandlePort(ui, portLeft, portRight, PortDirection.Left);
            HandlePort(ui, portLeft, portRight, PortDirection.Right);

            ui.Text($"{key} {content}").VAlign(TextAlign.Center).Size(20);

            ui.DivEnd();

        Node.ConnectionTargets.Add(connectionTarget);
    }

    private void HandlePort(Ui ui, UiContainer leftPort, UiContainer rightPort, PortDirection portDirection)
    {
        var activePort = portDirection == PortDirection.Left ? leftPort : rightPort;

        //Port Drag start
        if (ui.Window.IsMouseButtonPressed(MouseButtonKind.Left) && activePort.IsHovered)
        {
            // SDL_CaptureMouse(SDL_bool.SDL_TRUE);
            NodeGraph.DragStart = new ConnectionTarget
            {
                LeftPort = new Port(leftPort, PortDirection.Left),
                RightPort = new Port(rightPort, PortDirection.Right),
                ActivePortDirection = portDirection
            };
        }

        //Snap to Target Port
        if (NodeGraph.DragStart is not null && NodeGraph.DragStart.Value.LeftPort.PortElement != leftPort && NodeGraph.DragStart.Value.RightPort.PortElement != rightPort)
        {
            var mousePos = NodeGraph.Camera.ScreenToWorld(ui.Window.MousePosition);
            var portCenter = GetCenter(activePort);

            if (Vector2.Distance(mousePos, portCenter) < 40)
            {
                NodeGraph.DragEnd = new ConnectionTarget
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

    private void HandleMovement(Ui ui, Node node, UiContainer nodeDiv)
    {
        var mousePos = NodeGraph.Camera.ScreenToWorld(ui.Window.MousePosition);

        if (nodeDiv.IsClicked)
        {
            SDL.SDL_CaptureMouse(SDL.SDL_bool.SDL_TRUE);
            if (!node.IsSelected)
            {
                SelectNode(ui, node);
            }
            node.IsClicked = true;
        }

        if (ui.Window.IsMouseButtonReleased(MouseButtonKind.Left) && nodeDiv.IsHovered)
        {
            SDL.SDL_CaptureMouse(SDL.SDL_bool.SDL_FALSE);
            if (node.IsSelected && !_dragHasHappened)
            {
                SelectNode(ui, node);
            }
        }

        if (Node.IsDragging && ui.Window.IsMouseButtonReleased(MouseButtonKind.Left))
        {
            _dragHasHappened = false;
            Node.IsDragging = false;
            // SDL_CaptureMouse(SDL_bool.SDL_FALSE);
        }

        if (Node.IsDragging)
        {
            if (ui.Window.MouseDelta != Vector2.Zero)
            {
                _dragHasHappened = true;
            }
            Node.Pos = mousePos + Node.DragOffset;
        }

        Node.Width = nodeDiv.ComputedBounds.W;
        Node.Height = nodeDiv.ComputedBounds.H;

        nodeDiv.ComputedBounds.X = Node.Pos.X;
        nodeDiv.ComputedBounds.Y = Node.Pos.Y;
    }

    private void SelectNode(Ui ui, Node node)
    {
        if (ui.Window.IsKeyDown(SDL.SDL_Scancode.SDL_SCANCODE_LSHIFT))
        {
            node.IsSelected = true;
        }
        else
        {
            foreach (var otherNode in NodeGraph.Nodes)
            {
                otherNode.IsSelected = false;
            }

            node.IsSelected = true;
        }
    }
}
