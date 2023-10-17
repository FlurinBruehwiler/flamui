using System.Numerics;
using ImSharpUISample.UiElements;
using static SDL2.SDL;
using static ImSharpUISample.Ui;

namespace ImSharpUISample;

public record Node(Vector2 Pos, string Id)
{
    public Vector2 Pos { get; set; } = Pos;
    public bool IsSelected { get; set; }
};

public record Connection(Node NodeA, Node NodeB)
{
    public string Id { get; } = NodeA.Id + NodeB.Id;
    public Vector2 PortAPos { get; set; }
    public  Vector2 PortBPos { get; set; }
};

public class GraphSample
{
    private readonly List<Node> _nodes = new()
    {
        new Node(new Vector2(300, 100), Guid.NewGuid().ToString()),
        new Node(new Vector2(100, 500), Guid.NewGuid().ToString()),
        new Node(new Vector2(300, 300), Guid.NewGuid().ToString()),
    };

    public readonly List<Connection> _connections = new();
    public CameraInfo Camera = new(Vector2.Zero, Vector2.Zero, 1);
    public Node? DragStart;
    public Node? DragEnd;
    public Vector2 DragStartPos;
    public Vector2 DragEndPos;

    public void Build()
    {
        DragEnd = null;

        HandleCameraMovement();

        Start<Camera>().Info(Camera);
            DivStart(out var background).Color(29, 29, 29);
                background.ComputedX = -10000;
                background.ComputedY = -10000;
                background.ComputedHeight = 20000;
                background.ComputedWidth = 20000;
                foreach (var node in _nodes)
                {
                    Get<NodeComponent>(node.Id).Build(node, this);
                }

                if (DragStart is not null)
                {
                    if (Window.IsMouseButtonReleased(MouseButtonKind.Left))
                    {
                        if (DragEnd != null)
                        {
                            _connections.Add(new Connection(DragStart, DragEnd)
                            {
                                PortAPos = DragStartPos,
                                PortBPos = DragEndPos
                            });
                        }
                        DragStart = null;
                    }
                    else
                    {
                        var end = Camera.ScreenToWorld(Window.MousePosition);
                        if (DragEnd is not null)
                            end = DragEndPos;
                        GetElement<ConnectionLine>().From(DragStartPos).To(end);
                    }
                }

                foreach (var connection in _connections)
                {
                    GetElement<ConnectionLine>(connection.Id).From(connection.PortAPos).To(connection.PortBPos);
                }
            DivEnd();
        End<Camera>();
    }

    private void HandleCameraMovement()
    {
        if (Window.IsMouseButtonDown(MouseButtonKind.Middle))
        {
            var delta = Window.MouseDelta;
            delta *= -1.0f / Camera.Zoom;
            Camera.Target += delta;
        }

        var scrollDelta = Window.ScrollDelta;

        if (scrollDelta != 0)
        {
            var mouseWorldPos =  Camera.ScreenToWorld(Window.MousePosition);
            Camera.Offset = Window.MousePosition;
            Camera.Target = mouseWorldPos;

            const float zoomIncrement = 0.125f;

            Camera.Zoom += -scrollDelta * zoomIncrement;
            if (Camera.Zoom < zoomIncrement)
                Camera.Zoom = zoomIncrement;
        }
    }
}

public class NodeComponent
{
    private bool _isDragging;
    private Vector2 _dragOffset;
    private Node _node = null!;
    private GraphSample _graphSample = null!;

    public void Build(Node node, GraphSample graphSample)
    {
        _node = node;
        _graphSample = graphSample;
        DivStart(out var nodeDiv, LastKey).Clip().BorderColor(16, 16, 16).BorderWidth(4).Absolute(disablePositioning: true).Color(48, 48, 48).Radius(10).Width(300).Height(200);

            //Header
            DivStart(out var headerDiv).Color(29, 29, 29).Height(50).Dir(Dir.Horizontal);
                HandleMovement(nodeDiv, headerDiv);

                DivStart().Width(50);
                    SvgImage("expand_more.svg");
                DivEnd();
                DivStart();
                    Text("Group Input").Size(25).VAlign(TextAlign.Center);
                DivEnd();
            DivEnd();

            //Border
            DivStart().Height(4).Color(16, 16, 16);
            DivEnd();

            //Body
            DivStart();
                //Port
                DivStart(out var port).Color(0, 214, 163).Width(20).Height(20).Radius(10).BorderColor(255, 255, 255);
                    if (Window.IsMouseButtonPressed(MouseButtonKind.Left) && port.IsHovered)
                    {
                        _graphSample.DragStart = node;
                        _graphSample.DragStartPos = GetCenter(port);
                    }

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

                    foreach (var connection in _graphSample._connections)
                    {
                        if (connection.NodeA == node)
                        {
                            connection.PortAPos = GetCenter(port);
                        }
                        if (connection.NodeB == node)
                        {
                            connection.PortBPos = GetCenter(port);
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

    private void HandleMovement(IUiContainerBuilder nodeDiv, IUiContainerBuilder headerDiv)
    {
        var mousePos = _graphSample.Camera.ScreenToWorld(Window.MousePosition);

        if (Window.IsMouseButtonPressed(MouseButtonKind.Left) && headerDiv.IsHovered)
        {
            _isDragging = true;
            _dragOffset = new Vector2(nodeDiv.ComputedX - mousePos.X, nodeDiv.ComputedY - mousePos.Y);
            SDL_CaptureMouse(SDL_bool.SDL_TRUE);
        }

        if (_isDragging && Window.IsMouseButtonReleased(MouseButtonKind.Left))
        {
            _isDragging = false;
            SDL_CaptureMouse(SDL_bool.SDL_FALSE);
        }

        if (_isDragging)
        {
            _node.Pos = mousePos + _dragOffset;
        }

        nodeDiv.ComputedX = _node.Pos.X;
        nodeDiv.ComputedY = _node.Pos.Y;
    }
}
