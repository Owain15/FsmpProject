namespace FsmpLibrary.Services;

/// <summary>
/// Reads metadata from audio files (WAV, WMA, MP3) using TagLibSharp.
/// Corrupt or unsupported files return empty/null results rather than throwing.
/// </summary>
public class MetadataService : IMetadataService
{
    public TrackMetadata ReadMetadata(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Audio file not found.", filePath);

        try
        {
            using var tagFile = TagLib.File.Create(filePath);
            var tag = tagFile.Tag;
            var props = tagFile.Properties;

            return new TrackMetadata
            {
                Title = NullIfEmpty(tag.Title),
                Artist = NullIfEmpty(tag.FirstPerformer),
                Album = NullIfEmpty(tag.Album),
                Year = tag.Year > 0 ? (int)tag.Year : null,
                Genre = NullIfEmpty(tag.FirstGenre),
                Duration = props.Duration > TimeSpan.Zero ? props.Duration : null,
                BitRate = props.AudioBitrate > 0 ? props.AudioBitrate : null,
                SampleRate = props.AudioSampleRate > 0 ? props.AudioSampleRate : null,
                AlbumArt = ExtractAlbumArtInternal(tag),
                TrackNumber = tag.Track > 0 ? (int)tag.Track : null,
                DiscNumber = tag.Disc > 0 ? (int)tag.Disc : null,
            };
        }
        catch (TagLib.CorruptFileException)
        {
            return new TrackMetadata();
        }
        catch (TagLib.UnsupportedFormatException)
        {
            return new TrackMetadata();
        }
    }

    public byte[]? ExtractAlbumArt(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Audio file not found.", filePath);

        try
        {
            using var tagFile = TagLib.File.Create(filePath);
            return ExtractAlbumArtInternal(tagFile.Tag);
        }
        catch (TagLib.CorruptFileException)
        {
            return null;
        }
        catch (TagLib.UnsupportedFormatException)
        {
            return null;
        }
    }

    public TimeSpan? GetDuration(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Audio file not found.", filePath);

        try
        {
            using var tagFile = TagLib.File.Create(filePath);
            var duration = tagFile.Properties.Duration;
            return duration > TimeSpan.Zero ? duration : null;
        }
        catch (TagLib.CorruptFileException)
        {
            return null;
        }
        catch (TagLib.UnsupportedFormatException)
        {
            return null;
        }
    }

    public AudioProperties GetAudioProperties(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Audio file not found.", filePath);

        try
        {
            using var tagFile = TagLib.File.Create(filePath);
            var props = tagFile.Properties;

            return new AudioProperties
            {
                BitRate = props.AudioBitrate > 0 ? props.AudioBitrate : null,
                SampleRate = props.AudioSampleRate > 0 ? props.AudioSampleRate : null,
                Channels = props.AudioChannels > 0 ? props.AudioChannels : null,
                BitsPerSample = props.BitsPerSample > 0 ? props.BitsPerSample : null,
            };
        }
        catch (TagLib.CorruptFileException)
        {
            return new AudioProperties();
        }
        catch (TagLib.UnsupportedFormatException)
        {
            return new AudioProperties();
        }
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private static byte[]? ExtractAlbumArtInternal(TagLib.Tag tag)
    {
        var pictures = tag.Pictures;
        if (pictures.Length > 0)
        {
            return pictures[0].Data.Data;
        }
        return null;
    }
}
