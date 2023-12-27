using System.Numerics;
using Flamui.UiElements;
using SkiaSharp;

namespace Flamui;

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

    public override void Render(RenderContext renderContext)
    {
        renderContext.Add(new Matrix
        {
            SkMatrix = CameraInfo.GetCameraMatrix()
        });

        foreach (var uiElement in Children)
        {
            uiElement.Render(renderContext);
        }

        renderContext.Add(new ResetMatrix());
    }

    public override void Layout(UiWindow uiWindow)
    {
        foreach (var uiElement in Children)
        {
            //todo big refactor needed, because wtf
            if (uiElement.PWidth.Kind == SizeKind.Pixel)
            {
                uiElement.ComputedBounds.W = uiElement.PWidth.Value;
            }
            if (uiElement.PHeight.Kind == SizeKind.Pixel)
            {
                uiElement.ComputedBounds.H = uiElement.PHeight.Value;
            }
            uiElement.Layout(uiWindow);
        }
    }

    public override Vector2 ProjectPoint(Vector2 point)
    {
        return CameraInfo.ScreenToWorld(point);
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
