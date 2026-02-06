namespace FsmpLibrary.Services;

/// <summary>
/// Result of a library scan operation, summarising what was imported or changed.
/// </summary>
public class ScanResult
{
    public int TracksAdded { get; set; }
    public int TracksUpdated { get; set; }
    public int TracksRemoved { get; set; }
    public TimeSpan Duration { get; set; }
    public List<string> Errors { get; set; } = new();
}
