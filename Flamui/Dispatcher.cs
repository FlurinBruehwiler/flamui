namespace Flamui;

public class Dispatcher
{
    public static Dispatcher UIThread { get; set; }

    public readonly DispatcherPriorityQueue Queue = new();
    private readonly Thread _thread;

    public Dispatcher(Thread thread)
    {
        _thread = thread;
    }

    public DispatcherOperation InvokeAsync(Action callback)
    {
        var operation = new DispatcherOperation(callback);
        Queue.Enqueue(operation);
        return operation;
    }

    public bool CheckAccess()
    {
        return _thread == Thread.CurrentThread;
    }
}
