namespace FsmpLibrary.Models;

/// <summary>
/// Represents the application configuration stored in config.json.
/// </summary>
public class Configuration
{
    public List<string> LibraryPaths { get; set; } = new List<string>();
    public string DatabasePath { get; set; } = string.Empty;
    public bool AutoScanOnStartup { get; set; } = true;
    public int DefaultVolume { get; set; } = 75;
    public bool RememberLastPlayed { get; set; } = true;
    public string? LastPlayedTrackPath { get; set; }
}
