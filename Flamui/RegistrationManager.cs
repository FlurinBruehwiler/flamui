namespace Flamui;

public class RegistrationManager
{
    public List<Action<UiTree>> OnAfterInput { get; set; } = new();
}
