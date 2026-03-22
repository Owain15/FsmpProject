namespace FSMP.Core.Models;

/// <summary>
/// Represents the application configuration stored in config.json.
/// </summary>
public class Configuration
{
    public List<string> LibraryPaths { get; set; } = new List<string>();
    public string DatabasePath { get; set; } = string.Empty;
    public bool AutoScanOnStartup { get; set; } = true;
    public int DefaultVolume { get; set; } = 75;
    public bool ResumeSession { get; set; } = true;
    public bool AutoPlayOnStartup { get; set; } = false;
    public string? LastPlayedTrackPath { get; set; }
    public string Theme { get; set; } = "Light";
    public string TextSize { get; set; } = "Medium";
    public bool AllowUnsaveFromTagList { get; set; } = false;
    public string DoubleClickAction { get; set; } = "PlayNow";
    public string DefaultSortOrder { get; set; } = "Artist";
    public string DefaultOrganizeMode { get; set; } = "Copy";
    public string DefaultDuplicateStrategy { get; set; } = "Skip";
    public string UnknownArtistName { get; set; } = "Unknown Artist";
    public string UnknownAlbumName { get; set; } = "Unknown Album";
    public Dictionary<string, string>? CustomThemeColors { get; set; }
    public List<NamedCustomTheme> SavedCustomThemes { get; set; } = new();
}

public class NamedCustomTheme
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, string> Colors { get; set; } = new();
}