﻿using System.Numerics;
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
    public RenderContext _renderContext;

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
    public UiTree()
    {
    }

    public UiTree(FlamuiComponent rootComponent, IServiceProvider serviceProvider)
    {
        _rootComponent = rootComponent;
        ServiceProvider = serviceProvider;
    }

    public void Update(float width, float height)
    {
        // Ui.Arena = RenderContext.Arena;

        ProcessInputs();

        //ToDo cleanup
        foreach (var action in _registrationManager.OnAfterInput)
        {
            action(this);
        }

        HandleHitTest();

        // HandleZoomAndStuff();

        BuildUi(width, height);

        Input.OnAfterFrame();
    }

    // private void HandleZoomAndStuff()
    // {
    //     if (IsKeyPressed(Key.Escape))
    //     {
    //         //todo(refactor)
    //     }
    //
    //     if (IsMouseButtonDown(MouseButton.Right))
    //     {
    //         var mouseDelta = MouseDelta;
    //         ZoomTarget += mouseDelta * -1 / Zoom;
    //     }
    //
    //     if (IsKeyDown(Key.ControlLeft) && ScrollDeltaY != 0)
    //     {
    //         var factor = (float)Math.Pow(1.1, ScrollDeltaY);
    //         UserScaling *= new Vector2(factor, factor);
    //         UserScaling = new Vector2(Math.Clamp(UserScaling.X, 0.1f, 10f), Math.Clamp(UserScaling.Y, 0.1f, 10f));
    //     }
    //
    //     if (IsKeyDown(Key.AltLeft) && ScrollDeltaY != 0)
    //     {
    //         var factor = (float)Math.Pow(1.1, ScrollDeltaY);
    //         var mouseWorldPos = MousePosition;
    //         ZoomOffset = MouseScreenPosition;
    //         ZoomTarget = mouseWorldPos;
    //         Zoom *= new Vector2(factor, factor);
    //         Zoom = new Vector2(Math.Clamp(Zoom.X, 0.01f, 100f), Math.Clamp(Zoom.Y, 0.01f, 100f));
    //     }
    //
    //     if (IsKeyPressed(Key.R))
    //     {
    //         UserScaling = new Vector2(1, 1);
    //         Zoom = new Vector2(1, 1);
    //         ZoomOffset = new Vector2();
    //         ZoomTarget = new Vector2();
    //     }
    // }


    List<UiElement> hitElements = new();

    public void HandleHitTest()
    {
        HitTest(MouseScreenPosition);

        if (IsMouseButtonPressed(MouseButton.Left))
        {
            foreach (var windowHoveredDiv in HoveredElements)
            {
                if (windowHoveredDiv is FlexContainer { Info.Focusable:true} uiContainer)
                {
                    ActiveDiv = uiContainer;
                    return;
                }
            }

            ActiveDiv = null;
        }
    }
    
    private void HitTest(Vector2 originalPoint)
    {
        var transformedPoint = originalPoint;

        hitElements.Clear();

        //from back to front
        foreach (var (_, value) in _renderContext.CommandBuffers.OrderBy(x => x.Key))
        {
            foreach (var command in value)
            {
                if (command.Type == CommandType.Matrix)
                {
                    transformedPoint = originalPoint.Multiply(command.Matrix.Invert());
                }
                else if (command.Type == CommandType.Rect)
                {
                    command.UiElement.Get().FinalOnScreenSize = command.Bounds;
                    if (command.Bounds.ContainsPoint(transformedPoint))
                    {
                        hitElements.Add(command.UiElement.Get());
                    }
                }else if (command.Type == CommandType.Text)
                {
                    command.UiElement.Get().FinalOnScreenSize = command.Bounds;
                    if (command.Bounds.ContainsPoint(transformedPoint))
                    {
                        hitElements.Add(command.UiElement.Get());
                    }
                }
            }
        }

        //from front to back
        for (var i = hitElements.Count - 1; i >= 0; i--)
        {
            var hitElement = hitElements[i];

            if (hitElement is { } uiElement)
            {
                HoveredElements.Add(uiElement);
            }

            if (hitElement is FlexContainer { Info.BlockHit: true })
            {
                return;
            }
        }
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
        RootContainer.Layout(new BoxConstraint(0, width, 0, height));
    }
}
