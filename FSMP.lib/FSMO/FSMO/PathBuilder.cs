namespace FSMO;

public static class PathBuilder
{
    private const string UnknownArtist = "Unknown Artist";
    private const string UnknownAlbum = "Unknown Album";

    public static string BuildTargetPath(string destinationRoot, AudioMetadata metadata, string originalFileName)
    {
        ArgumentNullException.ThrowIfNull(destinationRoot);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(originalFileName);

        var artist = SanitizeFolderName(metadata.Artist, UnknownArtist);
        var album = SanitizeFolderName(metadata.Album, UnknownAlbum);

        return Path.Combine(destinationRoot, artist, album, originalFileName);
    }

    private static string SanitizeFolderName(string? name, string fallback)
    {
        if (string.IsNullOrWhiteSpace(name))
            return fallback;

        var trimmed = name.Trim();

        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
        {
            trimmed = trimmed.Replace(c.ToString(), "");
        }

        return string.IsNullOrWhiteSpace(trimmed) ? fallback : trimmed;
    }
}