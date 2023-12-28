namespace Flamui;

public class FlamuiSynchronizationContext : SynchronizationContext
{
    public static void Install()
    {
        if (Current is FlamuiSynchronizationContext)
            return;

        SetSynchronizationContext(new FlamuiSynchronizationContext());
    }

    //non blocking, i.e. return immediately
    public override void Post(SendOrPostCallback d, object? state)
    {
        Dispatcher.UIThread.InvokeAsync(() => d(state));
    }

    //blocking, i.e. returns only when the delegate has finished
    public override void Send(SendOrPostCallback d, object? state)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            d(state);
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(() => d(state)).GetAwaiter().GetResult(); //block
        }
    }
}
