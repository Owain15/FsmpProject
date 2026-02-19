namespace FSMP.Core.Models;

/// <summary>
/// Lookup entity for music tags. Add new tags by inserting a row — no code change required.
/// </summary>
public class Tags
{
    public int TagId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation properties (many-to-many back-references)
    public ICollection<Album> Albums { get; set; } = new List<Album>();
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
    public ICollection<Artist> Artists { get; set; } = new List<Artist>();
}
