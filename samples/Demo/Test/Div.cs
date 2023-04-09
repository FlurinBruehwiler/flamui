using System.Collections;

namespace Demo.Test;

public class Div : IComponent, IEnumerable<Div>
{
    private List<Div>? _children;
    private SizeDefinition _width = new(100, SizeKind.Percentage);
    private SizeDefinition _height = new(100, SizeKind.Percentage);
    private ColorDefinition _color = new(0, 0, 0, 255);
    private float _padding;
    private float _gap;
    private float _radius;
    private float _borderWidth;
    private Dir _dir = Demo.Dir.Column;
    private MAlign _mAlign = Demo.MAlign.FlexStart;
    private XAlign _xAlign = Demo.XAlign.FlexStart;

    private bool ApplyEqualLists(List<DivDefinition> oldElements, List<Div>? newElements)
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

    private bool ChildrenHaveChanges(DivDefinition divDefinition)
    {
        var childCount = _children?.Count ?? 0;
        
        if (divDefinition.Children.Count == childCount)
        {
            return ApplyEqualLists(divDefinition.Children, _children);
        }

        if (_children is null)
            return true;
    
        divDefinition.Children.Clear();
        
        foreach (var child in _children)
        {
            var newDev = new DivDefinition();
            child.ApplyChanges(newDev);
            divDefinition.Children.Add(newDev);
        }
        
        return true;
    }

    public bool ApplyChanges(DivDefinition divDefinition)
    {
        var layoutChange = ChildrenHaveChanges(divDefinition);

        if (_width != divDefinition.Width)
        {
            divDefinition.Width = _width;
            layoutChange = true;
        }

        if (_height != divDefinition.Height)
        {
            divDefinition.Height = _height;
            layoutChange = true;
        }

        if (_color != divDefinition.Color)
        {
            divDefinition.Color = _color;
        }

        if (_padding != divDefinition.Padding)
        {
            divDefinition.Padding = _padding;
            layoutChange = true;
        }

        if (_gap != divDefinition.Gap)
        {
            divDefinition.Gap = _gap;
            layoutChange = true;
        }

        if (_radius != divDefinition.Radius)
        {
            divDefinition.Radius = _radius;
        }

        if (_borderWidth != divDefinition.BorderWidth)
        {
            divDefinition.BorderWidth = _borderWidth;
        }

        if (_dir != divDefinition.Dir)
        {
            divDefinition.Dir = _dir;
            layoutChange = true;
        }

        if (_mAlign != divDefinition.MAlign)
        {
            divDefinition.MAlign = _mAlign;
            layoutChange = true;
        }

        if (_xAlign != divDefinition.XAlign)
        {
            divDefinition.XAlign = _xAlign;
            layoutChange = true;
        }

        return layoutChange;
    }

    public Div Items(IEnumerable<Div> children)
    {
        _children ??= new List<Div>();
        _children.AddRange(children);
        return this;
    }

    public IComponent Add(Div child)
    {
        _children ??= new List<Div>();
        _children.Add(child);
        return this;
    }

    public Div Width(float width, SizeKind sizeKind = SizeKind.Pixel)
    {
        _width = new SizeDefinition(width, sizeKind);
        return this;
    }

    public Div Height(float height, SizeKind sizeKind = SizeKind.Pixel)
    {
        _height = new SizeDefinition(height, sizeKind);
        return this;
    }

    public Div Color(float red, float green, float blue, float transparency = 255)
    {
        _color = new ColorDefinition(red, green, blue, transparency);
        return this;
    }

    public Div Color(ColorDefinition color)
    {
        _color = color;
        return this;
    }

    public Div Padding(float padding)
    {
        _padding = padding;
        return this;
    }

    public Div Gap(float gap)
    {
        _gap = gap;
        return this;
    }

    public Div Radius(float radius)
    {
        _radius = radius;
        return this;
    }

    public Div BorderWidth(float borderWidth)
    {
        _borderWidth = borderWidth;
        return this;
    }

    public Div Dir(Dir dir)
    {
        _dir = dir;
        return this;
    }

    public Div MAlign(MAlign mAlign)
    {
        _mAlign = mAlign;
        return this;
    }

    public Div XAlign(XAlign xAlign)
    {
        _xAlign = xAlign;
        return this;
    }

    public IEnumerator<Div> GetEnumerator()
    {
        return _children?.GetEnumerator() ?? Enumerable.Empty<Div>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}