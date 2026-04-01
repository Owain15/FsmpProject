namespace FSMP.Core.Models;

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
    public List<DirectoryScanResult> DirectoryResults { get; set; } = new();
}

public class DirectoryScanResult
{
    public string Path { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int TracksAdded { get; set; }
    public int TracksUpdated { get; set; }
    public int TracksRemoved { get; set; }
    public string? ErrorMessage { get; set; }
}
