namespace FsmpLibrary.Models;

/// <summary>
/// Represents a configured library path where music files are stored.
/// </summary>
public class LibraryPath
{
    public int LibraryPathId { get; set; }
    public string Path { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime AddedAt { get; set; }
    public DateTime? LastScannedAt { get; set; }
    public int TrackCount { get; set; }
}
