using System.Numerics;
using Flamui.Drawing;
using Flamui.Layouting;
using Flamui.UiElements;
using Silk.NET.Input;
using Silk.NET.Maths;
using MouseButton = Silk.NET.Input.MouseButton;

namespace Flamui;

public partial class UiTree
{
    // public IWindow Window;
    private FlamuiComponent _rootComponent;
    public IServiceProvider ServiceProvider;
    public bool IsDebugWindow;
    public Arena Arena;
    public RenderContext RenderContext;

    // private UiContainer? _hoveredContainer;
    private UiElement? _activeContainer;

    public UiElementContainer RootContainer;

    // private UiContainer? HoveredDiv
    // {
    //     get => _hoveredContainer;
    //     set
    //     {
    //         if (HoveredDiv is not null)
    //         {
    //             HoveredDiv.IsHovered = false;
    //         }
    //
    //         _hoveredContainer = value;
    //         if (value is not null)
    //         {
    //             value.IsHovered = true;
    //         }
    //     }
    // }

    public Input Input;
    private readonly TabIndexManager _tabIndexManager = new();
    public readonly Ui Ui = new();

    public UiElement? ActiveDiv
    {
        get => _activeContainer;
        set
        {
            if (ActiveDiv is not null)
            {
                ActiveDiv.IsActive = false;
            }

            _activeContainer = value;
            if (value is not null)
            {
                value.IsActive = true;
            }
        }
    }

    public List<UiElement> HoveredElements { get; set; } = new();
    public List<UiElement> OldHoveredElements { get; set; } = new();
    private RegistrationManager _registrationManager;

    //for testing
    public UiTree(Ui ui)
    {
        Ui = ui;
    }

    public UiTree(FlamuiComponent rootComponent, IServiceProvider serviceProvider)
    {
        _rootComponent = rootComponent;
        ServiceProvider = serviceProvider;
    }

    public double DeltaTime;

    public void Update(float width, float height)
    {
        if (!isInitialized)
            return;

        // Ui.Arena = RenderContext.Arena;

        ProcessInputs();

        //ToDo cleanup
        foreach (var action in _registrationManager.OnAfterInput)
        {
            action(this);
        }

        HitDetection();

        HandleZoomAndStuff();

        BuildUi(width, height);

        Input.OnAfterFrame();
    }

    private void HandleZoomAndStuff()
    {
        if (IsKeyPressed(Key.Escape))
        {
            Close();
        }

        if (IsMouseButtonDown(MouseButton.Right))
        {
            var mouseDelta = MouseDelta;
            ZoomTarget += mouseDelta * -1 / Zoom;
        }
        
        if (IsKeyDown(Key.ControlLeft) && ScrollDeltaY != 0)
        {
            var factor = (float)Math.Pow(1.1, ScrollDeltaY);
            UserScaling *= new Vector2(factor, factor);
            UserScaling = new Vector2(Math.Clamp(UserScaling.X, 0.1f, 10f), Math.Clamp(UserScaling.Y, 0.1f, 10f));
        }

        if (IsKeyDown(Key.AltLeft) && ScrollDeltaY != 0)
        {
            var factor = (float)Math.Pow(1.1, ScrollDeltaY);
            var mouseWorldPos = MousePosition;
            ZoomOffset = MouseScreenPosition;
            ZoomTarget = mouseWorldPos;
            Zoom *= new Vector2(factor, factor);
            Zoom = new Vector2(Math.Clamp(Zoom.X, 0.01f, 100f), Math.Clamp(Zoom.Y, 0.01f, 100f));
        }

        if (IsKeyPressed(Key.R))
        {
            UserScaling = new Vector2(1, 1);
            Zoom = new Vector2(1, 1);
            ZoomOffset = new Vector2();
            ZoomTarget = new Vector2();
        }
    }

    private Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        if (Matrix4X4.Invert(GetWorldToScreenMatrix(), out var inverted))
        {
            return screenPosition.Multiply(inverted);
        }

        throw new Exception("ahh");
    }

    public Matrix4X4<float> GetWorldToScreenMatrix()
    {
        var origin = Matrix4X4.CreateTranslation(-ZoomTarget.X, -ZoomTarget.Y, 0);
        var scale = Matrix4X4.CreateScale(GetCompleteScaling().X * Zoom.X);

        var translate = Matrix4X4.CreateTranslation(ZoomOffset.X, ZoomOffset.Y, 0);

        return  origin * scale * translate;
    }

    // private ZoomType zoomType = ZoomType.ScaleContent;

    public Vector2 Zoom = new(1, 1);
    public Vector2 ZoomOffset = new(0, 0);
    public Vector2 ZoomTarget;

    /// <summary>
    /// The DPI scaling from the OS
    /// </summary>
    public Vector2 DpiScaling;

    /// <summary>
    /// The User can zoom in, to make stuff bigger
    /// </summary>
    public Vector2 UserScaling = new(1, 1);

    /// <summary>
    /// the complete scaling
    /// </summary>
    public Vector2 GetCompleteScaling() => DpiScaling * UserScaling;


    private bool isInitialized;

    public void Close()
    {
        // Window.Close();
    }

    private void HitDetection()
    {
        // _hitTester.HandleHitTest();
    }


    private void ProcessInputs()
    {
        _tabIndexManager.HandleTab(this);
    }

    private void BuildUi(float width, float height)
    {
        Ui.ResetStuff();
        Ui.OpenElementStack.Push(RootContainer);

        RootContainer.OpenElement();

        _rootComponent.Build(Ui);

        RootContainer.PrepareLayout(Dir.Vertical);
        RootContainer.Layout(new BoxConstraint(0, width / GetCompleteScaling().X, 0, height / GetCompleteScaling().Y));
    }
}
