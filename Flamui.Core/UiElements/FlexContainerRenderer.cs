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

        //this is not technically correct, you can still have a shadow with 0 blur and spread!!!!, should probably introduce a separate property
        if (flexContainer.Info.ShadowBlur != 0 || flexContainer.Info.ShadowSpread != 0)
        {
            var bounds = new Bounds
            {
                X = offset.X,
                Y = offset.Y,
                H = flexContainer.Rect.Height,
                W = flexContainer.Rect.Width
            }
                .OffsetBy(flexContainer.Info.ShadowOffset)
                .ExpandBy(new Vector2(flexContainer.Info.ShadowSpread))
                .ExpandBy(new Vector2(flexContainer.Info.ShadowBlur)); //the blur also outgrows the bounds... but maybe we can also make this smaller

            renderContext.AddRect(bounds, flexContainer, flexContainer.Info.ShadowColor, flexContainer.Info.Radius + flexContainer.Info.ShadowBlur, 0, 0, flexContainer.Info.ShadowBlur);
        }

        if (flexContainer.Info.Color != null || flexContainer.Info.BlurRadius != 0)
        {
            renderContext.AddRect(flexContainer.Rect.ToBounds(offset), flexContainer, flexContainer.Info.Color ?? new ColorDefinition(), flexContainer.Info.Radius, blurRadius: flexContainer.Info.BlurRadius);
        }
        else if (flexContainer.Info.Interactable)
        {
            //hack, pls fix
            renderContext.AddRect(flexContainer.Rect.ToBounds(offset), flexContainer, C.Transparent, 0);
        }

        if (flexContainer.Info.BorderWidth != 0) //render border after
        {
            float borderRadius = flexContainer.Info.Radius == 0 ? 0 : flexContainer.Info.Radius + flexContainer.Info.BorderWidth;

            //renderContext.PushClip(flexContainer.Rect.ToBounds(offset), ClipMode.OnlyDrawOutside, flexContainer.Info.Radius);

            var bounds = new Bounds
            {
                X = offset.X - flexContainer.Info.BorderWidth,
                Y = offset.Y - flexContainer.Info.BorderWidth,
                W = flexContainer.Rect.Width + 2 * flexContainer.Info.BorderWidth,
                H = flexContainer.Rect.Height + 2 * flexContainer.Info.BorderWidth,
            };
            renderContext.AddRect(bounds, flexContainer, flexContainer.Info.BorderColor, borderRadius,
                blurRadius:0, borderWidth: flexContainer.Info.BorderWidth);
        }


        if (NeedsClip(flexContainer.Info))
        {
            renderContext.PushClip(flexContainer.Rect.ToBounds(offset), ClipMode.OnlyDrawWithin, flexContainer.Info.Radius);
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

            flexContainer.ScrollXElement?.GetElement()?.Render(renderContext, offset.Add(flexContainer.ScrollXElement.GetElement()!.ParentData.Position));
            flexContainer.ScrollYElement?.GetElement()?.Render(renderContext, offset.Add(flexContainer.ScrollYElement.GetElement()!.ParentData.Position));
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