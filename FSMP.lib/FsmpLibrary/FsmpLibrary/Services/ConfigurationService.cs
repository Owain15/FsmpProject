using System.Text.Json;
using FSMP.Core.Models;

namespace FsmpLibrary.Services;

/// <summary>
/// Manages application configuration stored as JSON.
/// </summary>
public class ConfigurationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _configPath;

    public ConfigurationService(string configPath)
    {
        _configPath = configPath;
    }

    public Configuration GetDefaultConfiguration()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return new Configuration
        {
            LibraryPaths = new List<string>(),
            DatabasePath = Path.Combine(appData, "FSMP", "fsmp.db"),
            AutoScanOnStartup = true,
            DefaultVolume = 75,
            RememberLastPlayed = true,
            LastPlayedTrackPath = null
        };
    }

    public async Task<Configuration> LoadConfigurationAsync()
    {
        if (!File.Exists(_configPath))
        {
            var defaultConfig = GetDefaultConfiguration();
            await SaveConfigurationAsync(defaultConfig);
            return defaultConfig;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_configPath);
            return JsonSerializer.Deserialize<Configuration>(json, JsonOptions)
                   ?? GetDefaultConfiguration();
        }
        catch (JsonException)
        {
            // Corrupt config file — overwrite with defaults
            var defaultConfig = GetDefaultConfiguration();
            await SaveConfigurationAsync(defaultConfig);
            return defaultConfig;
        }
    }

    public async Task SaveConfigurationAsync(Configuration config)
    {
        var directory = Path.GetDirectoryName(_configPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(config, JsonOptions);
        await File.WriteAllTextAsync(_configPath, json);
    }

    public async Task AddLibraryPathAsync(string path)
    {
        var config = await LoadConfigurationAsync();
        if (!config.LibraryPaths.Contains(path))
        {
            config.LibraryPaths.Add(path);
            await SaveConfigurationAsync(config);
        }
    }

    public async Task RemoveLibraryPathAsync(string path)
    {
        var config = await LoadConfigurationAsync();
        if (config.LibraryPaths.Remove(path))
        {
            await SaveConfigurationAsync(config);
        }
    }
}
