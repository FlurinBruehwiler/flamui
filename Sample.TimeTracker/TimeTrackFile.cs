using System.Text.Json;

namespace Sample.TimeTracker;

public class TimeTrackFile
{
    public required DateOnly Date { get; set; }
    public List<TimeTrackEntry> TimeTrackEntries { get; set; } = new();

    private (int second, string str) _cachedString = (0, "00:00:00");

    public async Task SaveAsync()
    {
        try
        {
            var fileName = Path.Combine(TimeTrackFolder.GetFolder(), Date.ToString()
                .Replace("/", "_")
                .Replace("\\", "_") + ".json");

            var str = JsonSerializer.Serialize(this);
            await File.WriteAllTextAsync(fileName, str);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public string GetTotalTimeString()
    {
        var totalTime = new TimeSpan();

        foreach (var timeTrackEntry in TimeTrackEntries)
        {
            totalTime = totalTime.Add(timeTrackEntry.GetTotalTime());
        }

        if (totalTime.Seconds != _cachedString.second)
        {
            var str = totalTime.ToString(@"hh\:mm\:ss");
            _cachedString = (totalTime.Seconds, str);
        }

        return _cachedString.str;
    }

    public override string ToString()
    {
        return S(Date, x => x.ToString());
    }

    public bool IsCurrentDay()
    {
        return Date == DateOnly.FromDateTime(DateTime.Now);
    }
}
