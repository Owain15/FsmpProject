using FSMP.Core;
using FsmpDataAcsses;
using FSMP.Core.Models;
using FsmpLibrary.Services;

namespace FsmpConsole;

/// <summary>
/// Console music player view with queue display and playback controls.
/// </summary>
public class PlayerUI
{
    private readonly ActivePlaylistService _activePlaylist;
    private readonly IAudioService _audioService;
    private readonly UnitOfWork _unitOfWork;
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private bool _isPlaying;

    public PlayerUI(
        ActivePlaylistService activePlaylist,
        IAudioService audioService,
        UnitOfWork unitOfWork,
        TextReader input,
        TextWriter output)
    {
        _activePlaylist = activePlaylist ?? throw new ArgumentNullException(nameof(activePlaylist));
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    /// <summary>
    /// Runs the player UI loop until the user chooses to go back.
    /// </summary>
    public async Task RunAsync()
    {
        while (true)
        {
            await DisplayPlayerStateAsync();

            var line = _input.ReadLine()?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(line))
                continue;

            if (line == "Q")
                break;

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

        Print.NewDisplay(
            _output,
            currentTrack,
            _isPlaying,
            queueItems,
            _activePlaylist.RepeatMode,
            _activePlaylist.IsShuffled);
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
            case " ":
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
            _output.WriteLine("  End of queue.");
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
            _output.WriteLine("  Beginning of queue.");
        }
    }

    private async Task TogglePauseAsync()
    {
        if (_isPlaying)
        {
            await _audioService.PauseAsync();
            _isPlaying = false;
        }
        else if (_activePlaylist.CurrentTrackId.HasValue)
        {
            await _audioService.ResumeAsync();
            _isPlaying = true;
        }
    }

    private async Task RestartTrackAsync()
    {
        if (_activePlaylist.CurrentTrackId.HasValue)
        {
            await _audioService.SeekAsync(TimeSpan.Zero);
            _isPlaying = true;
        }
    }

    private async Task StopAsync()
    {
        await _audioService.StopAsync();
        _isPlaying = false;
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
    }

    private void ToggleShuffle()
    {
        _activePlaylist.ToggleShuffle();
    }

    private async Task PlayTrackByIdAsync(int trackId)
    {
        var track = await _unitOfWork.Tracks.GetByIdAsync(trackId);
        if (track != null)
        {
            await _audioService.PlayTrackAsync(track);
            _isPlaying = true;
        }
    }

    private async Task<Track?> GetCurrentTrackAsync()
    {
        var trackId = _activePlaylist.CurrentTrackId;
        if (!trackId.HasValue) return null;
        return await _unitOfWork.Tracks.GetByIdAsync(trackId.Value);
    }

    private async Task<List<string>> BuildQueueDisplayAsync()
    {
        var items = new List<string>();
        var playOrder = _activePlaylist.PlayOrder;
        var currentIndex = _activePlaylist.CurrentIndex;

        for (int i = 0; i < playOrder.Count; i++)
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

        return items;
    }
}