namespace FsmpLibrary.Models;

/// <summary>
/// Join entity representing a track's membership and position within a playlist.
/// </summary>
public class PlaylistTrack
{
    public int PlaylistTrackId { get; set; }
    public int PlaylistId { get; set; }
    public int TrackId { get; set; }
    public int Position { get; set; }
    public DateTime AddedAt { get; set; }

    // Navigation properties
    public Playlist? Playlist { get; set; }
    public Track? Track { get; set; }
}
