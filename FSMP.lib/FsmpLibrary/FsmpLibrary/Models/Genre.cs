namespace FsmpLibrary.Models;

/// <summary>
/// Lookup entity for music genres. Add new genres by inserting a row — no code change required.
/// </summary>
public class Genre
{
    public int GenreId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation properties (many-to-many back-references)
    public ICollection<Album> Albums { get; set; } = new List<Album>();
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
    public ICollection<Artist> Artists { get; set; } = new List<Artist>();
}
