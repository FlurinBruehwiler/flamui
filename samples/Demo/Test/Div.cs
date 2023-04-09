using System.Collections;
using System.ComponentModel;

namespace Demo.Test;

public class Div : IComponent, IEnumerable<Div>
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public List<Div>? Children { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public SizeDefinition PWidth { get; set; } = new(100, SizeKind.Percentage);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public SizeDefinition PHeight { get; set; } = new(100, SizeKind.Percentage);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public ColorDefinition PColor { get; set; } = new(0, 0, 0, 255);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int PPadding { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int PGap { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int PRadius { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int PBorderWidth { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Dir PDir { get; set; } = Demo.Dir.Column;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public MAlign PMAlign { get; set; } = Demo.MAlign.FlexStart;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public XAlign PXAlign { get; set; } = Demo.XAlign.FlexStart;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float ComputedHeight { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float ComputedWidth { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float ComputedX { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float ComputedY { get; set; }

    private bool ApplyEqualLists(List<Div> oldElements, List<Div>? newElements)
    {
        if (newElements is null)
            return false;

        var hasLayoutChange = false;

        for (var i = 0; i < oldElements.Count; i++)
        {
            var previous = oldElements[i];
            var current = newElements[i];

            if (current.ApplyChanges(previous))
            {
                hasLayoutChange = true;
            }
        }

        return hasLayoutChange;
    }

    private bool ChildrenHaveChanges(Div divDefinition)
    {
        var childCount = Children?.Count ?? 0;

        if (divDefinition.Children.Count == childCount)
        {
            return ApplyEqualLists(divDefinition.Children, Children);
        }

        if (Children is null)
            return true;

        divDefinition.Children.Clear();

        foreach (var child in Children)
        {
            var newDev = new Div();
            child.ApplyChanges(newDev);
            divDefinition.Children.Add(newDev);
        }

        return true;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool ApplyChanges(Div divDefinition)
    {
        var layoutChange = ChildrenHaveChanges(divDefinition);

        if (Width != divDefinition.Width)
        {
            divDefinition.PWidth = PWidth;
            layoutChange = true;
        }

        if (PHeight != divDefinition.PHeight)
        {
            divDefinition.PHeight = PHeight;
            layoutChange = true;
        }

        if (PColor != divDefinition.PColor)
        {
            divDefinition.PColor = PColor;
        }

        if (PPadding != divDefinition.PPadding)
        {
            divDefinition.PPadding = PPadding;
            layoutChange = true;
        }

        if (PGap != divDefinition.PGap)
        {
            divDefinition.PGap = PGap;
            layoutChange = true;
        }

        if (PRadius != divDefinition.PRadius)
        {
            divDefinition.PRadius = PRadius;
        }

        if (PBorderWidth != divDefinition.PBorderWidth)
        {
            divDefinition.PBorderWidth = PBorderWidth;
        }

        if (PDir != divDefinition.PDir)
        {
            divDefinition.PDir = PDir;
            layoutChange = true;
        }

        if (PMAlign != divDefinition.PMAlign)
        {
            divDefinition.PMAlign = PMAlign;
            layoutChange = true;
        }

        if (PXAlign != divDefinition.PXAlign)
        {
            divDefinition.PXAlign = PXAlign;
            layoutChange = true;
        }

        return layoutChange;
    }

    public Div Items(IEnumerable<Div> children)
    {
        Children ??= new List<Div>();
        Children.AddRange(children);
        return this;
    }

    public IComponent Add(Div child)
    {
        Children ??= new List<Div>();
        Children.Add(child);
        return this;
    }

    public Div Width(float width, SizeKind sizeKind = SizeKind.Pixel)
    {
        PWidth = new SizeDefinition(width, sizeKind);
        return this;
    }

    public Div Height(float height, SizeKind sizeKind = SizeKind.Pixel)
    {
        PHeight = new SizeDefinition(height, sizeKind);
        return this;
    }

    public Div Color(float red, float green, float blue, float transparency = 255)
    {
        PColor = new ColorDefinition(red, green, blue, transparency);
        return this;
    }

    public Div Color(ColorDefinition color)
    {
        PColor = color;
        return this;
    }

    public Div Padding(int padding)
    {
        PPadding = padding;
        return this;
    }

    public Div Gap(int gap)
    {
        PGap = gap;
        return this;
    }

    public Div Radius(int radius)
    {
        PRadius = radius;
        return this;
    }

    public Div BorderWidth(int borderWidth)
    {
        PBorderWidth = borderWidth;
        return this;
    }

    public Div Dir(Dir dir)
    {
        PDir = dir;
        return this;
    }

    public Div MAlign(MAlign mAlign)
    {
        PMAlign = mAlign;
        return this;
    }

    public Div XAlign(XAlign xAlign)
    {
        PXAlign = xAlign;
        return this;
    }

    public Div OnClick(Action<int> callback)
    {
        return this;
    }

    public Div OnClick(Func<int, Task> callback)
    {
        return this;
    }

    public Div OnHover(Action<int> callback)
    {
        return this;
    }

    public Div OnHover(Func<int, Task> callback)
    {
        return this;
    }

    public IEnumerator<Div> GetEnumerator()
    {
        return Children?.GetEnumerator() ?? Enumerable.Empty<Div>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}