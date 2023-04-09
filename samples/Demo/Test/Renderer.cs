using System.Diagnostics;
using SkiaSharp;

namespace Demo.Test;

public class Renderer
{
    private static readonly SKPaint s_paint = new()
    {
        IsAntialias = true
    };

    public static readonly SKPaint BorderColor = new()
    {
        IsAntialias = true,
        Color = SKColors.Black
    };
    
    public static SKPaint GetColor(ColorDefinition colorDefinition)
    {
        s_paint.Color = new SKColor((byte) colorDefinition.Red, (byte)colorDefinition.Gree, (byte)colorDefinition.Blue, (byte)colorDefinition.Transparency);
        return s_paint;
    }

    private LayoutEngine _layoutEngine = new LayoutEngine();

    private DivDefinition _rootDivDefinition = new DivDefinition();

    private Div root;
    
    public void DoSomething(UiComponent uiroot)
    {
        if (LayoutEngine.IsFirstRender)
        {
            root = uiroot.Render();
        }
        
        var actualRoot = new Div();
        actualRoot.Width(Program.ImageInfo.Width);
        actualRoot.Height(Program.ImageInfo.Height);
        actualRoot.Add(root);

        _rootDivDefinition.ComputedHeight = Program.ImageInfo.Height;
        _rootDivDefinition.ComputedWidth = Program.ImageInfo.Width;
        
        var rootDefinition = _layoutEngine.CalculateIfNecessary(actualRoot, _rootDivDefinition);

        var stopwatch = Stopwatch.StartNew();
        Render(rootDefinition);

        var time = stopwatch.ElapsedTicks;
        Program.draw = time;
    }
    
    private void Render(DivDefinition div)
    {
        if (div.BorderWidth != 0)
        {
            if (div.Radius != 0)
            {
                var borderRadius = div.Radius + div.BorderWidth;
                
                Program.Canvas.DrawRoundRect(div.ComputedX - div.BorderWidth, div.ComputedY - div.BorderWidth,
                    div.ComputedWidth + 2 * div.BorderWidth, div.ComputedHeight + 2 * div.BorderWidth, borderRadius, borderRadius, BorderColor);
                Program.Canvas.DrawRoundRect(div.ComputedX, div.ComputedY, div.ComputedWidth, div.ComputedHeight, div.Radius, div.Radius,
                    GetColor(div.Color));
            }
            else
            {
                Program.Canvas.DrawRect(div.ComputedX - div.BorderWidth, div.ComputedY - div.BorderWidth,
                    div.ComputedWidth + 2 * div.BorderWidth, div.ComputedHeight + 2 * div.BorderWidth, BorderColor);
                Program.Canvas.DrawRect(div.ComputedX, div.ComputedY, div.ComputedWidth, div.ComputedHeight,
                    GetColor(div.Color));
            }
        }
        else
        {
            if (div.Radius != 0)
            {
                Program.Canvas.DrawRoundRect(div.ComputedX, div.ComputedY, div.ComputedWidth, div.ComputedHeight, div.Radius, div.Radius,
                    GetColor(div.Color));
            }
            else
            {
                Program.Canvas.DrawRect(div.ComputedX, div.ComputedY, div.ComputedWidth, div.ComputedHeight, GetColor(div.Color));
            }
        }

        foreach (var divDefinition in div.Children)
        {
            Render(divDefinition);
        }

        // if (div.Text != string.Empty)
        // {
        //     var paint = new SKPaint
        //     {
        //         Color = new SKColor(0, 0, 0),
        //         IsAntialias = true,
        //         TextSize = 15,
        //         Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
        //     };
        //
        //     var path = paint.GetTextPath(Text, ComputedX, ComputedY);
        //     path.GetBounds(out var rect);
        //
        //     var verticalCenter = ComputedY + ComputedHeight / 2;
        //     
        //     Program.Canvas.DrawText(Text, ComputedX + Padding, verticalCenter + rect.Height / 2, paint);
        // }

        // if (Svg != string.Empty)
        // {
        //     var svg = new SKSvg();
        //     svg.Load("./battery.svg");
        //     Program.Canvas.DrawPicture(svg.Picture, ComputedX, ComputedY);
        // }
    }
}