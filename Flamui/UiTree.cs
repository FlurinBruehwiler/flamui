using System.Numerics;
using Flamui.Drawing;
using Flamui.Layouting;
using Flamui.UiElements;
using Silk.NET.Input;
using Silk.NET.Maths;
using MouseButton = Silk.NET.Input.MouseButton;

namespace Flamui;

/*
What is a UiTree?

The typical arrangement is:

Window
    UiTree

But we can also have headless uiTrees for testing

UiTree
UiTree
etc.

Can we have nested UiTrees??
We want it for the purpose of a F12 debug menu, there we have a completely isolated DebugMenu / Overlay

Window
    UiTree (The debug Menu / overlay)
        UiTree (the actual application)

It is also useful for some kind of plugin architecture, where a plugin can host content within the host app, without being able to affect it.
The question is, shouldn't this be already handled by the hierarchical nature of the uiTree, should lower nodes be able to affect upper nodes?
The thing is, we want input isolation, input data is accessible throughout a UiTree, but if we want to isolate input, we probably need nested UiTrees.
I'm not yet 100% sure about this.

 */
public partial class UiTree
{
    private FlamuiComponent _rootComponent; //should this be here, should the component model be a fundamental part of flamui?
    public RenderContext _renderContext = new RenderContext();
    private UiElement? _activeContainer;
    public UiElementContainer RootContainer;
    // public Input Input;
    private readonly TabIndexManager _tabIndexManager = new();
    public readonly Ui Ui = new();
    List<UiElement> hitElements = new();
    public IUiTreeHost UiTreeHost;


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

    //for testing
    public UiTree()
    {
    }

    public UiTree(FlamuiComponent rootComponent)
    {
        _rootComponent = rootComponent;

        RootContainer = new FlexContainer
        {
            Id = new UiID("anita", "", 0, 0),
            Tree = this
        };
        Ui.Tree = this;
        currentArena = new Arena($"PerFrameArena1 {++incId}", 1_000_000);
        lastArena = new Arena($"PerFramArena2 {++incId}", 1_000_000);
    }

    private static int incId;

    private Arena currentArena;
    private Arena lastArena;

    public void Update(float width, float height)
    {
        (currentArena, lastArena) = (lastArena, currentArena);
        currentArena.Reset();
        Ui.Arena = currentArena;

        _tabIndexManager.HandleTab(this);

        HandleHitTest();

        BuildUi(width, height);

        CleanupInputAfterFrame();
    }

    public void HandleHitTest()
    {
        HitTest(MousePosition);

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
        HoveredElements.Clear();

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
