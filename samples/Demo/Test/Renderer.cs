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

    private LayoutEngine _layoutEngine = new();

    private readonly Div _oldRoot = new();
    
    public Div _newRoot = null!;

    private Div? _clickedElement;

    public void Build(UiComponent rootComponent)
    {
        _newRoot = rootComponent.Render();
    }
    
    public void LayoutPaintComposite()
    {
        var wrapper = new Div
        {
            _newRoot
        }.Width(Program.ImageInfo.Width).Height(Program.ImageInfo.Height);
        wrapper.PComputedHeight = Program.ImageInfo.Height;
        wrapper.PComputedWidth = Program.ImageInfo.Width;

        _layoutEngine.ApplyLayoutCalculations(wrapper, _oldRoot);
        
        var stopwatch = Stopwatch.StartNew();
        
        Render(wrapper);
        
        var time = stopwatch.ElapsedTicks;
        Program.draw = time;

        _clickedElement?.POnClick?.Invoke();
        _clickedElement = null;
    }
    
    private void Render(Div div)
    {
        if (div.PBorderWidth != 0)
        {
            if (div.PRadius != 0)
            {
                var borderRadius = div.PRadius + div.PBorderWidth;
                
                Program.Canvas.DrawRoundRect(div.PComputedX - div.PBorderWidth, div.PComputedY - div.PBorderWidth,
                    div.PComputedWidth + 2 * div.PBorderWidth, div.PComputedHeight + 2 * div.PBorderWidth, borderRadius, borderRadius, BorderColor);
                Program.Canvas.DrawRoundRect(div.PComputedX, div.PComputedY, div.PComputedWidth, div.PComputedHeight, div.PRadius, div.PRadius,
                    GetColor(div.PColor));
            }
            else
            {
                Program.Canvas.DrawRect(div.PComputedX - div.PBorderWidth, div.PComputedY - div.PBorderWidth,
                    div.PComputedWidth + 2 * div.PBorderWidth, div.PComputedHeight + 2 * div.PBorderWidth, BorderColor);
                Program.Canvas.DrawRect(div.PComputedX, div.PComputedY, div.PComputedWidth, div.PComputedHeight,
                    GetColor(div.PColor));
            }
        }
        else
        {
            if (div.PRadius != 0)
            {
                Program.Canvas.DrawRoundRect(div.PComputedX, div.PComputedY, div.PComputedWidth, div.PComputedHeight, div.PRadius, div.PRadius,
                    GetColor(div.PColor));
            }
            else
            {
                if (Program.ClickPos != new Point(-1, -1))
                {
                    if (Program.ClickPos.X > div.PComputedX && Program.ClickPos.X < div.PComputedWidth &&
                        Program.ClickPos.Y > div.PComputedY && Program.ClickPos.Y < div.PComputedHeight)
                    {
                        // Program.click++;
                        _clickedElement = div;
                    }
                }
                
                Program.Canvas.DrawRect(div.PComputedX, div.PComputedY, div.PComputedWidth, div.PComputedHeight, GetColor(div.PColor));
            }
        }

        if (div.Children is not null)
        {
            foreach (var divDefinition in div.Children)
            {
                Render(divDefinition);
            }    
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