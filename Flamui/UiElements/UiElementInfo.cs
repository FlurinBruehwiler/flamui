namespace Flamui.UiElements;

public struct UiElementInfo
{
    public Quadrant Margin;
    public AbsoluteInfo? AbsoluteInfo;
}

public struct AbsoluteInfo
{
    public FlexContainer? Anchor;
    public AbsolutePosition Position;
    public RelativeSize Size;
}
