namespace FsmpLibrary.Services;

/// <summary>
/// Technical audio properties extracted from a file (bit rate, sample rate, etc.).
/// </summary>
public class AudioProperties
{
    public int? BitRate { get; set; }
    public int? SampleRate { get; set; }
    public int? Channels { get; set; }
    public int? BitsPerSample { get; set; }
}
