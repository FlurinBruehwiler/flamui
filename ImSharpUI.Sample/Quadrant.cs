namespace ImSharpUISample;

public record struct Quadrant(int Left, int Right, int Top, int Bottom);

public enum SizeKind
{
    Percentage,
    Pixel
}

public struct SizeDefinition
{
    public SizeDefinition(float value, SizeKind kind)
    {
        Value = value;
        Kind = kind;
    }

    public float GetDpiAwareValue()
    {
        if (Kind == SizeKind.Percentage)
            return Value;

        return Value;//Todo
    }

    public float Value { get; set; }
    public SizeKind Kind { get; set; }
}

public enum MAlign
{
    FlexStart,
    FlexEnd,
    Center,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly
}

public enum Dir
{
    Horizontal,
    RowReverse,
    Vertical,
    ColumnReverse
}

public enum XAlign
{
    FlexStart,
    FlexEnd,
    Center
}

public record struct ColorDefinition(float Red, float Green, float Blue, float Appha = 255);

