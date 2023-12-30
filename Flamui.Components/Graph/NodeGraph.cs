using System.Numerics;
using Flamui.UiElements;

namespace Flamui.Components.Graph;

public record struct Port(UiElement PortElement, PortDirection PortDirection);

public struct ConnectionTarget
{
    public Port LeftPort { get; set; }
    public Port RightPort { get; set; }
    public string Id { get; set; }

    /// <summary>
    /// the port thats used while the user is making the connection, it shouldn't swap while the user is still dragging
    /// </summary>
    public PortDirection ActivePortDirection { get; set; }

    public Port GetActivePort()
    {
        return ActivePortDirection == PortDirection.Left ? LeftPort : RightPort;
    }
}

public record Node(Vector2 Pos, string Key, string Name)
{
    public Vector2 Pos { get; set; } = Pos;
    public float Width { get; set; }
    public float Height { get; set; }
    public bool IsSelected { get; set; }

    public bool IsDragging { get; set; }

    public Vector2 DragOffset;
    public bool IsClicked { get; set; }
    public List<ConnectionTarget> ConnectionTargets { get; set; } = [];
    public NodeComponent NodeComponent { get; set; }
};

public record Connection(ConnectionTarget A, ConnectionTarget B);

public record struct ConnectionToDraw(
    string NodeIdA,
    string ConnectionFieldIdA,
    string NodeIdB,
    string ConnectionFieldIdB);

public class NodeGraph : OpenCloseComponent
{
    private readonly IntPtr _dragCursor = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL);
    private readonly IntPtr _normalCursor = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
    private Vector2? _creationDialogPos;

    public readonly List<Node> Nodes = [];
    public readonly Dictionary<string, Node> OldNodes = [];

    public CameraInfo Camera = new(Vector2.Zero, Vector2.Zero, 1);
    public ConnectionTarget? DragStart;
    public ConnectionTarget? DragEnd;
    private Vector2? _mouseDragStartPos;
    private List<ConnectionToDraw> _connectionsToDraw = new();

    private UiContainer _background;

    public override void Open()
    {
        Nodes.Clear();

        DragEnd = null;

        DivStart().ZIndex(-1).Clip();
            DivStart(out _background).Color(29, 29, 29);
                Start<Camera>().Info(Camera);
                    HandleCameraMovement();

                    Get<DotGrid>();
    }

    public override void Close()
    {
                    foreach (var node in Nodes)
                    {
                        node.NodeComponent.Update();
                    }

                    foreach (var connectionToDraw in _connectionsToDraw)
                    {
                        DrawConnectionInternal(connectionToDraw);
                    }

                    HandleNodeSelection();

                    HandleDragSelection();

                    HandleConnectionDrag();
                End<Camera>();
            DivEnd();
        DivEnd();

        _connectionsToDraw.Clear();

        OldNodes.Clear();

        foreach (var node in Nodes)
        {
            node.ConnectionTargets.Clear();
            OldNodes.Add(node.Key, node);
        }
    }

    public void StartNode(string key, string name, out NodeComponent n)
    {
        if (!OldNodes.TryGetValue(key, out var node))
        {
            node = new Node(new Vector2(Random.Shared.Next(3000), Random.Shared.Next(2000)), key, name);
        }

        Nodes.Add(node);

        // ReSharper disable once RedundantTypeArgumentsOfMethod
        node.NodeComponent = StartComponent<NodeComponent, NcParam>(out n, new NcParam(node, this), name);
    }

    public void EndNode()
    {
        EndComponent<NodeComponent, NcParam>();
    }

    public void DrawConnection(string nodeIdA, string connectionFieldIdA, string nodeIdB, string connectionFieldIdB)
    {
        _connectionsToDraw.Add(new ConnectionToDraw(nodeIdA, connectionFieldIdA, nodeIdB, connectionFieldIdB));
    }

    private void DrawConnectionInternal(ConnectionToDraw connectionToDraw)
    {
        ConnectionTarget targetA = new();
        ConnectionTarget targetB = new();

        for (var i = 0; i < Nodes.Count; i++)
        {
            var node = Nodes[i];

            if (node.Name == connectionToDraw.NodeIdA || node.Name == connectionToDraw.NodeIdB)
            {
                for (var j = 0; j < node.ConnectionTargets.Count; j++)
                {
                    var target = node.ConnectionTargets[j];

                    if (target.Id == connectionToDraw.ConnectionFieldIdA && node.Name == connectionToDraw.NodeIdA)
                    {
                        targetA = target;
                    }
                    else if (target.Id == connectionToDraw.ConnectionFieldIdB && node.Name == connectionToDraw.NodeIdB)
                    {
                        targetB = target;
                    }
                }
            }
        }

        var connection = new Connection(targetA, targetB);
        //todo remove string allocation
        Get<ConnectionLine>($"{connectionToDraw.NodeIdA}.{connectionToDraw.ConnectionFieldIdA}-{connectionToDraw.NodeIdB}.{connectionToDraw.ConnectionFieldIdB}")
            .Dynamic(connection.A, connection.B);
    }

    private void HandleNodeSelection()
    {
        if (Window.IsMouseButtonPressed(MouseButtonKind.Left))
        {
            var dragStartNode = Nodes.FirstOrDefault(static x => x.IsClicked);
            if (dragStartNode is not null)
            {
                Nodes.Remove(dragStartNode);//bring to front :) //todo make actually work with new system
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
            else if(_background.ContainsPoint(Window.MousePosition))
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
            selectionDiv.ComputedBounds.X = xMin;
            selectionDiv.ComputedBounds.Y = yMin;
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
                    // _connections.Add(new Connection(DragStart.Value, DragEnd.Value, Guid.NewGuid().ToString())); //todo
                }
                else
                {
                    _creationDialogPos = Window.MousePosition;
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

        //ToDo some generic solution
        // if (_creationDialogPos is {} pos)
        // {
        //     var dialog = GetComponent<ObjectCreationDialog>().Build(pos);
        //     if (dialog.IsCancelled)
        //         _creationDialogPos = null;
        // }
    }

    private bool _isCameraDragging;

    private void HandleCameraMovement()
    {
        if (!_background.ContainsPoint(Window.MousePosition))
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

