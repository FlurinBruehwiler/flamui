using System.Numerics;
using ImSharpUISample.UiElements;
using SkiaSharp;

namespace ImSharpUISample;

public record struct CameraInfo(Vector2 Offset, Vector2 Target, float Zoom)
{
    public Vector2 ScreenToWorld(Vector2 screenVector)
    {
        var invertedCameraMatrix = GetCameraMatrix().Invert();
        var worldPoint = invertedCameraMatrix.MapPoint(screenVector.X, screenVector.Y);
        return new Vector2(worldPoint.X, worldPoint.Y);
    }

    public SKMatrix GetCameraMatrix()
    {
        var matOrigin = SKMatrix.CreateTranslation(-Target.X, -Target.Y);
        var matScale = SKMatrix.CreateScale(Zoom, Zoom);
        var matTranslation = SKMatrix.CreateTranslation(Offset.X, Offset.Y);

        return SKMatrix.Concat(SKMatrix.Concat(matTranslation, matScale), matOrigin);
    }
};

public record struct Data(object Obj, UiElementId Id);


public class Camera : UiElementContainer
{
    public Camera Info(CameraInfo cameraInfo)
    {
        CameraInfo = cameraInfo;
        return this;
    }
    public CameraInfo CameraInfo { get; set; }

    private SKPaint Paint = new()
    {
        Color = SKColors.Red
    };

    public override void Render(SKCanvas canvas)
    {
        canvas.SetMatrix(CameraInfo.GetCameraMatrix());

        foreach (var uiElement in Children)
        {
            uiElement.Render(canvas);
        }
        canvas.DrawRect(CameraInfo.Target.X, CameraInfo.Target.Y, 10, 10, Paint);
        canvas.ResetMatrix();
    }

    public override void Layout(Window window)
    {
        foreach (var uiElement in Children)
        {
            //todo big refactor needed, because wtf
            if (uiElement.PWidth.Kind == SizeKind.Pixel)
            {
                uiElement.PComputedWidth = uiElement.PWidth.Value;
            }
            if (uiElement.PHeight.Kind == SizeKind.Pixel)
            {
                uiElement.PComputedHeight = uiElement.PHeight.Value;
            }
            uiElement.Layout(window);
        }
    }

    public override bool LayoutHasChanged()
    {
        throw new NotImplementedException();
    }

    public override bool HasChanges()
    {
        throw new NotImplementedException();
    }
}
