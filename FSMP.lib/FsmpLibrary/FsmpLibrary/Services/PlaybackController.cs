using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;

namespace FsmpLibrary.Services;

public class PlaybackController : IPlaybackController
{
    private readonly IAudioService _audioService;
    private readonly IActivePlaylistService _activePlaylist;
    private readonly ITrackRepository _trackRepository;
    private bool _playerEventSubscribed;

    public PlaybackController(
        IAudioService audioService,
        IActivePlaylistService activePlaylist,
        ITrackRepository trackRepository)
    {
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _activePlaylist = activePlaylist ?? throw new ArgumentNullException(nameof(activePlaylist));
        _trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
    }

    public bool IsPlaying => _audioService.Player.State == PlaybackState.Playing;
    public RepeatMode RepeatMode => _activePlaylist.RepeatMode;
    public bool IsShuffled => _activePlaylist.IsShuffled;
    public int QueueCount => _activePlaylist.Count;
    public int CurrentIndex => _activePlaylist.CurrentIndex;

    public async Task<Result> PlayTrackByIdAsync(int trackId)
    {
        try
        {
            var track = await _trackRepository.GetByIdAsync(trackId);
            if (track == null)
                return Result.Failure($"Track {trackId} not found in database.");

            await _audioService.PlayTrackAsync(track);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Playback error: {ex.Message}");
        }
    }

    public async Task<Result> NextTrackAsync()
    {
        var nextId = _activePlaylist.MoveNext();
        if (nextId.HasValue)
            return await PlayTrackByIdAsync(nextId.Value);

        await _audioService.StopAsync();
        return Result.Failure("End of queue.");
    }

    public async Task<Result> PreviousTrackAsync()
    {
        var prevId = _activePlaylist.MovePrevious();
        if (prevId.HasValue)
            return await PlayTrackByIdAsync(prevId.Value);

        return Result.Failure("Beginning of queue.");
    }

    public async Task<Result> TogglePlayStopAsync()
    {
        try
        {
            if (IsPlaying)
            {
                await _audioService.StopAsync();
                return Result.Success();
            }

            if (_activePlaylist.CurrentTrackId.HasValue)
            {
                return await PlayTrackByIdAsync(_activePlaylist.CurrentTrackId.Value);
            }

            return Result.Failure("No track selected.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result> RestartTrackAsync()
    {
        try
        {
            if (_activePlaylist.CurrentTrackId.HasValue)
            {
                await _audioService.SeekAsync(TimeSpan.Zero);
                await _audioService.ResumeAsync();
                return Result.Success();
            }
            return Result.Failure("No track selected.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result> StopAsync()
    {
        try
        {
            await _audioService.StopAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error: {ex.Message}");
        }
    }

    public Result ToggleRepeatMode()
    {
        _activePlaylist.RepeatMode = _activePlaylist.RepeatMode switch
        {
            RepeatMode.None => RepeatMode.One,
            RepeatMode.One => RepeatMode.All,
            RepeatMode.All => RepeatMode.None,
            _ => RepeatMode.None
        };
        return Result.Success();
    }

    public Result ToggleShuffle()
    {
        if (_activePlaylist.Count == 0)
            return Result.Failure("No tracks in queue to shuffle.");

        _activePlaylist.ToggleShuffle();
        return Result.Success();
    }

    public async Task<Result> JumpToAsync(int queueIndex)
    {
        try
        {
            _activePlaylist.JumpTo(queueIndex);
            if (_activePlaylist.CurrentTrackId.HasValue)
                return await PlayTrackByIdAsync(_activePlaylist.CurrentTrackId.Value);

            return Result.Failure("No track at that position.");
        }
        catch (ArgumentOutOfRangeException)
        {
            return Result.Failure("Invalid queue position.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<Track?>> GetCurrentTrackAsync()
    {
        try
        {
            var trackId = _activePlaylist.CurrentTrackId;
            if (!trackId.HasValue)
                return Result.Success<Track?>(null);

            var track = await _trackRepository.GetByIdAsync(trackId.Value);
            return Result.Success<Track?>(track);
        }
        catch (Exception ex)
        {
            return Result.Failure<Track?>($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<QueueItem>>> GetQueueItemsAsync(bool truncate = true)
    {
        try
        {
            var items = new List<QueueItem>();
            var playOrder = _activePlaylist.PlayOrder;
            var currentIndex = _activePlaylist.CurrentIndex;
            const int maxVisible = 9;

            int startIndex = 0;
            int endIndex = playOrder.Count;
            var shouldTruncate = truncate && playOrder.Count > maxVisible + 1;

            if (shouldTruncate)
            {
                startIndex = Math.Max(0, currentIndex - 4);
                endIndex = startIndex + maxVisible;
                if (endIndex > playOrder.Count)
                {
                    endIndex = playOrder.Count;
                    startIndex = Math.Max(0, endIndex - maxVisible);
                }
            }

            for (int i = startIndex; i < endIndex; i++)
            {
                var track = await _trackRepository.GetByIdAsync(playOrder[i]);
                items.Add(new QueueItem
                {
                    Index = i,
                    Title = track?.DisplayTitle ?? "Unknown",
                    Artist = track?.DisplayArtist ?? "",
                    Duration = track?.Duration,
                    IsCurrent = i == currentIndex
                });
            }

            return Result.Success(items);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<QueueItem>>($"Error: {ex.Message}");
        }
    }

    public void SetQueue(IReadOnlyList<int> trackIds)
    {
        _activePlaylist.SetQueue(trackIds);
    }

    public void AppendToQueue(List<int> trackIds)
    {
        if (_activePlaylist.Count == 0)
        {
            _activePlaylist.SetQueue(trackIds);
            return;
        }

        var currentQueue = _activePlaylist.PlayOrder.ToList();
        var currentIdx = _activePlaylist.CurrentIndex;
        var wasShuffled = _activePlaylist.IsShuffled;

        foreach (var id in trackIds)
        {
            if (!currentQueue.Contains(id))
                currentQueue.Add(id);
        }

        _activePlaylist.SetQueue(currentQueue);
        if (currentIdx >= 0)
            _activePlaylist.JumpTo(currentIdx);
        if (wasShuffled)
            _activePlaylist.ToggleShuffle();
    }

    public void SubscribeToTrackEnd(Action onTrackEnded)
    {
        if (_playerEventSubscribed) return;
        _playerEventSubscribed = true;
        _audioService.Player.PlaybackCompleted += (s, e) => onTrackEnded();
    }

    public async Task<Result> AutoAdvanceAsync()
    {
        var nextId = _activePlaylist.MoveNext();
        if (nextId.HasValue)
            return await PlayTrackByIdAsync(nextId.Value);

        return Result.Failure("End of queue.");
    }
}
