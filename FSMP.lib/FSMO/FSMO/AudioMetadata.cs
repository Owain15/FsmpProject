namespace FSMO;

public class AudioMetadata
{
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public int? TrackNumber { get; set; }
    public int? Year { get; set; }
    public TimeSpan? Duration { get; set; }
}