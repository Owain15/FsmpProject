namespace FSMO;

public static class MetadataReader
{
    public static AudioMetadata ReadMetadata(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Audio file not found.", filePath);

        try
        {
            using var tagFile = TagLib.File.Create(filePath);
            var tag = tagFile.Tag;
            var props = tagFile.Properties;

            return new AudioMetadata
            {
                Title = NullIfEmpty(tag.Title),
                Artist = NullIfEmpty(tag.FirstPerformer),
                Album = NullIfEmpty(tag.Album),
                TrackNumber = tag.Track > 0 ? (int)tag.Track : null,
                Year = tag.Year > 0 ? (int)tag.Year : null,
                Duration = props.Duration > TimeSpan.Zero ? props.Duration : null,
            };
        }
        catch (TagLib.CorruptFileException)
        {
            return new AudioMetadata();
        }
        catch (TagLib.UnsupportedFormatException)
        {
            return new AudioMetadata();
        }
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}