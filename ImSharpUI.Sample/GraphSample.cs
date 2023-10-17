using System.Numerics;
using static ImSharpUISample.Ui;

namespace ImSharpUISample;

public class GraphSample
{
    private readonly List<Node> _nodes = new()
    {
        new Node(new List<(string, string)>()),
        new Node(new List<(string, string)>()),
        new Node(new List<(string, string)>()),
        new Node(new List<(string, string)>()),
    };
    private CameraInfo _camera = new(Vector2.Zero, Vector2.Zero, 1);

    public void Build()
    {
        HandleCameraMovement();

        Start<Camera>().Info(_camera);

            Get<NodeComponent>().Build(null);

        End<Camera>();
    }

    private void HandleCameraMovement()
    {
        if (IsMouseButtonDown())
        {
            var delta = GetMouseDelta();
            delta *= -1.0f / _camera.Zoom;
            _camera.Target += delta;
        }

        var scrollDelta = GetScrollDelta();

        if (scrollDelta != 0)
        {
            var mouseWorldPos =  _camera.ScreenToWorld(GetMousePosition());
            _camera.Offset = GetMousePosition();
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
        DivStart(out var div).Absolute(disablePositioning: true).Color(48, 48, 48).Radius(10).Width(300).Height(200);

        DivEnd();
    }
}

public record Node(List<(string, string)> Properties);
