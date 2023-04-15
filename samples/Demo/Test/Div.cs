using System.Collections;
using System.ComponentModel;
using SkiaSharp;

namespace Demo.Test;

public class Div : RenderObject, IEnumerable<RenderObject>
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public List<RenderObject>? Children { get; set; }

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
    public MAlign PmAlign { get; set; } = Demo.MAlign.FlexStart;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public XAlign PxAlign { get; set; } = Demo.XAlign.FlexStart;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Action? POnClick { get; set; }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Func<Task>? POnClickAsync { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool LayoutHasChanged(Div oldDiv)
    {
        if (PWidth != oldDiv.PWidth)
            return true;

        if (PHeight != oldDiv.PHeight)
            return true;
        
        if ((oldDiv.Children?.Count ?? 0) != (Children?.Count ?? 0))
            return true;

        if (PPadding != oldDiv.PPadding)
            return true;

        if (PGap != oldDiv.PGap)
            return true;
        
        if (PDir != oldDiv.PDir)
            return true;

        if (PmAlign != oldDiv.PmAlign)
            return true;

        if (PxAlign != oldDiv.PxAlign)
            return true;

        if (Children is not null && oldDiv.Children is not null)
        {
            for (var i = Children.Count - 1; i >= 0; i--)
            {
                // if (Children[i].LayoutHasChanged(oldDiv.Children[i]))
                    return true;
            }
        }
     
        //Copy layout calculations
        PComputedWidth = oldDiv.PComputedWidth;
        PComputedHeight = oldDiv.PComputedHeight;
        PComputedX = oldDiv.PComputedX;
        PComputedY = oldDiv.PComputedY;
        
        return false;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void Render()
    {
        if (PBorderWidth != 0)
        {
            if (PRadius != 0)
            {
                float borderRadius = PRadius + PBorderWidth;
                
                // Program.Canvas.DrawRoundRect(PComputedX - PComputedY - PBorderWidth, PComputedWidth + 2 * PBorderWidth, PComputedHeight + 2 * PBorderWidth, borderRadius, borderRadius, BorderColor);
                Program.Canvas.DrawRoundRect(PComputedX, PComputedY, PComputedWidth, PComputedHeight, PRadius, PRadius,
                    GetColor(PColor));
            }
            else
            {
                Program.Canvas.DrawRect(PComputedX - PBorderWidth, PComputedY - PBorderWidth,
                    PComputedWidth + 2 * PBorderWidth, PComputedHeight + 2 * PBorderWidth, BorderColor);
                Program.Canvas.DrawRect(PComputedX, PComputedY, PComputedWidth, PComputedHeight,
                    GetColor(PColor));
            }
        }
        else
        {
            if (PRadius != 0)
            {
                Program.Canvas.DrawRoundRect(PComputedX, PComputedY, PComputedWidth, PComputedHeight, PRadius, PRadius,
                    GetColor(PColor));
            }
            else
            {
                Program.Canvas.DrawRect(PComputedX, PComputedY, PComputedWidth, PComputedHeight, GetColor(PColor));
            }
        }

        if (Children is not null)
        {
            foreach (var divDefinition in Children)
            {
                divDefinition.Render();
            }    
        }
    }
    
    private static readonly SKPaint s_paint = new()
    {
        IsAntialias = true
    };
    
    public static SKPaint GetColor(ColorDefinition colorDefinition)
    {
        s_paint.Color = new SKColor((byte) colorDefinition.Red, (byte)colorDefinition.Gree, (byte)colorDefinition.Blue, (byte)colorDefinition.Transparency);
        return s_paint;
    }

    public static readonly SKPaint BorderColor = new()
    {
        IsAntialias = true,
        Color = SKColors.Black
    };
    
    public Div Items(IEnumerable<RenderObject> children)
    {
        Children ??= new List<RenderObject>();
        Children.AddRange(children);
        return this;
    }

    public RenderObject Add(RenderObject child)
    {
        Children ??= new List<RenderObject>();
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
        PmAlign = mAlign;
        return this;
    }

    public Div XAlign(XAlign xAlign)
    {
        PxAlign = xAlign;
        return this;
    }

    public Div OnClick(Action callback)
    {
        POnClick = callback;
        return this;
    }

    public Div OnClick(Func<Task> callback)
    {
        POnClickAsync = callback;
        return this;
    }

    public IEnumerator<RenderObject> GetEnumerator()
    {
        return Children?.GetEnumerator() ?? Enumerable.Empty<RenderObject>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}