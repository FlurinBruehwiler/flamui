namespace Flamui.UiElements;

public struct UiElementInfo
{
    public Quadrant Margin;
    public AbsoluteInfo? AbsoluteInfo;
    public string DebugTag;
}

public struct AbsoluteInfo
{
    public AbsolutePosition Position;
    public RelativeSize Size;
}
