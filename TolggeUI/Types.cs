namespace TolggeUI;

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

public record TolggeSize(int Value, SizeKind SizeKind);
