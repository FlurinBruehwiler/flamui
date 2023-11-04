using ImSharpUISample.UiElements;

namespace ImSharpUISample;

public abstract class UiComponent
{
    public virtual void OnInitialized()
    {

    }

    public abstract void Build();
}

public class Sidebar : UiComponent
{
    private SidebarSide _side;
    private int _windowWidth = 300;
    private ToolWindowDefinition? _selectedToolWindow;
    private readonly List<ToolWindowDefinition> _toolWindowDefinitions = new();
    private bool _isDragging = false;

    //ToDo free cursor with https://wiki.libsdl.org/SDL2/SDL_FreeCursor
    private readonly IntPtr _resizeCursor = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEWE);
    private readonly IntPtr _normalCursor = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);

    public override void OnInitialized()
    {
        _selectedToolWindow = _toolWindowDefinitions.FirstOrDefault();
    }

    public override void Build()
    {
        //Sidebar
        DivStart(_side.ToString()).Color(C.Background).Width(40).Padding(5).Gap(10).BlockHit();

            foreach (var toolWindowDefinition in _toolWindowDefinitions)
            {
                SidebarIcon(toolWindowDefinition);
            }

            //Window
            if (_selectedToolWindow is {} selectedToolWindow)
            {
                DivStart(out var toolWindow, _side.ToString()).ZIndex(1).BlockHit().Color(C.Background).BorderWidth(1).BorderColor(20, 20, 20);

                    DivStart(out var dragZone).Absolute().Width(4).BlockHit();
                        if (_side == SidebarSide.Left)
                            dragZone.Absolute(right: -2);
                        else
                            dragZone.Absolute(left: -2);

                        HandleWindowResize(dragZone);
                    DivEnd();

                    toolWindow.Width(_windowWidth);

                    if (_side == SidebarSide.Left)
                        toolWindow.Absolute(left: 40);
                    else
                        toolWindow.Absolute(left: -_windowWidth);

                    var comp = (UiComponent)GetComponent(selectedToolWindow.WindowComponent, selectedToolWindow.Path);
                    comp.Build();
                DivEnd();
            }

        DivEnd();

        _toolWindowDefinitions.Clear();
    }

    public void ToolWindow<T>(string icon) where T : UiComponent
    {
        //ToDo remove memory allocation
        _toolWindowDefinitions.Add(new ToolWindowDefinition($"./Icons/{icon}.svg", typeof(T)));
    }

    public void Side(SidebarSide side)
    {
        _side = side;
    }

    private void HandleWindowResize(UiContainer dragZone)
    {
        if (dragZone.IsNewlyHovered)
            SDL_SetCursor(_resizeCursor);
        if (dragZone.IsNewlyUnHovered && !_isDragging)
            SDL_SetCursor(_normalCursor);

        if (dragZone.IsHovered && Window.IsMouseButtonPressed(MouseButtonKind.Left))
        {
            _isDragging = true;
        }

        if (_isDragging && Window.IsMouseButtonReleased(MouseButtonKind.Left))
        {
            _isDragging = false;
            SDL_SetCursor(_normalCursor);
        }

        if (_isDragging)
        {
            if (_side == SidebarSide.Left)
            {
                _windowWidth += (int)Window.MouseDelta.X;
            }
            else
            {
                _windowWidth -= (int)Window.MouseDelta.X;
            }
        }
    }

    private void SidebarIcon(ToolWindowDefinition toolWindowDefinition)
    {
        DivStart(out var toolbar, toolWindowDefinition.Path).Rounded(5).Padding(3).Height(30).Color(C.Transparent);
            if (toolbar.Clicked)
            {
                if (toolWindowDefinition == _selectedToolWindow)
                {
                    _selectedToolWindow = null;
                }
                else
                {
                    _selectedToolWindow = toolWindowDefinition;
                }
            }

            if (toolWindowDefinition == _selectedToolWindow || toolbar.IsHovered)
            {
                toolbar.Color(C.Selected);
            }
            SvgImage(toolWindowDefinition.Path);
        DivEnd();
    }
}
