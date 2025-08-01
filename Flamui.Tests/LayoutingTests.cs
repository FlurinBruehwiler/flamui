using System.Text;
using Flamui.Layouting;
using Flamui.UiElements;
using Xunit.Abstractions;

namespace Flamui.Tests;

public sealed class LayoutingTests : IDisposable
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

        using (ui.Rect())
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

        using (ui.Rect())
        {
            using (ui.Rect())
            {

            }

            using (ui.Rect())
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

        using (ui.Rect().Direction(Dir.Horizontal))
        {
            using (ui.Rect())
            {

            }

            using (ui.Rect())
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

        using (ui.Rect().Direction(Dir.Horizontal))
        {
            using (ui.Rect().Width(40))
            {

            }

            using (ui.Rect().Width(30))
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

        using (ui.Rect().Direction(Dir.Horizontal).Gap(10))
        {
            using (ui.Rect().Width(40))
            {

            }

            using (ui.Rect().Width(30))
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

        using (ui.Rect().Direction(Dir.Horizontal).Padding(10))
        {
            using (ui.Rect())
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

        using (ui.Rect().Direction(Dir.Horizontal))
        {
            using (ui.Rect().WidthFraction(50))
            {

            }

            using (ui.Rect().Width(30))
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

        using (ui.Rect().Direction(Dir.Horizontal).MainAlign(MAlign.SpaceBetween))
        {
            using (ui.Rect().Width(10))
            {

            }

            using (ui.Rect().Width(10))
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
    public void WidthFraction_0_Percent()
    {
        var ui = GetUi();

        using (ui.Rect())
        {
            using (ui.Rect().WidthFraction(0))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:0, Y:0, W:0, H:100
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void MainAlign_SpaceBetween_2()
    {
        var ui = GetUi();

        using (ui.Rect().MainAlign(MAlign.SpaceBetween))
        {
            using (ui.Rect().Height(10))
            {

            }

            using (ui.Rect().Height(10))
            {

            }

            using (ui.Rect().Height(10))
            {

            }
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:100, H:100
                FlexContainer = X:0, Y:0, W:100, H:10
                FlexContainer = X:0, Y:45, W:100, H:10
                FlexContainer = X:0, Y:90, W:100, H:10
            """;

        AssertUi(ui, 100, 100, expected);
    }

    [Fact]
    public void MainAlign_Center()
    {
        var ui = GetUi();

        using (ui.Rect().Direction(Dir.Horizontal).MainAlign(MAlign.Center))
        {
            using (ui.Rect().Width(10))
            {

            }

            using (ui.Rect().Width(10))
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

        using (ui.Rect().Direction(Dir.Horizontal).MainAlign(MAlign.End))
        {
            using (ui.Rect().Width(10))
            {

            }

            using (ui.Rect().Width(10))
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

        using (ui.Rect().Direction(Dir.Horizontal))
        {
            using (ui.Rect().Margin(10).Width(10))
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

        using (ui.Rect().Direction(Dir.Horizontal))
        {
            using (ui.Rect().MarginLeft(10).Width(10))
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

        using (ui.Rect().Direction(Dir.Horizontal))
        {
            using (ui.Rect().MarginRight(10).Width(10))
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

        using (ui.Rect().Direction(Dir.Horizontal))
        {
            using (ui.Rect().MarginTop(10).Width(10))
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

        using (ui.Rect().Direction(Dir.Horizontal))
        {
            using (ui.Rect().Width(10).HeightFraction(50))
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

        using (ui.Rect().Direction(Dir.Horizontal))
        {
            using (ui.Rect().MarginTop(10).Width(10))
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

        using (ui.Rect().Direction(Dir.Horizontal).Padding(10))
        {
            using (ui.Rect().Margin(10).Width(10))
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

        using (ui.Rect())
        {
            using (ui.Rect().Margin(10))
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
    public void MarginWithMainAlignCenter()
    {
        var ui = GetUi();

        using (ui.Rect().MainAlign(MAlign.Center))
        {
            using (ui.Rect().Margin(10))
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
    public void MarginWithMainAlignEnd()
    {
        var ui = GetUi();

        using (ui.Rect().MainAlign(MAlign.End))
        {
            using (ui.Rect().Margin(10))
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
    public void MarginWithMainAlignSpaceBetween()
    {
        var ui = GetUi();

        using (ui.Rect().MainAlign(MAlign.SpaceBetween))
        {
            using (ui.Rect().Margin(10))
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

        using (ui.Rect().CrossAlign(XAlign.Center))
        {
            using (ui.Rect().Width(10).Height(10))
            {

            }

            using (ui.Rect().Width(10).Height(10))
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

        using (ui.Rect().CrossAlign(XAlign.End))
        {
            using (ui.Rect().Width(10).Height(10))
            {

            }

            using (ui.Rect().Width(10).Height(10))
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

        using (ui.Rect().Center().Direction(Dir.Horizontal))
        {
            using (ui.Rect().Width(10).Height(10))
            {

            }

            using (ui.Rect().Width(10).Height(10))
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

        using (ui.Rect().Center().Direction(Dir.Vertical))
        {
            using (ui.Rect().Width(10).Height(10))
            {

            }

            using (ui.Rect().Width(10).Height(10))
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

        using (ui.Rect().Center().Direction(Dir.Horizontal).Gap(10))
        {
            using (ui.Rect().Width(10).Height(10))
            {

            }

            using (ui.Rect().Width(10).Height(10))
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

        using (ui.Rect().Padding(10))
        {
            using (ui.Rect().HeightFraction(50))
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

        using (ui.Rect().PaddingLeft(10))
        {
            using (ui.Rect())
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

        using (ui.Rect().PaddingRight(10))
        {
            using (ui.Rect())
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

        using (ui.Rect().PaddingTop(10))
        {
            using (ui.Rect())
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

        using (ui.Rect().PaddingBottom(10))
        {
            using (ui.Rect())
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

        using (ui.Rect().Shrink())
        {
            using (ui.Rect().Width(10).Height(10))
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

        using (ui.Rect().Shrink().Padding(10))
        {
            using (ui.Rect().Width(10).Height(10))
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

        using (ui.Rect().Shrink())
        {
            using (ui.Rect().Width(10).Height(10).Margin(10))
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

        using (ui.Rect().ShrinkWidth().Gap(10).Direction(Dir.Horizontal))
        {
            using (ui.Rect().Width(10))
            {

            }

            using (ui.Rect().Width(10))
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

        using (ui.Rect().ShrinkHeight().Gap(10).Direction(Dir.Horizontal))
        {
            using (ui.Rect())
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

        using (ui.Rect().Shrink(minWidth: 50, minHeight: 50))
        {
            using (ui.Rect().Width(10).Height(10))
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

        using (ui.Rect().Direction(Dir.Horizontal))
        {
            using (ui.Rect().ShrinkWidth())
            {
                using (ui.Rect().Width(10))
                {

                }
            }

            using (ui.Rect())
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

        using (ui.Rect().Shrink())
        {
            using (ui.Rect().Shrink())
            {
                using (ui.Rect().Width(10).Height(10))
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

        using (ui.Rect().Shrink())
        {
            using (ui.Rect())
            {
                using (ui.Rect().Width(10).Height(10))
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

        using (ui.Rect().Direction(Dir.Horizontal))
        {
            using (ui.Rect().ShrinkWidth())
            {
                using (ui.Rect().Width(10))
                {

                }
            }

            using (ui.Rect().WidthFraction(50))
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

        using (ui.Rect())
        {
            using (ui.Rect())
            {

            }

            using (ui.Rect().Absolute().Width(88).Height(78))
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

        using (ui.Rect().Center())
        {
            using (ui.Rect().Width(50).Height(50).Center())
            {
                using (ui.Rect().Width(10).Height(10))
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

        using (ui.Rect().Center())
        {
            using (ui.Rect().Width(10).Height(10))
            {
                using (ui.Rect().AbsolutePosition(top: -10, left: -10))
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

        using (ui.Rect().Center())
        {
            using (ui.Rect().Width(10).Height(10))
            {
                using (ui.Rect().AbsolutePosition(bottom: 10, right: 10))
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

        using (ui.Rect().Center())
        {
            using (ui.Rect().Width(10).Height(10))
            {
                using (ui.Rect().AbsoluteSize(widthOffsetParent: 0, heightOffsetParent: 0))
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

        using (ui.Rect().Direction(Dir.Horizontal))
        {
            ui.Text(loremIpsum + "\n" + loremIpsum).Multiline().Color(188, 190, 196);
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:400, H:100
                UiText = X:0, Y:0, W:274.55945, H:32
                    Line = Lorem ipsum dolor sit amet, consectetur adipiscing elit.
                    Line = Lorem ipsum dolor sit amet, consectetur adipiscing elit.
            """;

        AssertUi(ui, 400, 100, expected);
    }

    [Fact]
    public void Text_Wrap()
    {
        var ui = GetUi();

        using (ui.Rect().Direction(Dir.Horizontal))
        {
            ui.Text(loremIpsum).Multiline().Color(188, 190, 196);
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:200, H:100
                UiText = X:0, Y:0, W:142.5991, H:32
                    Line = Lorem ipsum dolor sit amet, 
                    Line = consectetur adipiscing elit.
            """;

        AssertUi(ui, 200, 100, expected);
    }

    [Fact]
    public void Multiple_Text_Sizes()
    {
        var ui = GetUi();

        using (ui.Rect().Direction(Dir.Vertical))
        {
            ui.Text(loremIpsum).Multiline().Size(20);
            ui.Text(loremIpsum).Multiline().Size(40);
        }

        var expected =
            """
            FlexContainer = X:0, Y:0, W:400, H:400
                UiText = X:0, Y:0, W:366.07925, H:21
                    Line = Lorem ipsum dolor sit amet, consectetur adipiscing elit.
                UiText = X:0, Y:21, W:380.2643, H:82
                    Line = Lorem ipsum dolor sit amet, 
                    Line = consectetur adipiscing elit.
            """;

        AssertUi(ui, 400, 400, expected);
    }

    [Fact]
    public void Grid_Single_Column()
    {
        var ui = GetUi();

        using (var grid = ui.Grid())
        {
            grid.DefineColumn(width: 100, fractional: true);

            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    using (ui.Rect().Height(10))
                    {

                    }
                }
            }
        }

        var expected =
            """
            Grid = X:0, Y:0, W:0, H:0
                FlexContainer = X:0, Y:0, W:400, H:10
                FlexContainer = X:0, Y:10, W:400, H:10
                FlexContainer = X:0, Y:20, W:400, H:10
            """;

        AssertUi(ui, 400, 400, expected);
    }

    [Fact]
    public void Grid_Multiple_Column()
    {
        var ui = GetUi();

        using (var grid = ui.Grid())
        {
            grid.DefineColumn(width: 100, fractional: true);
            grid.DefineColumn(width: 100, fractional: true);
            grid.DefineColumn(width: 100, fractional: true);

            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    using (ui.Rect().Height(10))
                    {

                    }
                }
            }
        }

        var expected =
            """
            Grid = X:0, Y:0, W:0, H:0
                FlexContainer = X:0, Y:0, W:133.33334, H:10
                FlexContainer = X:133.33334, Y:0, W:133.33334, H:10
                FlexContainer = X:266.6667, Y:0, W:133.33334, H:10
            """;

        AssertUi(ui, 400, 400, expected);
    }

    [Fact]
    public void Grid_Multiple_Row()
    {
        var ui = GetUi();

        using (var grid = ui.Grid())
        {
            grid.DefineColumn(width: 100, fractional: true);
            grid.DefineColumn(width: 100, fractional: true);
            grid.DefineColumn(width: 100, fractional: true);

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    using (ui.Rect().Height(10))
                    {

                    }
                }
            }
        }

        var expected =
            """
            Grid = X:0, Y:0, W:0, H:0
                FlexContainer = X:0, Y:0, W:133.33334, H:10
                FlexContainer = X:133.33334, Y:0, W:133.33334, H:10
                FlexContainer = X:266.6667, Y:0, W:133.33334, H:10
                FlexContainer = X:0, Y:10, W:133.33334, H:10
            """;

        AssertUi(ui, 400, 400, expected);
    }

    [Fact]
    public void Grid_Margin()
    {
        var ui = GetUi();

        using (var grid = ui.Grid())
        {
            grid.DefineColumn(width: 100, fractional: true);
            grid.DefineColumn(width: 100, fractional: true);
            grid.DefineColumn(width: 100, fractional: true);

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    using (ui.Rect().Height(10).Margin(2))
                    {

                    }
                }
            }
        }

        var expected =
            """
            Grid = X:0, Y:0, W:0, H:0
                FlexContainer = X:2, Y:2, W:129.33334, H:10
                FlexContainer = X:135.33334, Y:2, W:129.33334, H:10
                FlexContainer = X:268.6667, Y:2, W:129.33334, H:10
                FlexContainer = X:2, Y:16, W:129.33334, H:10
            """;

        AssertUi(ui, 400, 400, expected);
    }

    [NoScopeGeneration]
    private void AssertUi(Ui ui, int width, int height, string expected)
    {
        var root = ui.PopElement();

        root.PrepareLayout(Dir.Vertical);
        root.Layout(new BoxConstraint(0, width, 0, height));

        var sb = new StringBuilder();

        foreach (var rootChild in root.Children)
        {
            BuildTextualRepresentation(rootChild, sb, 0, rootChild.ParentData.Position);
        }

        var actual = sb.ToString().Trim();

        expected = expected.Replace("\r\n", "\n");
        actual = actual.Replace("\r\n", "\n");

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
        sb.Append($"{element.GetType().Name} = X:{position.X}, Y:{position.Y}, W:{element.Rect.Width}, H:{element.Rect.Height}\n");

        if (element is UiText text)
        {
            indentation++;
            foreach (var line in text.TextLayoutInfo.Lines)
            {
                sb.Append(new string(' ', indentation * 4));
                sb.Append($"Line = {line.TextContent}\n");
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
            Id = 0,
            Tree = tree
        };
        tree.Ui.Tree = tree;
        tree.Ui.FontManager = new FontManager();
        Ui.Arena = new Arena("test_arena", 10_000);
        tree.Ui.Root = rootContainer;

        tree.Ui.ResetStuff();

        tree.Ui.PushScope(-4711);
        tree.Ui.PushOpenElement(rootContainer);
        rootContainer.OpenElement();

        return tree.Ui;
    }
}
