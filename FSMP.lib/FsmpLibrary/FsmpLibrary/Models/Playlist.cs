namespace FsmpLibrary.Models;

/// <summary>
/// Represents a user-created playlist containing an ordered collection of tracks.
/// </summary>
public class Playlist
{
    public int PlaylistId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();
}
