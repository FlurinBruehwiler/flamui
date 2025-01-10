using System.Text;
using Flamui.Layouting;
using Flamui.UiElements;
using Xunit.Abstractions;

namespace Flamui.Test;

public class LayoutingTests(ITestOutputHelper console)
{
    [Fact]
    public void BasicFill()
    {
        var ui = GetUi();

        using (ui.Div())
        {

        }

        var expected =
                """
                FlexContainer = X:0, Y:0, W:100, H:100
                """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void VerticalPercentage()
    {
        var ui = GetUi();

        using (ui.Div())
        {
            using (ui.Div())
            {

            }

            using (ui.Div())
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:0, Y:0, W:100, H:50
                FlexContainer = X:0, Y:50, W:100, H:50
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void HorizontalPercentage()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal))
        {
            using (ui.Div())
            {

            }

            using (ui.Div())
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:0, Y:0, W:50, H:100
                FlexContainer = X:50, Y:0, W:50, H:100
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void HorizontalFixed()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal))
        {
            using (ui.Div().Width(40))
            {

            }

            using (ui.Div().Width(30))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:0, Y:0, W:40, H:100
                FlexContainer = X:40, Y:0, W:30, H:100
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void HorizontalFixedGap()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal).Gap(10))
        {
            using (ui.Div().Width(40))
            {

            }

            using (ui.Div().Width(30))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:0, Y:0, W:40, H:100
                FlexContainer = X:50, Y:0, W:30, H:100
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Padding()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal).Padding(10))
        {
            using (ui.Div())
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:10, Y:10, W:80, H:80
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void MixedChildrenHorizontal()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal))
        {
            using (ui.Div().WidthFraction(50))
            {

            }

            using (ui.Div().Width(30))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:0, Y:0, W:35, H:100
                FlexContainer = X:35, Y:0, W:30, H:100
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void MainAlign_SpaceBetween()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal).MainAlign(MAlign.SpaceBetween))
        {
            using (ui.Div().Width(10))
            {

            }

            using (ui.Div().Width(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:0, Y:0, W:10, H:100
                FlexContainer = X:90, Y:0, W:10, H:100
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void MainAlign_Center()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal).MainAlign(MAlign.Center))
        {
            using (ui.Div().Width(10))
            {

            }

            using (ui.Div().Width(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:40, Y:0, W:10, H:100
                FlexContainer = X:50, Y:0, W:10, H:100
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void MainAlign_End()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal).MainAlign(MAlign.End))
        {
            using (ui.Div().Width(10))
            {

            }

            using (ui.Div().Width(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:80, Y:0, W:10, H:100
                FlexContainer = X:90, Y:0, W:10, H:100
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Margin_Fixed()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal))
        {
            using (ui.Div().Margin(10).Width(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:10, Y:10, W:10, H:80
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Margin_Padding_Fixed()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal).Padding(10))
        {
            using (ui.Div().Margin(10).Width(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:20, Y:20, W:10, H:60
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Margin_Flexible()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal))
        {
            using (ui.Div().Margin(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:10, Y:10, W:80, H:80
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void CrossAlign_Center()
    {
        var ui = GetUi();

        using (ui.Div().CrossAlign(XAlign.Center))
        {
            using (ui.Div().Width(10).Height(10))
            {

            }

            using (ui.Div().Width(10).Height(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:45, Y:0, W:10, H:10
                FlexContainer = X:45, Y:10, W:10, H:10
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void CrossAlign_End()
    {
        var ui = GetUi();

        using (ui.Div().CrossAlign(XAlign.End))
        {
            using (ui.Div().Width(10).Height(10))
            {

            }

            using (ui.Div().Width(10).Height(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:90, Y:0, W:10, H:10
                FlexContainer = X:90, Y:10, W:10, H:10
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Horizontal_Center()
    {
        var ui = GetUi();

        using (ui.Div().Center().Direction(Dir.Horizontal))
        {
            using (ui.Div().Width(10).Height(10))
            {

            }

            using (ui.Div().Width(10).Height(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:40, Y:45, W:10, H:10
                FlexContainer = X:50, Y:45, W:10, H:10
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Vertical_Center()
    {
        var ui = GetUi();

        using (ui.Div().Center().Direction(Dir.Vertical))
        {
            using (ui.Div().Width(10).Height(10))
            {

            }

            using (ui.Div().Width(10).Height(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:45, Y:40, W:10, H:10
                FlexContainer = X:45, Y:50, W:10, H:10
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Horizontal_Center_Gap()
    {
        var ui = GetUi();

        using (ui.Div().Center().Direction(Dir.Horizontal).Gap(10))
        {
            using (ui.Div().Width(10).Height(10))
            {

            }

            using (ui.Div().Width(10).Height(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:35, Y:45, W:10, H:10
                FlexContainer = X:55, Y:45, W:10, H:10
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Padding_Fraction()
    {
        var ui = GetUi();

        using (ui.Div().Padding(10))
        {
            using (ui.Div().HeightFraction(50))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:10, Y:10, W:80, H:40
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Padding_Left()
    {
        var ui = GetUi();

        using (ui.Div().PaddingLeft(10))
        {
            using (ui.Div())
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:10, Y:0, W:90, H:100
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Padding_Right()
    {
        var ui = GetUi();

        using (ui.Div().PaddingRight(10))
        {
            using (ui.Div())
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:0, Y:0, W:90, H:100
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Padding_Top()
    {
        var ui = GetUi();

        using (ui.Div().PaddingTop(10))
        {
            using (ui.Div())
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:0, Y:10, W:100, H:90
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Padding_Bottom()
    {
        var ui = GetUi();

        using (ui.Div().PaddingBottom(10))
        {
            using (ui.Div())
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:0, Y:0, W:100, H:90
            """;

        AssertUi(ui, 100, 100, expected);
    }

    private void AssertUi(Ui ui, int width, int height, string expected)
    {
        var root = (UiElementContainer)ui.OpenElementStack.Pop();

        root.PrepareLayout(Dir.Vertical);
        root.Layout(new BoxConstraint(0, width, 0, height));

        var sb = new StringBuilder();

        foreach (var rootChild in root.Children)
        {
            BuildTextualRepresentation(rootChild, sb, 0);
        }

        var actual = sb.ToString().Trim();

        if (expected != actual)
        {
            console.WriteLine($"Expected:\n{expected}");
            console.WriteLine($"\nActual:\n{actual}");
            Assert.Equal(expected, actual);
        }
    }

    private void BuildTextualRepresentation(UiElement element, StringBuilder sb, int indentation)
    {
        sb.Append(new string(' ', indentation * 4));
        sb.AppendLine($"{element.GetType().Name} = X:{element.ParentData.Position.X}, Y:{element.ParentData.Position.Y}, W:{element.Rect.Width}, H:{element.Rect.Height}");

        if (element is UiElementContainer container)
        {
            indentation++;
            foreach (var child in container.Children)
            {
                BuildTextualRepresentation(child, sb, indentation);
            }
        }
    }

    private Ui GetUi()
    {
        var ui = new Ui();
        var window = new UiWindow(ui);

        var rootContainer = new FlexContainer
        {
            Id = new UiID("RootElement", "", 0, 0),
            Window = window
        };
        ui.Window = window;

        ui.OpenElementStack.Push(rootContainer);
        rootContainer.OpenElement();

        return ui;
    }
}
