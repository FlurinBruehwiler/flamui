using ShimSkiaSharp;
using Svg.Skia;

namespace Demo.Test;

public class Renderer
{
    public void Render(DivDefinition div)
    {
        if (div.BorderWidth != 0)
        {
            if (div.Radius != 0)
            {
                var borderRadius = div.Radius + div.BorderWidth;
                
                Program.Canvas.DrawRoundRect(div.ComputedX - div.BorderWidth, div.ComputedY - div.BorderWidth,
                    div.ComputedWidth + 2 * div.BorderWidth, div.ComputedHeight + 2 * div.BorderWidth, borderRadius, borderRadius, div.BorderColor);
                Program.Canvas.DrawRoundRect(div.ComputedX, div.ComputedY, div.ComputedWidth, div.ComputedHeight, div.Radius, div.Radius,
                    div.Color);
            }
            else
            {
                Program.Canvas.DrawRect(div.ComputedX - div.BorderWidth, div.ComputedY - div.BorderWidth,
                    div.ComputedWidth + 2 * div.BorderWidth, div.ComputedHeight + 2 * div.BorderWidth, div.BorderColor);
                Program.Canvas.DrawRect(div.ComputedX, div.ComputedY, div.ComputedWidth, div.ComputedHeight,
                    div.Color);
            }
        }
        else
        {
            if (div.Radius != 0)
            {
                Program.Canvas.DrawRoundRect(div.ComputedX, div.ComputedY, div.ComputedWidth, div.ComputedHeight, div.Radius, div.Radius,
                    div.Color);
            }
            else
            {
                Program.Canvas.DrawRect(div.ComputedX, div.ComputedY, div.ComputedWidth, div.ComputedHeight, div.Color);
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