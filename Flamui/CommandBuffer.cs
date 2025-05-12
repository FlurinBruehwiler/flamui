namespace Flamui;

public struct CommandBuffer
{
    public Dictionary<int, ArenaChunkedList<Command>> InnerBuffers;

    public bool IsEqualTo(CommandBuffer otherBuffer)
    {
        if (otherBuffer.InnerBuffers == null)
            return false;

        if (this.InnerBuffers.Count != otherBuffer.InnerBuffers.Count)
        {
            return false;
        }

        foreach (var (key, commandsA) in this.InnerBuffers)
        {
            if (!otherBuffer.InnerBuffers.TryGetValue(key, out var commandsB))
                return false;

            if (!ArenaChunkedList<Command>.CompareGrowableArenaBuffers(commandsA, commandsB))
                return false;
        }

        return true;
    }
}