using System.Runtime.CompilerServices;
using static ImSharpUISample.Ui;

namespace ImSharpUISample;

public class GraphSample
{
    private List<Node> nodes = new();

    public void Build()
    {
        foreach (var node in nodes)
        {
            Get<NodeComponent>().Build(node);
        }
    }
}

public class NodeComponent
{
    public void Build(Node node)
    {
        DivStart().Absolute(disablePositioning: true).Color(48, 48, 48).Radius(10).Width(300).Height(200);

        DivEnd();
    }
}

public record Node(List<(string, string)> Properties);

public partial class Ui
{
    public static T Get<T>(string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {

    }

    public static void Node()
    {

    }
}
