using System.Numerics;
using Flamui.Layouting;
using Silk.NET.Maths;

namespace Flamui.UiElements;

public static class FlexContainerRenderer
{
    private static Matrix4X4<float> GetRotationMatrix(FlexContainer flexContainer, Point offset)
    {
        Vector2 rotationOffset = flexContainer.Info.RotationPivot switch
        {
            RotationPivot.Center => new Vector2(flexContainer.Rect.Width / 2, flexContainer.Rect.Height / 2),
            RotationPivot.TopLeft => new Vector2(0, 0),
            RotationPivot.TopRight => new Vector2(flexContainer.Rect.Width, 0),
            RotationPivot.BottomLeft => new Vector2(0, flexContainer.Rect.Height),
            RotationPivot.BottomRight => new Vector2(flexContainer.Rect.Width, flexContainer.Rect.Height),
            _ => throw new ArgumentOutOfRangeException()
        };

        return Matrix4X4<float>.Identity;
        //todo
        // return Matrix4X4 CreateRotationDegrees(flexContainer.Info.Rotation, offset.X + rotationOffset.X,
        //     offset.Y + rotationOffset.Y);
    }

    public static void Render(RenderContext renderContext, FlexContainer flexContainer, Point offset)
    {
        if (flexContainer.Info.ZIndex != 0)
        {
            renderContext.SetIndex(flexContainer.Info.ZIndex);
        }

        if (flexContainer.Info.Rotation != 0)
        {
            renderContext.PushMatrix(GetRotationMatrix(flexContainer, offset));
        }

        if (flexContainer.Info.ClipToIgnore is not null)
        {
            // renderContext.Add(new Restore());//todo
        }

        if (flexContainer.Info.BorderWidth != 0 && flexContainer.Info.BorderColor is {} borderColor)
        {
            float borderRadius = flexContainer.Info.Radius + flexContainer.Info.BorderWidth;

            var bounds = new Bounds
            {
                X = offset.X - flexContainer.Info.BorderWidth,
                Y = offset.Y - flexContainer.Info.BorderWidth,
                W = flexContainer.Rect.Width + 2 * flexContainer.Info.BorderWidth,
                H = flexContainer.Rect.Height + 2 * flexContainer.Info.BorderWidth,
            };
            renderContext.AddRect(bounds, flexContainer, borderColor, borderRadius);
        }

        if (flexContainer.Info.Color is { } color)
        {
            // //shadow
            // if (flexContainer.Info.PShadowColor is { } blurColor)
            // {
            //     float borderRadius = flexContainer.Info.Radius + flexContainer.Info.BorderWidth;
            //
            //     //todo replace with readable code or something
            //     var bounds = new Bounds
            //     {
            //         X = offset.X - flexContainer.Info.BorderWidth + flexContainer.Info.ShadowOffset.Left,
            //         Y = offset.Y - flexContainer.Info.BorderWidth + flexContainer.Info.ShadowOffset.Top,
            //         H = flexContainer.Rect.Height + 2 * flexContainer.Info.BorderWidth -
            //             flexContainer.Info.ShadowOffset.Top - flexContainer.Info.ShadowOffset.Bottom,
            //         W = flexContainer.Rect.Width + 2 * flexContainer.Info.BorderWidth -
            //             flexContainer.Info.ShadowOffset.Left - flexContainer.Info.ShadowOffset.Right,
            //     };
            //
            //     //todo shadowsigma from flexcontainer.Info
            //     renderContext.AddRect(bounds, flexContainer, blurColor, flexContainer.Info.Radius == 0 ? 0 : borderRadius);
            // }

            renderContext.AddRect(flexContainer.Rect.ToBounds(offset), flexContainer, color, flexContainer.Info.Radius);
        }


        if (NeedsClip(flexContainer.Info))
        {
            renderContext.PushClip(flexContainer.Rect.ToBounds(offset), flexContainer.Info.Radius);
        }

        if (flexContainer.Info.ScrollConfigY.CanScroll || flexContainer.Info.ScrollConfigX.CanScroll)
        {
            renderContext.PushMatrix(Matrix4X4.CreateTranslation(-flexContainer.ScrollPosX, -flexContainer.ScrollPosY, 0));
        }

        foreach (var childElement in flexContainer.Children)
        {
            if (childElement is FlexContainer { Info.Hidden: true })
            {
                continue;
            }

            childElement.Render(renderContext, offset.Add(childElement.ParentData.Position));
        }

        if (flexContainer.Info.ScrollConfigY.CanScroll || flexContainer.Info.ScrollConfigX.CanScroll)
        {
            renderContext.PopMatrix();

            flexContainer._scrollBarContainerX.UiElement?.Render(renderContext, offset.Add(flexContainer._scrollBarContainerX.UiElement.ParentData.Position));
            flexContainer._scrollBarContainerY.UiElement?.Render(renderContext, offset.Add(flexContainer._scrollBarContainerY.UiElement.ParentData.Position));
        }

        if (NeedsClip(flexContainer.Info))
        {
            renderContext.PopClip();
        }

        if (flexContainer.Info.Rotation != 0)
        {
            renderContext.PopMatrix();
        }

        //reapply clip
        // uiElement.Info.ClipToIgnore?.ClipContent(renderContext);

        if (flexContainer.Info.ZIndex != 0)
        {
            renderContext.RestoreZIndex();
        }
    }

    private static bool NeedsClip(FlexContainerInfo Info)
    {
        return Info.ScrollConfigY.CanScroll || Info.ScrollConfigX.CanScroll || Info.IsClipped;
    }
}
