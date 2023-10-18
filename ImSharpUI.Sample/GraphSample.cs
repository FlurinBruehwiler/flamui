using System.Numerics;
using static SDL2.SDL;
using static ImSharpUISample.Ui;

namespace ImSharpUISample;

public record Node(Vector2 Pos, string Id)
{
    public Vector2 Pos { get; set; } = Pos;
    public float Width { get; set; }
    public float Height { get; set; }
    public bool IsSelected { get; set; }
    public bool IsDragging;
    public Vector2 DragOffset;
    public bool IsClicked { get; set; }
};

public record Connection(Node NodeA, Node NodeB)
{
    public string Id { get; } = NodeA.Id + NodeB.Id;
    public Vector2 PortAPos { get; set; }
    public  Vector2 PortBPos { get; set; }
};

public class GraphSample
{
    public readonly List<Node> Nodes = new()
    {
        new Node(new Vector2(300, 100), Guid.NewGuid().ToString()),
        new Node(new Vector2(100, 500), Guid.NewGuid().ToString()),
        new Node(new Vector2(300, 300), Guid.NewGuid().ToString()),
    };

    public readonly List<Connection> Connections = new();
    public CameraInfo Camera = new(Vector2.Zero, Vector2.Zero, 1);
    public Node? DragStart;
    public Node? DragEnd;
    public Vector2 DragStartPos;
    public Vector2 DragEndPos;
    private Vector2? _mouseDragStartPos;

    public void Build()
    {
        DragEnd = null;

        HandleCameraMovement();

        DivStart().Color(200, 0, 0).HeightFraction(20);
        DivEnd();

        DivStart().Clip();
            Start<Camera>().Info(Camera);
                DivStart(out var background).Color(29, 29, 29);
                    GetElement<DotGrid>();

                    background.ComputedX = -10000;
                    background.ComputedY = -10000;
                    background.ComputedHeight = 20000;
                    background.ComputedWidth = 20000;

                    foreach (var node in Nodes)
                    {
                        Get<NodeComponent>(node.Id).Build(node, this);
                    }

                    if (Window.IsMouseButtonPressed(MouseButtonKind.Left))
                    {
                        var dragStartNode = Nodes.FirstOrDefault(static x => x.IsClicked);
                        if (dragStartNode is not null)
                        {
                            foreach (var node in Nodes)
                            {
                                if (!node.IsSelected)
                                    continue;

                                node.IsClicked = false;
                                node.IsDragging = true;
                                node.DragOffset = node.Pos - Camera.ScreenToWorld(Window.MousePosition);
                            }
                        }
                        else if(background.IsHovered)
                        {
                            if(!Window.IsKeyDown(SDL_Scancode.SDL_SCANCODE_LSHIFT))
                            {
                                foreach (var node in Nodes)
                                {
                                    node.IsSelected = false;
                                }
                            }

                            _mouseDragStartPos = Camera.ScreenToWorld(Window.MousePosition);
                        }
                    }

                    if (_mouseDragStartPos is { } startPos)
                    {
                        var mousePos = Camera.ScreenToWorld(Window.MousePosition);
                        var xMax = Math.Max(mousePos.X, startPos.X);
                        var yMax = Math.Max(mousePos.Y, startPos.Y);
                        var xMin = Math.Min(mousePos.X, startPos.X);
                        var yMin = Math.Min(mousePos.Y, startPos.Y);

                        DivStart(out var selectionDiv).Color(255, 255, 255, 50).Absolute(disablePositioning:true);
                            selectionDiv.ComputedX = xMin;
                            selectionDiv.ComputedY = yMin;
                            selectionDiv.Width(xMax - xMin);
                            selectionDiv.Height(yMax - yMin);
                        DivEnd();

                        if (Window.IsMouseButtonReleased(MouseButtonKind.Left))
                        {
                            _mouseDragStartPos = null;
                            foreach (var node in Nodes)
                            {
                                if (NodeIntersectsSelection(node, new Vector2(xMin, yMin), new Vector2(xMax, yMax)))
                                {
                                    node.IsSelected = true;
                                }
                            }
                        }
                    }

                    SDL_CaptureMouse(SDL_bool.SDL_TRUE);

                    HandleConnectionDrag();

                    foreach (var connection in Connections)
                    {
                        GetElement<ConnectionLine>(connection.Id).From(connection.PortAPos).To(connection.PortBPos);
                    }
                DivEnd();
            End<Camera>();
        DivEnd();
    }

    private void HandleConnectionDrag()
    {
        if (DragStart is not null)
        {
            if (Window.IsMouseButtonReleased(MouseButtonKind.Left))
            {
                //Connection Drag end
                SDL_CaptureMouse(SDL_bool.SDL_FALSE);
                if (DragEnd != null)
                {
                    Connections.Add(new Connection(DragStart, DragEnd)
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

    private bool NodeIntersectsSelection(Node node, Vector2 topLeft, Vector2 bottomRight)
    {
        if (bottomRight.X < node.Pos.X || topLeft.X > node.Pos.X + node.Width ||
            bottomRight.Y < node.Pos.Y || topLeft.Y > node.Pos.Y + node.Height)
        {
            return false;
        }

        // If the above conditions are false, then the rectangles intersect
        return true;
    }
}

