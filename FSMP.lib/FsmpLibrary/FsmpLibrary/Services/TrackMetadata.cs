namespace FsmpLibrary.Services;

/// <summary>
/// Plain data object holding metadata extracted from an audio file.
/// </summary>
public class TrackMetadata
{
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public int? Year { get; set; }
    public string? Genre { get; set; }
    public TimeSpan? Duration { get; set; }
    public int? BitRate { get; set; }
    public int? SampleRate { get; set; }
    public byte[]? AlbumArt { get; set; }
    public int? TrackNumber { get; set; }
    public int? DiscNumber { get; set; }
}
