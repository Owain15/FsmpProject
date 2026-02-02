namespace FsmpLibrary.Models;

/// <summary>
/// Represents a record of a track being played.
/// </summary>
public class PlaybackHistory
{
    public int PlaybackHistoryId { get; set; }
    public int TrackId { get; set; }
    public DateTime PlayedAt { get; set; }
    public TimeSpan? PlayDuration { get; set; }
    public bool CompletedPlayback { get; set; }
    public bool WasSkipped { get; set; }

    // Navigation property
    public Track Track { get; set; } = null!;
}
