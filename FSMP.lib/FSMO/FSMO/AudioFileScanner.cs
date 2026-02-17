namespace FSMO;

public class AudioFileScanner
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3",
        ".wav",
        ".wma"
    };

    public static bool IsSupportedFormat(string extension)
    {
        return SupportedExtensions.Contains(extension);
    }

    public static List<FileInfo> ScanDirectory(string sourcePath)
    {
        if (string.IsNullOrEmpty(sourcePath))
            throw new ArgumentException("Source path cannot be null or empty.", nameof(sourcePath));

        if (!Directory.Exists(sourcePath))
            throw new DirectoryNotFoundException($"Directory not found: {sourcePath}");

        var directory = new DirectoryInfo(sourcePath);
        return directory
            .EnumerateFiles("*", SearchOption.AllDirectories)
            .Where(f => IsSupportedFormat(f.Extension))
            .ToList();
    }
}
