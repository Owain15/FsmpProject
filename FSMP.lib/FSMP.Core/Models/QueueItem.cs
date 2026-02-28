namespace FSMP.Core.Models;

public class QueueItem
{
    public int Index { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Artist { get; init; } = string.Empty;
    public TimeSpan? Duration { get; init; }
    public bool IsCurrent { get; init; }
}
