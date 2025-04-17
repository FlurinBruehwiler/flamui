using System.Text;
using Flamui.Layouting;
using Flamui.UiElements;
using Xunit.Abstractions;

namespace Flamui.Test;

public class LayoutingTests : IDisposable
{
    private readonly ITestOutputHelper _console;

    private const string loremIpsum =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

    public LayoutingTests(ITestOutputHelper console)
    {
        _console = console;

        Ui.Arena = new Arena("TestArena", 1_000);
    }

    public void Dispose()
    {
        Ui.Arena.VirtualBuffer.Dispose();
    }

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
    public void Margin_Left()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal))
        {
            using (ui.Div().MarginLeft(10).Width(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:10, Y:0, W:10, H:100
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Margin_Right()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal))
        {
            using (ui.Div().MarginRight(10).Width(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:0, Y:0, W:10, H:100
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Margin_Top()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal))
        {
            using (ui.Div().MarginTop(10).Width(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:0, Y:10, W:10, H:90
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Cross_Fraction()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal))
        {
            using (ui.Div().Width(10).HeightFraction(50))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:150
                FlexContainer = X:0, Y:0, W:10, H:75
            """;

        AssertUi(ui, 100, 150, expected);
    }

    [Fact]
    public void Margin_Bottom()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal))
        {
            using (ui.Div().MarginTop(10).Width(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:0, Y:10, W:10, H:90
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

    [Fact]
    public void Shrink_Basic()
    {
        var ui = GetUi();

        using (ui.Div().Shrink())
        {
            using (ui.Div().Width(10).Height(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:10, H:10
                FlexContainer = X:0, Y:0, W:10, H:10
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Shrink_Padding()
    {
        var ui = GetUi();

        using (ui.Div().Shrink().Padding(10))
        {
            using (ui.Div().Width(10).Height(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:30, H:30
                FlexContainer = X:10, Y:10, W:10, H:10
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Shrink_Margin()
    {
        var ui = GetUi();

        using (ui.Div().Shrink())
        {
            using (ui.Div().Width(10).Height(10).Margin(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:30, H:30
                FlexContainer = X:10, Y:10, W:10, H:10
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Shrink_Gap()
    {
        var ui = GetUi();

        using (ui.Div().ShrinkWidth().Gap(10).Direction(Dir.Horizontal))
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
            FlexContainer = X:0, Y:0, W:30, H:100
                FlexContainer = X:0, Y:0, W:10, H:100
                FlexContainer = X:20, Y:0, W:10, H:100
            """;

        AssertUi(ui, 100, 100, expected);
    }

    // [Fact]
    public void Shrink_CrossAxis()
    {
        var ui = GetUi();

        using (ui.Div().ShrinkHeight().Gap(10).Direction(Dir.Horizontal))
        {
            using (ui.Div())
            {

            }
        }

        var exception = Assert.Throws<InvalidLayoutException>(() => AssertUi(ui, 100, 100, ""));

        Assert.Equal(InvalidLayoutType.FractionWithinShrink, exception.InvalidLayoutType);
    }

    [Fact]
    public void Shrink_MinSize()
    {
        var ui = GetUi();

        using (ui.Div().Shrink(minWidth: 50, minHeight: 50))
        {
            using (ui.Div().Width(10).Height(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:50, H:50
                FlexContainer = X:0, Y:0, W:10, H:10
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Shrink_Multiple()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal))
        {
            using (ui.Div().ShrinkWidth())
            {
                using (ui.Div().Width(10))
                {

                }
            }

            using (ui.Div())
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:0, Y:0, W:10, H:100
                    FlexContainer = X:0, Y:0, W:10, H:100
                FlexContainer = X:10, Y:0, W:90, H:100
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Double_Shrink()
    {
        var ui = GetUi();

        using (ui.Div().Shrink())
        {
            using (ui.Div().Shrink())
            {
                using (ui.Div().Width(10).Height(10))
                {

                }
            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:10, H:10
                FlexContainer = X:0, Y:0, W:10, H:10
                    FlexContainer = X:0, Y:0, W:10, H:10
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Shrink_Fraction_Fixed()
    {
        var ui = GetUi();

        using (ui.Div().Shrink())
        {
            using (ui.Div())
            {
                using (ui.Div().Width(10).Height(10))
                {

                }
            }
        }

        var exception = Assert.Throws<InvalidLayoutException>(() => AssertUi(ui, 100, 100, ""));

        Assert.Equal(InvalidLayoutType.FractionWithinShrink, exception.InvalidLayoutType);
    }

    [Fact]
    public void Shrink_And_Fraction()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal))
        {
            using (ui.Div().ShrinkWidth())
            {
                using (ui.Div().Width(10))
                {

                }
            }

            using (ui.Div().WidthFraction(50))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:0, Y:0, W:10, H:100
                    FlexContainer = X:0, Y:0, W:10, H:100
                FlexContainer = X:10, Y:0, W:45, H:100
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Absolute_Basic()
    {
        var ui = GetUi();

        using (ui.Div())
        {
            using (ui.Div())
            {

            }

            using (ui.Div().Absolute().Width(88).Height(78))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:0, Y:0, W:88, H:78
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void NestedCenter()
    {
        var ui = GetUi();

        using (ui.Div().Center())
        {
            using (ui.Div().Width(50).Height(50).Center())
            {
                using (ui.Div().Width(10).Height(10))
                {

                }
            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:25, Y:25, W:50, H:50
                    FlexContainer = X:45, Y:45, W:10, H:10
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Absolute_Position_Left_Top()
    {
        var ui = GetUi();

        using (ui.Div().Center())
        {
            using (ui.Div().Width(10).Height(10))
            {
                using (ui.Div().AbsolutePosition(top: -10, left: -10))
                {

                }
            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:45, Y:45, W:10, H:10
                    FlexContainer = X:35, Y:35, W:0, H:0
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Absolute_Position_Bottom_Right()
    {
        var ui = GetUi();

        using (ui.Div().Center())
        {
            using (ui.Div().Width(10).Height(10))
            {
                using (ui.Div().AbsolutePosition(bottom: 10, right: 10))
                {

                }
            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:45, Y:45, W:10, H:10
                    FlexContainer = X:65, Y:65, W:0, H:0
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Absolute_Size()
    {
        var ui = GetUi();

        using (ui.Div().Center())
        {
            using (ui.Div().Width(10).Height(10))
            {
                using (ui.Div().AbsoluteSize(widthOffsetParent: 0, heightOffsetParent: 0))
                {

                }
            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:45, Y:45, W:10, H:10
                    FlexContainer = X:45, Y:45, W:10, H:10
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void Text_Multiline()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal))
        {
            ui.Text(loremIpsum + "\n" + loremIpsum).Multiline().Color(188, 190, 196);
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:400, H:100
                UiText = X:0, Y:0, W:241, H:32
                    Line = Lorem ipsum dolor sit amet, consectetur adipiscing elit.
                    Line = Lorem ipsum dolor sit amet, consectetur adipiscing elit.
            """;

        AssertUi(ui, 400, 100, expected);
    }

    [Fact]
    public void Text_Wrap()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Horizontal))
        {
            ui.Text(loremIpsum).Multiline().Color(188, 190, 196);
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:200, H:100
                UiText = X:0, Y:0, W:126, H:32
                    Line = Lorem ipsum dolor sit amet, 
                    Line = consectetur adipiscing elit.
            """;

        AssertUi(ui, 200, 100, expected);
    }

    [Fact]
    public void Multiple_Text_Sizes()
    {
        var ui = GetUi();

        using (ui.Div().Direction(Dir.Vertical))
        {
            ui.Text(loremIpsum).Multiline().Size(20);
            ui.Text(loremIpsum).Multiline().Size(40);
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:400, H:400
                UiText = X:0, Y:0, W:336, H:21
                    Line = Lorem ipsum dolor sit amet, consectetur adipiscing elit.
                UiText = X:0, Y:21, W:367, H:82
                    Line = Lorem ipsum dolor sit amet, 
                    Line = consectetur adipiscing elit.
            """;

        AssertUi(ui, 400, 400, expected);
    }

    private void AssertUi(Ui ui, int width, int height, string expected)
    {
        var root = (UiElementContainer)ui.OpenElementStack.Pop();

        root.PrepareLayout(Dir.Vertical);
        root.Layout(new BoxConstraint(0, width, 0, height));

        var sb = new StringBuilder();

        foreach (var rootChild in root.Children)
        {
            BuildTextualRepresentation(rootChild, sb, 0, rootChild.ParentData.Position);
        }

        var actual = sb.ToString().Trim();

        if (expected != actual)
        {
            _console.WriteLine($"Expected:\n{expected}");
            _console.WriteLine($"\nActual:\n{actual}");
            Assert.Equal(expected, actual);
        }
    }

    private void BuildTextualRepresentation(UiElement element, StringBuilder sb, int indentation, Point position)
    {
        sb.Append(new string(' ', indentation * 4));
        sb.AppendLine($"{element.GetType().Name} = X:{position.X}, Y:{position.Y}, W:{element.Rect.Width}, H:{element.Rect.Height}");

        if (element is UiText text)
        {
            indentation++;
            foreach (var line in text.TextLayoutInfo.Lines)
            {
                sb.Append(new string(' ', indentation * 4));
                sb.AppendLine($"Line = {line.TextContent}");
            }
        }

        if (element is UiElementContainer container)
        {
            indentation++;
            foreach (var child in container.Children)
            {
                BuildTextualRepresentation(child, sb, indentation, position.Add(child.ParentData.Position));
            }
        }
    }

    private Ui GetUi()
    {
        var tree = new UiTree();

        var rootContainer = new FlexContainer
        {
            Id = new UiID("RootElement", "", 0, 0),
            Tree = tree
        };
        tree.Ui.Tree = tree;
        tree.Ui.FontManager = new FontManager();

        tree.Ui.ResetStuff();

        tree.Ui.OpenElementStack.Push(rootContainer);
        rootContainer.OpenElement();

        return tree.Ui;
    }
}
