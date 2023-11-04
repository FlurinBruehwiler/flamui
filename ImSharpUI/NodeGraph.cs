using System.Numerics;
using ImSharpUISample.UiElements;

namespace ImSharpUISample;

public record struct Port(UiElement PortElement, PortDirection PortDirection);

public struct ConnectionTarget
{
    public Port LeftPort { get; set; }
    public Port RightPort { get; set; }

    /// <summary>
    /// the port thats used while the user is making the connection, it shouldn't swap while the user is still dragging
    /// </summary>
    public PortDirection ActivePortDirection { get; set; }

    public Port GetActivePort()
    {
        return ActivePortDirection == PortDirection.Left ? LeftPort : RightPort;
    }
}

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

public record Connection(ConnectionTarget A, ConnectionTarget B, string Id);

public class NodeGraph
{
    private readonly IntPtr _dragCursor = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL);
    private readonly IntPtr _normalCursor = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);

    public readonly List<Node> Nodes = new()
    {

    };

    public NodeGraph()
    {
        foreach (var _ in Enumerable.Range(0, 50))
        {
            Nodes.Add(new Node(new Vector2(Random.Shared.Next(3000), Random.Shared.Next(3000)), Guid.NewGuid().ToString()));
        }
    }

    public readonly List<Connection> Connections = new();
    public CameraInfo Camera = new(Vector2.Zero, Vector2.Zero, 1);
    public ConnectionTarget? DragStart;
    public ConnectionTarget? DragEnd;
    private Vector2? _mouseDragStartPos;

    public void Build()
    {
        DragEnd = null;

        DivStart().ZIndex(-1).Clip();
            DivStart(out var background).Color(29, 29, 29);
                Start<Camera>().Info(Camera);
                    HandleCameraMovement(background);

                    Get<DotGrid>();

                    foreach (var connection in Connections)
                    {
                        Get<ConnectionLine>(connection.Id).Dynamic(connection.A, connection.B);
                    }

                    foreach (var node in Nodes)
                    {
                        GetComponent<NodeComponent>(node.Id).Build(node, this);
                    }

                    HandleNodeSelection(background);

                    HandleDragSelection();

                    HandleConnectionDrag();
                End<Camera>();
            DivEnd();
        DivEnd();
    }

    private void HandleNodeSelection(UiContainer background)
    {
        if (Window.IsMouseButtonPressed(MouseButtonKind.Left))
        {
            var dragStartNode = Nodes.FirstOrDefault(static x => x.IsClicked);
            if (dragStartNode is not null)
            {
                Nodes.Remove(dragStartNode);
                Nodes.Add(dragStartNode);
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

                SDL_CaptureMouse(SDL_bool.SDL_TRUE);
                _mouseDragStartPos = Camera.ScreenToWorld(Window.MousePosition);
            }
        }
    }

    private void HandleDragSelection()
    {
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
                SDL_CaptureMouse(SDL_bool.SDL_FALSE);
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
    }

    private Vector2? creationDialogPos;

    private void HandleConnectionDrag()
    {
        if (DragStart is not null)
        {
            if (Window.IsMouseButtonReleased(MouseButtonKind.Left))
            {
                //Connection Drag end
                // SDL_CaptureMouse(SDL_bool.SDL_FALSE);
                if (DragEnd != null)
                {
                    Connections.Add(new Connection(DragStart.Value, DragEnd.Value, Guid.NewGuid().ToString()));
                }
                else
                {
                    creationDialogPos = Window.MousePosition;
                }
                DragStart = null;
            }
            else
            {
                if (DragEnd is not null)
                {
                    Get<ConnectionLine>().Static(DragStart.Value, DragEnd.Value);
                }
                else
                {
                    var end = Camera.ScreenToWorld(Window.MousePosition);
                    Get<ConnectionLine>().Static(DragStart.Value, end);
                }
            }
        }

        if (creationDialogPos is {} pos)
        {
            var dialog = GetComponent<ObjectCreationDialog>().Build(pos);
            if (dialog.IsCancelled)
                creationDialogPos = null;
        }
    }

    private bool _isCameraDragging;

    private void HandleCameraMovement(UiContainer background)
    {
        if (!background.ContainsPoint(Window.MousePosition))
            return;

        if (Window.IsMouseButtonDown(MouseButtonKind.Middle) || Window.IsMouseButtonDown(MouseButtonKind.Left) &&
            (Window.IsKeyDown(SDL_Scancode.SDL_SCANCODE_LCTRL) || Window.IsKeyDown(SDL_Scancode.SDL_SCANCODE_SPACE)))
        {
            if (!_isCameraDragging) //start drag
            {
                SDL_SetCursor(_dragCursor);
            }

            _isCameraDragging = true;
            var delta = Window.MouseDelta;
            delta *= -1.0f / Camera.Zoom;
            Camera.Target += delta;
        }
        else if (_isCameraDragging) //stop drag
        {
            _isCameraDragging = false;
            SDL_SetCursor(_normalCursor);
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

