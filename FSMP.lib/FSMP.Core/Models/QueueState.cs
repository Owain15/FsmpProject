namespace FSMP.Core.Models;

public class QueueState
{
    public List<int> OriginalOrder { get; set; } = new();
    public List<int> PlayOrder { get; set; } = new();
    public int CurrentIndex { get; set; } = -1;
    public RepeatMode RepeatMode { get; set; } = RepeatMode.None;
    public bool IsShuffled { get; set; }
}
