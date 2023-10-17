using System.Numerics;
using static ImSharpUISample.Ui;

namespace ImSharpUISample;

public class GraphSample
{
    private readonly List<Node> _nodes = new()
    {
        new Node(new List<(string, string)>(), Guid.NewGuid().ToString()),
        new Node(new List<(string, string)>(), Guid.NewGuid().ToString()),
        new Node(new List<(string, string)>(), Guid.NewGuid().ToString()),
        new Node(new List<(string, string)>(), Guid.NewGuid().ToString()),
    };
    private CameraInfo _camera = new(Vector2.Zero, Vector2.Zero, 1);

    public void Build()
    {
        HandleCameraMovement();

        Start<Camera>().Info(_camera);

            foreach (var node in _nodes)
            {
                Get<NodeComponent>(node.Id).Build(node);
            }

        End<Camera>();
    }

    private void HandleCameraMovement()
    {
        if (Window.IsMouseButtonDown(MouseButtonKind.Middle))
        {
            var delta = Window.MouseDelta;
            delta *= -1.0f / _camera.Zoom;
            _camera.Target += delta;
        }

        var scrollDelta = Window.ScrollDelta;

        if (scrollDelta != 0)
        {
            var mouseWorldPos =  _camera.ScreenToWorld(Window.MousePosition);
            _camera.Offset = Window.MousePosition;
            _camera.Target = mouseWorldPos;

            const float zoomIncrement = 0.125f;

            _camera.Zoom += -scrollDelta * zoomIncrement;
            if (_camera.Zoom < zoomIncrement)
                _camera.Zoom = zoomIncrement;
        }
    }
}

public class NodeComponent
{
    public void Build(Node node)
    {
        DivStart(out var div, LastKey).Absolute(disablePositioning: true).Color(48, 48, 48).Radius(10).Width(300).Height(200);

        DivEnd();
    }
}

public record Node(List<(string, string)> Properties, string Id);
