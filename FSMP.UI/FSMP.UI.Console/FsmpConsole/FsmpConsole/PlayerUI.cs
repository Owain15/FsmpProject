using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;

namespace FsmpConsole;

/// <summary>
/// Main application screen — music player with navigation to Browse, Playlists, and Directories.
/// </summary>
public class PlayerUI
{
    private readonly IPlaybackController _playback;
    private readonly IPlaylistManager _playlists;
    private readonly ILibraryManager _library;
    private readonly ILibraryBrowser _browser;
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly Action? _onClear;
    private bool _exitRequested;
    private volatile bool _trackEnded;
    private string? _statusMessage;

    public PlayerUI(
        IPlaybackController playback,
        IPlaylistManager playlists,
        ILibraryManager library,
        ILibraryBrowser browser,
        TextReader input,
        TextWriter output,
        Action? onClear = null)
    {
        _playback = playback ?? throw new ArgumentNullException(nameof(playback));
        _playlists = playlists ?? throw new ArgumentNullException(nameof(playlists));
        _library = library ?? throw new ArgumentNullException(nameof(library));
        _browser = browser ?? throw new ArgumentNullException(nameof(browser));
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _onClear = onClear;
    }

    /// <summary>
    /// Runs the player UI loop until the user chooses to go back.
    /// </summary>
    public async Task RunAsync()
    {
        _exitRequested = false;

        // Initialize audio player with timeout to avoid blocking the UI if LibVLC hangs.
        var initTask = Task.Run(() =>
        {
            try { _playback.SubscribeToTrackEnd(() => _trackEnded = true); }
            catch { /* player init deferred to first play */ }
        });
        if (await Task.WhenAny(initTask, Task.Delay(TimeSpan.FromSeconds(5))) != initTask)
        {
            _statusMessage = "Audio player still loading...";
        }

        while (!_exitRequested)
        {
            if (_trackEnded)
            {
                _trackEnded = false;
                var advanceResult = await _playback.AutoAdvanceAsync();
                if (!advanceResult.IsSuccess)
                    _statusMessage = advanceResult.ErrorMessage;
            }

            _onClear?.Invoke();
            await DisplayPlayerStateAsync();

            var raw = _input.ReadLine();
            if (raw == null)
                break; // end of input stream
            var line = raw.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(line))
                continue;

            await HandleInputAsync(line);
        }
    }

    /// <summary>
    /// Displays the current player state including track info, queue, and controls.
    /// </summary>
    public async Task DisplayPlayerStateAsync()
    {
        var trackResult = await _playback.GetCurrentTrackAsync();
        var currentTrack = trackResult.IsSuccess ? trackResult.Value : null;

        var queueResult = await _playback.GetQueueItemsAsync();
        var queueItems = new List<string>();
        if (queueResult.IsSuccess && queueResult.Value != null)
        {
            foreach (var item in queueResult.Value)
            {
                var durationStr = item.Duration.HasValue
                    ? $" [{item.Duration.Value.Minutes}:{item.Duration.Value.Seconds:D2}]"
                    : "";
                var prefix = item.IsCurrent ? "> " : "  ";
                var artistSuffix = !string.IsNullOrEmpty(item.Artist) ? $" - {item.Artist}" : "";
                queueItems.Add($"{prefix}{item.Index + 1}) {item.Title}{artistSuffix}{durationStr}");
            }
        }

        var message = _statusMessage;
        _statusMessage = null;

        Print.NewDisplay(
            _output,
            currentTrack,
            _playback.IsPlaying,
            queueItems,
            _playback.RepeatMode,
            _playback.IsShuffled,
            message);
    }

    /// <summary>
    /// Handles a single user input command.
    /// </summary>
    public async Task HandleInputAsync(string command)
    {
        switch (command)
        {
            case "N":
                var nextResult = await _playback.NextTrackAsync();
                if (!nextResult.IsSuccess)
                    _statusMessage = nextResult.ErrorMessage;
                else
                    _statusMessage = "Playing next track.";
                break;
            case "P":
                var prevResult = await _playback.PreviousTrackAsync();
                if (!prevResult.IsSuccess)
                    _statusMessage = prevResult.ErrorMessage;
                break;
            case "K":
                var toggleResult = await _playback.TogglePlayStopAsync();
                if (!toggleResult.IsSuccess)
                    _statusMessage = toggleResult.ErrorMessage;
                else
                    _statusMessage = _playback.IsPlaying ? "Playing." : "Stopped.";
                break;
            case "R":
                var restartResult = await _playback.RestartTrackAsync();
                if (!restartResult.IsSuccess)
                    _statusMessage = restartResult.ErrorMessage;
                break;
            case "S":
                var stopResult = await _playback.StopAsync();
                _statusMessage = stopResult.IsSuccess ? "Stopped." : stopResult.ErrorMessage;
                break;
            case "M":
                _playback.ToggleRepeatMode();
                _statusMessage = $"Repeat: {_playback.RepeatMode}";
                break;
            case "H":
                var shuffleResult = _playback.ToggleShuffle();
                if (!shuffleResult.IsSuccess)
                    _statusMessage = shuffleResult.ErrorMessage;
                else
                    _statusMessage = _playback.IsShuffled ? "Shuffle: ON" : "Shuffle: OFF";
                break;
            case "V":
                await ViewFullQueueAsync();
                break;
            case "B":
                await BrowseAsync();
                await AutoPlayIfQueuedAsync();
                break;
            case "L":
                await ManagePlaylistsAsync();
                await AutoPlayIfQueuedAsync();
                break;
            case "D":
                await ManageDirectoriesAsync();
                break;
            case "X":
                _exitRequested = true;
                _output.WriteLine();
                _output.WriteLine("Goodbye!");
                break;
            default:
                if (int.TryParse(command, out var trackNum)
                    && trackNum >= 1 && trackNum <= _playback.QueueCount)
                {
                    var jumpResult = await _playback.JumpToAsync(trackNum - 1);
                    if (!jumpResult.IsSuccess)
                        _statusMessage = jumpResult.ErrorMessage;
                }
                else
                {
                    _statusMessage = "Invalid selection.";
                }
                break;
        }
    }

    private async Task AutoPlayIfQueuedAsync()
    {
        if (!_playback.IsPlaying && _playback.CurrentIndex >= 0 && _playback.QueueCount > 0)
        {
            await _playback.JumpToAsync(_playback.CurrentIndex);
        }
    }

    private async Task ViewFullQueueAsync()
    {
        var queueResult = await _playback.GetQueueItemsAsync(truncate: false);
        if (!queueResult.IsSuccess || queueResult.Value == null || queueResult.Value.Count == 0)
        {
            _statusMessage = "Queue is empty.";
            return;
        }

        _output.WriteLine();
        _output.WriteLine($"== Full Queue ({_playback.QueueCount} tracks) ==");
        _output.WriteLine();
        foreach (var item in queueResult.Value)
        {
            var durationStr = item.Duration.HasValue
                ? $" [{item.Duration.Value.Minutes}:{item.Duration.Value.Seconds:D2}]"
                : "";
            var prefix = item.IsCurrent ? "> " : "  ";
            var artistSuffix = !string.IsNullOrEmpty(item.Artist) ? $" - {item.Artist}" : "";
            _output.WriteLine($"  {prefix}{item.Index + 1}) {item.Title}{artistSuffix}{durationStr}");
        }
        _output.WriteLine();
        _output.WriteLine("  [#] Skip to track  [0] Back");
        _output.WriteLine();
        _output.Write("Select: ");
        var input = _input.ReadLine()?.Trim();
        if (int.TryParse(input, out var trackNum)
            && trackNum >= 1 && trackNum <= _playback.QueueCount)
        {
            var jumpResult = await _playback.JumpToAsync(trackNum - 1);
            if (!jumpResult.IsSuccess)
                _statusMessage = jumpResult.ErrorMessage;
        }
    }

    private async Task BrowseAsync()
    {
        var browseUI = new BrowseUI(_browser, _playback, _input, _output, _onClear);
        await browseUI.RunAsync();
    }

    private async Task ManagePlaylistsAsync()
    {
        while (true)
        {
            var playlistsResult = await _playlists.GetAllPlaylistsAsync();
            if (!playlistsResult.IsSuccess)
            {
                _output.WriteLine($"Error: {playlistsResult.ErrorMessage}");
                return;
            }

            var playlists = playlistsResult.Value!;
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
                    var createResult = await _playlists.CreatePlaylistAsync(name, string.IsNullOrEmpty(desc) ? null : desc);
                    if (createResult.IsSuccess)
                        _output.WriteLine($"Created playlist: {createResult.Value!.Name}");
                    else
                        _output.WriteLine($"Error creating playlist: {createResult.ErrorMessage}");
                }
                continue;
            }

            if (int.TryParse(input, out var idx) && idx >= 1 && idx <= playlists.Count)
            {
                await ViewPlaylistAsync(playlists[idx - 1].PlaylistId);
            }
            else
            {
                _output.WriteLine("Invalid selection.");
            }
        }
    }

    private async Task ViewPlaylistAsync(int playlistId)
    {
        while (true)
        {
            var playlistResult = await _playlists.GetPlaylistWithTracksAsync(playlistId);
            if (!playlistResult.IsSuccess || playlistResult.Value == null)
            {
                _output.WriteLine("Playlist not found.");
                return;
            }

            var playlist = playlistResult.Value;
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
                    var trackResult = await _browser.GetTrackByIdAsync(pt.TrackId);
                    var track = trackResult.IsSuccess ? trackResult.Value : null;
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
            if (input == "0" || string.IsNullOrEmpty(input))
                return;

            switch (input)
            {
                case "l":
                    var loadResult = await _playlists.LoadPlaylistIntoQueueAsync(playlistId);
                    if (loadResult.IsSuccess)
                        _output.WriteLine($"Loaded {tracks.Count} tracks into player queue.");
                    else
                        _output.WriteLine(loadResult.ErrorMessage ?? "Error loading playlist.");
                    continue;
                case "d":
                    var deleteResult = await _playlists.DeletePlaylistAsync(playlistId);
                    if (deleteResult.IsSuccess)
                        _output.WriteLine("Playlist deleted.");
                    else
                        _output.WriteLine($"Error deleting playlist: {deleteResult.ErrorMessage}");
                    return; // playlist gone, back to list
                default:
                    _output.WriteLine("Invalid selection.");
                    break;
            }
        }
    }

    private async Task ManageDirectoriesAsync()
    {
        while (true)
        {
        var configResult = await _library.LoadConfigurationAsync();
        if (!configResult.IsSuccess)
        {
            _output.WriteLine($"Error: {configResult.ErrorMessage}");
            return;
        }
        var config = configResult.Value!;

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
        _output.WriteLine("  S) Scan all libraries");
        _output.WriteLine("  0) Back");
        _output.WriteLine();
        _output.Write("Select: ");

        var input = _input.ReadLine()?.Trim()?.ToLowerInvariant();
        if (input == "0" || string.IsNullOrEmpty(input))
            return;

        switch (input)
        {
            case "a":
                _output.Write("Enter path: ");
                var newPath = _input.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(newPath))
                {
                    var addResult = await _library.AddLibraryPathAsync(newPath);
                    if (addResult.IsSuccess)
                        _output.WriteLine("Path added.");
                    else
                        _output.WriteLine(addResult.ErrorMessage);
                }
                break;
            case "r":
                _output.Write("Enter path number to remove: ");
                var removeInput = _input.ReadLine()?.Trim();
                if (int.TryParse(removeInput, out var removeIndex)
                    && removeIndex >= 1 && removeIndex <= config.LibraryPaths.Count)
                {
                    var removeResult = await _library.RemoveLibraryPathAsync(config.LibraryPaths[removeIndex - 1]);
                    if (removeResult.IsSuccess)
                        _output.WriteLine("Path removed.");
                    else
                        _output.WriteLine(removeResult.ErrorMessage);
                }
                break;
            case "s":
                _output.WriteLine("Scanning libraries...");
                var scanResult = await _library.ScanAllLibrariesAsync();
                if (scanResult.IsSuccess)
                {
                    var r = scanResult.Value!;
                    _output.WriteLine($"Scan complete: {r.TracksAdded} added, {r.TracksUpdated} updated, {r.TracksRemoved} removed");
                    if (r.Errors.Count > 0)
                    {
                        _output.WriteLine($"  {r.Errors.Count} error(s):");
                        foreach (var error in r.Errors)
                            _output.WriteLine($"    - {error}");
                    }
                    _output.WriteLine($"  Duration: {r.Duration.TotalSeconds:F1}s");
                }
                else
                {
                    _output.WriteLine(scanResult.ErrorMessage);
                }
                break;
            default:
                _output.WriteLine("Invalid selection.");
                break;
        }
        }
    }
}
