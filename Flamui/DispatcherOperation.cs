using System.Runtime.CompilerServices;

namespace Flamui;

public class DispatcherOperation
{
    private readonly Action _callback;
    public TaskCompletionSource<object> TaskSource { get; set; }

    public DispatcherOperation(Action callback)
    {
        _callback = callback;
        TaskSource = new TaskCompletionSource<object>();
    }

    public void Execute()
    {
        try
        {
            _callback();
            TaskSource.SetResult(null);
        }
        catch (Exception e)
        {
            // TaskSource.SetException(e);
            throw;
        }
    }

    public TaskAwaiter GetAwaiter()
    {
        return GetTask().GetAwaiter();
    }

    private Task GetTask()
    {
        return TaskSource.Task;
    }
}
