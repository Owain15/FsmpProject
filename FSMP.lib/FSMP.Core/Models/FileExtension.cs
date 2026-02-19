namespace FSMP.Core.Models;

/// <summary>
/// Lookup entity for supported audio file extensions. Seed values: wav, wma, mp3.
/// </summary>
public class FileExtension
{
    public int FileExtensionId { get; set; }
    public string Extension { get; set; } = string.Empty;

    // Navigation property (one-to-many back-reference)
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}