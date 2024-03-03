using Flamui.UiElements;

namespace Flamui;

public class DataStore
{
    private Dictionary<UiID, object>? _oldDataById;
    private  Dictionary<UiID, object>? _data;
    public Dictionary<UiID, object> OldDataById => _oldDataById ??= new Dictionary<UiID, object>();
    public Dictionary<UiID, object> Data => _data ??= new  Dictionary<UiID, object>();//maybe change do dictionary, but maybe this is slower, should benchmark it

    public void Reset()
    {
        OldDataById.Clear();
        foreach (var o in Data)
        {
            OldDataById.Add(o.Key, o.Value);
        }

        Data.Clear();
    }
}
