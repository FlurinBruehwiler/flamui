namespace Flamui;

public class DispatcherPriorityQueue
{
    private readonly Queue<DispatcherOperation> _queue = new();

    public void Enqueue(DispatcherOperation dispatcherOperation)
    {
        _queue.Enqueue(dispatcherOperation);
    }

    public void RunPendingTasks()
    {
        while (_queue.Count > 0)
        {
            var operation = _queue.Dequeue();
            operation.Execute();
        }
    }
}
