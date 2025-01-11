using Flamui.UiElements;

namespace Flamui;

public enum InvalidLayoutType
{
    FractionWithinShrink
}

public class InvalidLayoutException : Exception
{
    public required InvalidLayoutType InvalidLayoutType;
    public required UiElement UiElement;

    private InvalidLayoutException(string message) : base(message)
    {

    }

    public static InvalidLayoutException Create(InvalidLayoutType invalidLayoutType, UiElement uiElement)
    {
        var message = invalidLayoutType switch
        {
            InvalidLayoutType.FractionWithinShrink => "An element can not have a fraction as the size, when the parent is shrinkable on that axis",
            _ => throw new ArgumentOutOfRangeException(nameof(invalidLayoutType), invalidLayoutType, null)
        };

        return new InvalidLayoutException($"{message}: {uiElement.Id}")
        {
            InvalidLayoutType = invalidLayoutType,
            UiElement = uiElement
        };
    }
}