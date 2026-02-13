using FSMP.Core;
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
    private readonly PlaylistService _playlistService;
    private readonly ActivePlaylistService _activePlaylist;
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public MenuSystem(
        IAudioService audioService,
        ConfigurationService configService,
        StatisticsService statsService,
        LibraryScanService scanService,
        UnitOfWork unitOfWork,
        PlaylistService playlistService,
        ActivePlaylistService activePlaylist,
        TextReader input,
        TextWriter output)
    {
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _statsService = statsService ?? throw new ArgumentNullException(nameof(statsService));
        _scanService = scanService ?? throw new ArgumentNullException(nameof(scanService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _playlistService = playlistService ?? throw new ArgumentNullException(nameof(playlistService));
        _activePlaylist = activePlaylist ?? throw new ArgumentNullException(nameof(activePlaylist));
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
                    await OpenPlayerAsync();
                    break;
                case "3":
                    await ManagePlaylistsAsync();
                    break;
                case "4":
                    await ScanLibrariesAsync();
                    break;
                case "5":
                    await ViewStatisticsAsync();
                    break;
                case "6":
                    await ManageLibrariesAsync();
                    break;
                case "7":
                    await SettingsAsync();
                    break;
                case "8":
					_output.WriteLine(""); _output.WriteLine("Goodbye!");
                    return;
                default:
					_output.WriteLine(""); _output.WriteLine("Invalid option. Please enter 1-8.");
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
            new[] { "Browse & Play", "Player", "Playlists", "Scan Libraries", "View Statistics", "Manage Libraries", "Settings", "Exit" },
            "Select option", backLabel: null);
    }

    private async Task BrowseAndPlayAsync()
    {
        var browseUI = new BrowseUI(_unitOfWork, _audioService, _activePlaylist, _input, _output);
        await browseUI.RunAsync();
    }

    private async Task OpenPlayerAsync()
    {
        var playerUI = new PlayerUI(_activePlaylist, _audioService, _unitOfWork, _input, _output);
        await playerUI.RunAsync();
    }

    private async Task ManagePlaylistsAsync()
    {
        while (true)
        {
            var playlists = (await _playlistService.GetAllPlaylistsAsync()).ToList();

            var items = playlists.Select(p =>
            {
                var trackCount = p.PlaylistTracks?.Count ?? 0;
                return $"{p.Name} ({trackCount} tracks)";
            }).ToList();

            _output.WriteLine();
            _output.WriteLine("== Playlists ==");
            _output.WriteLine();
            for (int i = 0; i < items.Count; i++)
                _output.WriteLine($"  {i + 1}) {items[i]}");
            _output.WriteLine();
            _output.WriteLine("  C) Create new playlist");
            _output.WriteLine("  0) Back");
            _output.WriteLine();
            _output.Write("Select: ");

            var input = _input.ReadLine()?.Trim();
            if (input == "0" || string.IsNullOrEmpty(input))
                return;

            if (input?.Equals("c", StringComparison.OrdinalIgnoreCase) == true)
            {
                _output.Write("Playlist name: ");
                var name = _input.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    _output.Write("Description (optional): ");
                    var desc = _input.ReadLine()?.Trim();
                    var created = await _playlistService.CreatePlaylistAsync(name, string.IsNullOrEmpty(desc) ? null : desc);
                    _output.WriteLine($"Created playlist: {created.Name}");
                }
                continue;
            }

            if (int.TryParse(input, out var idx) && idx >= 1 && idx <= playlists.Count)
            {
                await ViewPlaylistAsync(playlists[idx - 1].PlaylistId);
            }
        }
    }

    private async Task ViewPlaylistAsync(int playlistId)
    {
        var playlist = await _playlistService.GetPlaylistWithTracksAsync(playlistId);
        if (playlist == null)
        {
            _output.WriteLine("Playlist not found.");
            return;
        }

        var tracks = playlist.PlaylistTracks.OrderBy(pt => pt.Position).ToList();

        _output.WriteLine();
        _output.WriteLine($"== {playlist.Name} ==");
        if (!string.IsNullOrEmpty(playlist.Description))
            _output.WriteLine($"  {playlist.Description}");
        _output.WriteLine();

        if (tracks.Count == 0)
        {
            _output.WriteLine("  (no tracks)");
        }
        else
        {
            foreach (var pt in tracks)
            {
                var track = await _unitOfWork.Tracks.GetByIdAsync(pt.TrackId);
                var title = track?.DisplayTitle ?? "Unknown";
                var artist = track?.DisplayArtist ?? "";
                var artistSuffix = !string.IsNullOrEmpty(artist) ? $" - {artist}" : "";
                _output.WriteLine($"  {pt.Position + 1}) {title}{artistSuffix}");
            }
        }

        _output.WriteLine();
        _output.WriteLine("  L) Load into Player queue");
        _output.WriteLine("  D) Delete playlist");
        _output.WriteLine("  0) Back");
        _output.WriteLine();
        _output.Write("Select: ");

        var input = _input.ReadLine()?.Trim()?.ToLowerInvariant();

        switch (input)
        {
            case "l":
                if (tracks.Count > 0)
                {
                    _activePlaylist.SetQueue(tracks.Select(pt => pt.TrackId).ToList());
                    _output.WriteLine($"Loaded {tracks.Count} tracks into player queue.");
                }
                else
                {
                    _output.WriteLine("No tracks to load.");
                }
                break;
            case "d":
                await _playlistService.DeletePlaylistAsync(playlistId);
                _output.WriteLine("Playlist deleted.");
                break;
        }
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
