using System.Runtime.CompilerServices;

namespace Flamui.Components;

public static partial class UiExtensions
{
    public static TabBar GetTabBar(this Ui ui)
    {
        var tabBar = new TabBar
        {
            ActiveTab = ref ui.Get(0),
            IdForNextDeclaredTab = ref ui.Get(0),
            Ui = ui
        };

        using (ui.Rect())
        {
            using (ui.Rect().ShrinkHeight().Direction(Dir.Horizontal).Gap(2))
            {
                tabBar.Header = ui.CreateLayoutScope();
            }

            using (ui.Rect())
            {
                tabBar.Body = ui.CreateLayoutScope();
            }
        }

        tabBar.IdForNextDeclaredTab = 0;
        return tabBar;
    }
}

public ref struct TabBar
{
    public ref int ActiveTab;
    public ref int IdForNextDeclaredTab;
    public LayoutScope Body;
    public LayoutScope Header;
    public Ui Ui;

    public bool TabItem(string name, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
    {
        using var _ = Ui.CreateIdScope(file, lineNumber);

        using (Header.Enter())
        {
            using (var tabItem = Ui.Rect().Color(C.Transparent).Height(30).ShrinkWidth().PaddingHorizontal(10).Center())
            {
                if (tabItem.IsClicked)
                {
                    ActiveTab = IdForNextDeclaredTab;
                }

                Ui.Text(name).Size(20);

                using (var activeIndicator = Ui.Rect().Height(2).Rounded(1).AbsolutePosition(bottom: -2, left: 2).AbsoluteSize(widthOffsetParent:-4))
                {
                    if (ActiveTab == IdForNextDeclaredTab)
                    {
                        activeIndicator.Color(ColorPalette.AccentColor);
                    }
                }
            }
        }

        if (IdForNextDeclaredTab != ActiveTab)
        {
            IdForNextDeclaredTab++;
            return false;
        }

        IdForNextDeclaredTab++;
        return true;
    }
}