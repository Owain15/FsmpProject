namespace FsmpLibrary.Models;

/// <summary>
/// Represents a music artist or performer.
/// </summary>
public class Artist
{
    public int ArtistId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SortName { get; set; }
    public string? Biography { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Album> Albums { get; set; } = new List<Album>();
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}
