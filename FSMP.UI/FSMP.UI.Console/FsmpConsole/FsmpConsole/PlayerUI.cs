using FSMP.Core;
using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using FSMP.Core.Models;
using FsmpLibrary.Services;

namespace FsmpConsole;

/// <summary>
/// Main application screen — music player with navigation to Browse, Playlists, and Directories.
/// </summary>
public class PlayerUI
{
    private readonly ActivePlaylistService _activePlaylist;
    private readonly IAudioService _audioService;
    private readonly UnitOfWork _unitOfWork;
    private readonly PlaylistService _playlistService;
    private readonly ConfigurationService _configService;
    private readonly LibraryScanService _scanService;
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly Action? _onClear;
    private bool _isPlaying;
    private bool _isStopped;
    private bool _exitRequested;
    private volatile bool _trackEnded;
    private string? _statusMessage;
    private bool _playerEventSubscribed;

    public PlayerUI(
        ActivePlaylistService activePlaylist,
        IAudioService audioService,
        UnitOfWork unitOfWork,
        PlaylistService playlistService,
        ConfigurationService configService,
        LibraryScanService scanService,
        TextReader input,
        TextWriter output,
        Action? onClear = null)
    {
        _activePlaylist = activePlaylist ?? throw new ArgumentNullException(nameof(activePlaylist));
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _playlistService = playlistService ?? throw new ArgumentNullException(nameof(playlistService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _scanService = scanService ?? throw new ArgumentNullException(nameof(scanService));
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
            try { EnsurePlayerEventSubscribed(); }
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
                var nextId = _activePlaylist.MoveNext();
                if (nextId.HasValue)
                    await PlayTrackByIdAsync(nextId.Value);
                else
                    _statusMessage = "End of queue.";
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
        var currentTrack = await GetCurrentTrackAsync();
        var queueItems = await BuildQueueDisplayAsync();
        var message = _statusMessage;
        _statusMessage = null;

        Print.NewDisplay(
            _output,
            currentTrack,
            _isPlaying,
            queueItems,
            _activePlaylist.RepeatMode,
            _activePlaylist.IsShuffled,
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
                await NextTrackAsync();
                break;
            case "P":
                await PreviousTrackAsync();
                break;
            case "K":
                await TogglePauseAsync();
                break;
            case "R":
                await RestartTrackAsync();
                break;
            case "S":
                await StopAsync();
                break;
            case "M":
                ToggleRepeatMode();
                break;
            case "H":
                ToggleShuffle();
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
                    && trackNum >= 1 && trackNum <= _activePlaylist.Count)
                {
                    _activePlaylist.JumpTo(trackNum - 1);
                    await PlayTrackByIdAsync(_activePlaylist.CurrentTrackId!.Value);
                }
                else
                {
                    _statusMessage = "Invalid selection.";
                }
                break;
        }
    }

    private async Task NextTrackAsync()
    {
        var nextId = _activePlaylist.MoveNext();
        if (nextId.HasValue)
        {
            await PlayTrackByIdAsync(nextId.Value);
        }
        else
        {
            await _audioService.StopAsync();
            _isPlaying = false;
            _statusMessage = "End of queue.";
        }
    }

    private async Task PreviousTrackAsync()
    {
        var prevId = _activePlaylist.MovePrevious();
        if (prevId.HasValue)
        {
            await PlayTrackByIdAsync(prevId.Value);
        }
        else
        {
            _statusMessage = "Beginning of queue.";
        }
    }

    private async Task TogglePauseAsync()
    {
        if (_isPlaying)
        {
            await _audioService.PauseAsync();
            _isPlaying = false;
            _statusMessage = "Paused.";
        }
        else if (_activePlaylist.CurrentTrackId.HasValue)
        {
            if (_isStopped)
            {
                // After stop, must re-load the track
                await PlayTrackByIdAsync(_activePlaylist.CurrentTrackId.Value);
                _isStopped = false;
            }
            else
            {
                await _audioService.ResumeAsync();
                _isPlaying = true;
            }
            _statusMessage = "Playing.";
        }
    }

    private async Task RestartTrackAsync()
    {
        if (_activePlaylist.CurrentTrackId.HasValue)
        {
            await _audioService.SeekAsync(TimeSpan.Zero);
            await _audioService.ResumeAsync();
            _isPlaying = true;
        }
    }

    private async Task StopAsync()
    {
        await _audioService.StopAsync();
        _isPlaying = false;
        _isStopped = true;
        _statusMessage = "Stopped.";
    }

    private void ToggleRepeatMode()
    {
        _activePlaylist.RepeatMode = _activePlaylist.RepeatMode switch
        {
            RepeatMode.None => RepeatMode.One,
            RepeatMode.One => RepeatMode.All,
            RepeatMode.All => RepeatMode.None,
            _ => RepeatMode.None
        };
        _statusMessage = $"Repeat: {_activePlaylist.RepeatMode}";
    }

    private void ToggleShuffle()
    {
        if (_activePlaylist.Count == 0)
        {
            _statusMessage = "No tracks in queue to shuffle.";
            return;
        }
        _activePlaylist.ToggleShuffle();
        _statusMessage = _activePlaylist.IsShuffled ? "Shuffle: ON" : "Shuffle: OFF";
    }

    private void EnsurePlayerEventSubscribed()
    {
        if (_playerEventSubscribed) return;
        _playerEventSubscribed = true;
        _audioService.Player.PlaybackCompleted += (s, e) =>
        {
            _isPlaying = false;
            _trackEnded = true;
        };
    }

    private async Task AutoPlayIfQueuedAsync()
    {
        if (!_isPlaying && _activePlaylist.CurrentTrackId.HasValue)
        {
            await PlayTrackByIdAsync(_activePlaylist.CurrentTrackId.Value);
        }
    }

    private async Task PlayTrackByIdAsync(int trackId)
    {
        var track = await _unitOfWork.Tracks.GetByIdAsync(trackId);
        if (track == null)
        {
            _statusMessage = $"Track {trackId} not found in database.";
            return;
        }
        try
        {
            await _audioService.PlayTrackAsync(track);
            _isPlaying = true;
            _isStopped = false;
            _statusMessage = $"Now playing: {track.DisplayTitle}";
        }
        catch (Exception ex)
        {
            _isPlaying = false;
            _statusMessage = $"Playback error: {ex.Message}";
        }
    }

    private async Task<Track?> GetCurrentTrackAsync()
    {
        var trackId = _activePlaylist.CurrentTrackId;
        if (!trackId.HasValue) return null;
        return await _unitOfWork.Tracks.GetByIdAsync(trackId.Value);
    }

    private async Task<List<string>> BuildQueueDisplayAsync(bool truncate = true)
    {
        var items = new List<string>();
        var playOrder = _activePlaylist.PlayOrder;
        var currentIndex = _activePlaylist.CurrentIndex;
        const int maxVisible = 9;

        var shouldTruncate = truncate && playOrder.Count > maxVisible + 1;

        // Calculate the window of tracks to display
        int startIndex = 0;
        int endIndex = playOrder.Count;

        if (shouldTruncate)
        {
            // Place current track in the middle 3 lines (positions 3-5 of 0-8)
            // Target: currentIndex at display position 4 (middle)
            startIndex = Math.Max(0, currentIndex - 4);
            endIndex = startIndex + maxVisible;

            // Clamp to end of list
            if (endIndex > playOrder.Count)
            {
                endIndex = playOrder.Count;
                startIndex = Math.Max(0, endIndex - maxVisible);
            }
        }

        if (shouldTruncate && startIndex > 0)
        {
            items.Add($"  ... {startIndex} earlier");
        }

        for (int i = startIndex; i < endIndex; i++)
        {
            var track = await _unitOfWork.Tracks.GetByIdAsync(playOrder[i]);
            var title = track?.DisplayTitle ?? "Unknown";
            var artist = track?.DisplayArtist ?? "";
            var duration = track?.Duration.HasValue == true
                ? $" [{track.Duration.Value.Minutes}:{track.Duration.Value.Seconds:D2}]"
                : "";

            var prefix = i == currentIndex ? "> " : "  ";
            var artistSuffix = !string.IsNullOrEmpty(artist) ? $" - {artist}" : "";
            items.Add($"{prefix}{i + 1}) {title}{artistSuffix}{duration}");
        }

        if (shouldTruncate && endIndex < playOrder.Count)
        {
            var remaining = playOrder.Count - endIndex;
            items.Add($"  ... {remaining} more — press [V] to view all");
        }

        return items;
    }

    private async Task ViewFullQueueAsync()
    {
        var items = await BuildQueueDisplayAsync(truncate: false);
        if (items.Count == 0)
        {
            _statusMessage = "Queue is empty.";
            return;
        }

        _output.WriteLine();
        _output.WriteLine($"== Full Queue ({_activePlaylist.Count} tracks) ==");
        _output.WriteLine();
        foreach (var item in items)
            _output.WriteLine($"  {item}");
        _output.WriteLine();
        _output.WriteLine("  [#] Skip to track  [0] Back");
        _output.WriteLine();
        _output.Write("Select: ");
        var input = _input.ReadLine()?.Trim();
        if (int.TryParse(input, out var trackNum)
            && trackNum >= 1 && trackNum <= _activePlaylist.Count)
        {
            _activePlaylist.JumpTo(trackNum - 1);
            await PlayTrackByIdAsync(_activePlaylist.CurrentTrackId!.Value);
        }
    }

    private async Task BrowseAsync()
    {
        var browseUI = new BrowseUI(_unitOfWork, _audioService, _activePlaylist, _input, _output, _onClear);
        await browseUI.RunAsync();
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
                    try
                    {
                        var created = await _playlistService.CreatePlaylistAsync(name, string.IsNullOrEmpty(desc) ? null : desc);
                        _output.WriteLine($"Created playlist: {created.Name}");
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"Error creating playlist: {ex.Message}");
                    }
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
            if (input == "0" || string.IsNullOrEmpty(input))
                return;

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
                    continue;
                case "d":
                    try
                    {
                        await _playlistService.DeletePlaylistAsync(playlistId);
                        _output.WriteLine("Playlist deleted.");
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"Error deleting playlist: {ex.Message}");
                    }
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
                    if (!Directory.Exists(newPath))
                    {
                        _output.WriteLine($"Directory not found: {newPath}");
                    }
                    else
                    {
                        await _configService.AddLibraryPathAsync(newPath);
                        _output.WriteLine("Path added.");
                    }
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
            case "s":
                if (config.LibraryPaths.Count == 0)
                {
                    _output.WriteLine("No library paths configured.");
                }
                else
                {
                    _output.WriteLine("Scanning libraries...");
                    var result = await _scanService.ScanAllLibrariesAsync(config.LibraryPaths);
                    _output.WriteLine($"Scan complete: {result.TracksAdded} added, {result.TracksUpdated} updated, {result.TracksRemoved} removed");
                    if (result.Errors.Count > 0)
                    {
                        _output.WriteLine($"  {result.Errors.Count} error(s):");
                        foreach (var error in result.Errors)
                            _output.WriteLine($"    - {error}");
                    }
                    _output.WriteLine($"  Duration: {result.Duration.TotalSeconds:F1}s");
                }
                break;
            default:
                _output.WriteLine("Invalid selection.");
                break;
        }
        }
    }
}