namespace FSMP.Core.Models;

/// <summary>
/// Represents a music album containing multiple tracks.
/// </summary>
public class Album
{
    public int AlbumId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string? AlbumArtistName { get; set; }
    public int? ArtistId { get; set; }
    public byte[]? AlbumArt { get; set; }
    public string? AlbumArtPath { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Artist? Artist { get; set; }
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
    public ICollection<Tags> Tags { get; set; } = new List<Tags>();
}