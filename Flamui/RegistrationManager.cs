namespace Flamui;

public class RegistrationManager
{
    public List<Action<UiWindow>> OnAfterInput { get; set; } = new();
}
