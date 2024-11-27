
using Flamui.Layouting;
using SkiaSharp;

namespace Flamui.UiElements;

public static class FlexContainerRenderer
{
    public static void Render(RenderContext renderContext, FlexContainer flexContainer, Point offset)
    {
        if (flexContainer.Info.ZIndex != 0)
        {
            renderContext.SetIndex(flexContainer.Info.ZIndex);
        }

        if (flexContainer.Info.Rotation != 0)
        {
            renderContext.Add(new Matrix
            {
                SkMatrix = SKMatrix.CreateRotationDegrees(flexContainer.Info.Rotation, offset.X + flexContainer.Rect.Width / 2, offset.Y + flexContainer.Rect.Height / 2)
            });
        }

        if (flexContainer.Info.ClipToIgnore is not null)
        {
            renderContext.Add(new Restore());
        }

        if (flexContainer.Info.Color is { } color)
        {
            //shadow
            if (flexContainer.Info.PShadowColor is { } blurColor)
            {
                float borderRadius = flexContainer.Info.Radius + flexContainer.Info.BorderWidth;

                //todo replace with readable code or something
                renderContext.Add(new Rect
                {
                    UiElement = flexContainer,
                    Bounds = new Bounds
                    {
                        X = offset.X - flexContainer.Info.BorderWidth + flexContainer.Info.ShadowOffset.Left,
                        Y = offset.Y - flexContainer.Info.BorderWidth + flexContainer.Info.ShadowOffset.Top,
                        H = flexContainer.Rect.Height + 2 * flexContainer.Info.BorderWidth - flexContainer.Info.ShadowOffset.Top - flexContainer.Info.ShadowOffset.Bottom,
                        W = flexContainer.Rect.Width + 2 * flexContainer.Info.BorderWidth - flexContainer.Info.ShadowOffset.Left - flexContainer.Info.ShadowOffset.Right,
                    },
                    Radius = flexContainer.Info.Radius == 0 ? 0 : borderRadius,
                    RenderPaint = new ShadowPaint
                    {
                        ShadowSigma = flexContainer.Info.ShadowSigma,
                        SkColor = blurColor.ToSkColor()
                    }
                });
            }

            renderContext.Add(new Rect
            {
                UiElement = flexContainer,
                Bounds = flexContainer.Rect.ToBounds(offset),
                Radius = flexContainer.Info.Radius,
                RenderPaint = new PlaintPaint
                {
                    SkColor = color.ToSkColor()
                }
            });
        }

        if (flexContainer.Info.BorderWidth != 0 && flexContainer.Info.BorderColor is {} borderColor)
        {
            renderContext.Add(new Save());

            float borderRadius = flexContainer.Info.Radius + flexContainer.Info.BorderWidth;

            renderContext.Add(new RectClip
            {
                Bounds = flexContainer.Rect.ToBounds(offset),
                Radius = flexContainer.Info.Radius,
                ClipOperation = SKClipOperation.Difference
            });

            renderContext.Add(new Rect
            {
                UiElement = flexContainer,
                Bounds = new Bounds
                {
                    X = offset.X - flexContainer.Info.BorderWidth,
                    Y = offset.Y - flexContainer.Info.BorderWidth,
                    W = flexContainer.Rect.Width + 2 * flexContainer.Info.BorderWidth,
                    H = flexContainer.Rect.Height + 2 * flexContainer.Info.BorderWidth,
                },
                Radius = borderRadius,
                RenderPaint = new PlaintPaint
                {
                    SkColor = borderColor.ToSkColor()
                }
            });

            renderContext.Add(new Restore());
        }

        ClipContent(renderContext, flexContainer, flexContainer.Info, offset);

        if (flexContainer.Info.ScrollConfigY.CanScroll || flexContainer.Info.ScrollConfigX.CanScroll)
        {
            renderContext.Add(new Matrix
            {
                SkMatrix = SKMatrix.CreateTranslation(-flexContainer.ScrollPosX, -flexContainer.ScrollPosY)
            });
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
            renderContext.Add(new Matrix
            {
                SkMatrix = SKMatrix.CreateTranslation(flexContainer.ScrollPosX, flexContainer.ScrollPosY)
            });

            flexContainer._scrollBarContainer.UiElement?.Render(renderContext, offset.Add(flexContainer._scrollBarContainer.UiElement.ParentData.Position));
        }

        if (NeedsClip(flexContainer.Info))
        {
            renderContext.Add(new Restore());
        }

        if (flexContainer.Info.Rotation != 0)
        {
            renderContext.Add(new Matrix
            {
                SkMatrix = SKMatrix.CreateRotationDegrees(-flexContainer.Info.Rotation, offset.X + flexContainer.Rect.Width / 2, offset.Y + flexContainer.Rect.Height / 2)
            });
        }

        //reapply clip
        // uiElement.Info.ClipToIgnore?.ClipContent(renderContext);

        if (flexContainer.Info.ZIndex != 0)
        {
            renderContext.RestoreZIndex();
        }
    }

    private static void ClipContent(RenderContext renderContext, UiElement uiElement, FlexContainerInfo Info, Point offset)
    {
        if (NeedsClip(Info))

        {
            renderContext.Add(new Save());

            renderContext.Add(new RectClip
            {
                Bounds = uiElement.Rect.ToBounds(offset),
                Radius = Info.Radius,
                ClipOperation = SKClipOperation.Intersect
            });
        }
    }

    private static bool NeedsClip(FlexContainerInfo Info)
    {
        return Info.ScrollConfigY.CanScroll || Info.ScrollConfigX.CanScroll || Info.IsClipped;
    }
}
