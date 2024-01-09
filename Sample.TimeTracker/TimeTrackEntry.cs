using System.Text.Json.Serialization;

namespace Sample.TimeTracker;

public class TimeTrackEntry(string name)
{
    public string Name { get; set; } = name;

    [JsonIgnore]
    public DateTime? ActiveSince { get; set; }
    public TimeSpan SpentTime { get; set; } = new();

    private (int second, string str) _cachedString = (0, "00:00:00");

    public void Deactivate()
    {
        SpentTime = DateTime.Now - ActiveSince!.Value;
        ActiveSince = null;
    }

    public string GetTotalTimeAsString()
    {
        var totalTime = GetTotalTime();
        if (totalTime.Seconds != _cachedString.second)
        {
            var str = totalTime.ToString(@"hh\:mm\:ss");
            _cachedString = (totalTime.Seconds, str);
        }

        return _cachedString.str;
    }

    public TimeSpan GetTotalTime()
    {
        if (ActiveSince is { } activeSince)
        {
            return SpentTime.Add(DateTime.Now - activeSince);
        }

        return SpentTime;
    }
}
