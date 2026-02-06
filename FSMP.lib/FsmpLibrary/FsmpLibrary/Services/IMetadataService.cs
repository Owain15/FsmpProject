namespace FsmpLibrary.Services;

/// <summary>
/// Reads metadata from audio files using TagLibSharp.
/// </summary>
public interface IMetadataService
{
    /// <summary>
    /// Reads all available metadata from an audio file.
    /// Returns a populated TrackMetadata on success, or an empty one for corrupt/unsupported files.
    /// </summary>
    TrackMetadata ReadMetadata(string filePath);

    /// <summary>
    /// Extracts embedded album art from an audio file, or null if none is present.
    /// </summary>
    byte[]? ExtractAlbumArt(string filePath);

    /// <summary>
    /// Gets the duration of an audio file, or null if it cannot be determined.
    /// </summary>
    TimeSpan? GetDuration(string filePath);

    /// <summary>
    /// Gets technical audio properties (bit rate, sample rate, etc.) from a file.
    /// </summary>
    AudioProperties GetAudioProperties(string filePath);
}
