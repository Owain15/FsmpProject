using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using FsmpLibrary.Services;

namespace FsmpConsole;

/// <summary>
/// Interactive console menu system for navigating application features.
/// Uses TextReader/TextWriter for testability.
/// </summary>
public class MenuSystem
{
    private readonly IAudioService _audioService;
    private readonly ConfigurationService _configService;
    private readonly StatisticsService _statsService;
    private readonly LibraryScanService _scanService;
    private readonly UnitOfWork _unitOfWork;
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public MenuSystem(
        IAudioService audioService,
        ConfigurationService configService,
        StatisticsService statsService,
        LibraryScanService scanService,
        UnitOfWork unitOfWork,
        TextReader input,
        TextWriter output)
    {
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _statsService = statsService ?? throw new ArgumentNullException(nameof(statsService));
        _scanService = scanService ?? throw new ArgumentNullException(nameof(scanService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    /// <summary>
    /// Runs the main menu loop until the user exits.
    /// </summary>
    public async Task RunAsync()
    {
        while (true)
        {
            DisplayMainMenu();
            var choice = _input.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(choice))
                continue;

            switch (choice)
            {
                case "1":
                    await BrowseAndPlayAsync();
                    break;
                case "2":
                    await ScanLibrariesAsync();
                    break;
                case "3":
                    await ViewStatisticsAsync();
                    break;
                case "4":
                    await ManageLibrariesAsync();
                    break;
                case "5":
                    await SettingsAsync();
                    break;
                case "6":
                    _output.WriteLine("Goodbye!");
                    return;
                default:
                    _output.WriteLine("Invalid option. Please enter 1-6.");
                    break;
            }
        }
    }

    /// <summary>
    /// Displays the main menu options.
    /// </summary>
    public void DisplayMainMenu()
    {
        Print.WriteSelectionMenu(_output, "FSMP - File System Music Player",
            new[] { "Browse & Play", "Scan Libraries", "View Statistics", "Manage Libraries", "Settings", "Exit" },
            "Select option", backLabel: null);
    }

    private async Task BrowseAndPlayAsync()
    {
        var browseUI = new BrowseUI(_unitOfWork, _audioService, _input, _output);
        await browseUI.RunAsync();
    }

    private async Task ScanLibrariesAsync()
    {
        var config = await _configService.LoadConfigurationAsync();

        if (config.LibraryPaths.Count == 0)
        {
            _output.WriteLine("No library paths configured. Add one in Manage Libraries.");
            return;
        }

        _output.WriteLine("Scanning libraries...");
        var result = await _scanService.ScanAllLibrariesAsync(config.LibraryPaths);

        _output.WriteLine($"Scan complete: {result.TracksAdded} added, {result.TracksUpdated} updated, {result.TracksRemoved} removed");
        if (result.Errors.Count > 0)
            _output.WriteLine($"  {result.Errors.Count} error(s) occurred.");
        _output.WriteLine($"  Duration: {result.Duration.TotalSeconds:F1}s");
    }

    private async Task ViewStatisticsAsync()
    {
        var totalTracks = await _statsService.GetTotalTrackCountAsync();
        var totalPlays = await _statsService.GetTotalPlayCountAsync();
        var totalTime = await _statsService.GetTotalListeningTimeAsync();

        Print.WriteDetailCard(_output, "Library Statistics", new List<(string Label, string Value)>
        {
            ("Total tracks:", totalTracks.ToString()),
            ("Total plays:", totalPlays.ToString()),
            ("Listen time:", $"{totalTime.Hours}h {totalTime.Minutes}m")
        });

        var mostPlayed = (await _statsService.GetMostPlayedTracksAsync(5)).ToList();
        if (mostPlayed.Count > 0)
        {
            _output.WriteLine();
            _output.WriteLine("  Most Played:");
            foreach (var t in mostPlayed)
                _output.WriteLine($"    {t.DisplayTitle} ({t.PlayCount} plays)");
        }
    }

    private async Task ManageLibrariesAsync()
    {
        var config = await _configService.LoadConfigurationAsync();

        _output.WriteLine();
        _output.WriteLine("== Library Paths ==");
        _output.WriteLine();
        if (config.LibraryPaths.Count == 0)
        {
            _output.WriteLine("  (none configured)");
        }
        else
        {
            for (int i = 0; i < config.LibraryPaths.Count; i++)
                _output.WriteLine($"  {i + 1}) {config.LibraryPaths[i]}");
        }
        _output.WriteLine();
        _output.WriteLine("  A) Add path");
        _output.WriteLine("  R) Remove path");
        _output.WriteLine("  0) Back");
        _output.WriteLine();
        _output.Write("Select: ");

        var input = _input.ReadLine()?.Trim()?.ToLowerInvariant();

        switch (input)
        {
            case "a":
                _output.Write("Enter path: ");
                var newPath = _input.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(newPath))
                {
                    await _configService.AddLibraryPathAsync(newPath);
                    _output.WriteLine("Path added.");
                }
                break;
            case "r":
                _output.Write("Enter path number to remove: ");
                var removeInput = _input.ReadLine()?.Trim();
                if (int.TryParse(removeInput, out var removeIndex)
                    && removeIndex >= 1 && removeIndex <= config.LibraryPaths.Count)
                {
                    await _configService.RemoveLibraryPathAsync(config.LibraryPaths[removeIndex - 1]);
                    _output.WriteLine("Path removed.");
                }
                break;
        }
    }

    private async Task SettingsAsync()
    {
        var config = await _configService.LoadConfigurationAsync();

        Print.WriteDetailCard(_output, "Settings", new List<(string Label, string Value)>
        {
            ("Volume:", $"{config.DefaultVolume}%"),
            ("Auto-scan:", config.AutoScanOnStartup ? "On" : "Off"),
            ("Database path:", config.DatabasePath)
        });
        _output.WriteLine();
        _output.WriteLine("  0) Back");
        _output.WriteLine();
        _output.Write("Select: ");
        _input.ReadLine(); // consume input
    }
}
