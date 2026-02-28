using FSMP.Core.Models;

namespace FSMP.Core.Interfaces;

public interface IPlaybackController
{
    bool IsPlaying { get; }
    RepeatMode RepeatMode { get; }
    bool IsShuffled { get; }
    int QueueCount { get; }
    int CurrentIndex { get; }

    Task<Result> PlayTrackByIdAsync(int trackId);
    Task<Result> NextTrackAsync();
    Task<Result> PreviousTrackAsync();
    Task<Result> TogglePlayStopAsync();
    Task<Result> RestartTrackAsync();
    Task<Result> StopAsync();
    Result ToggleRepeatMode();
    Result ToggleShuffle();
    Task<Result> JumpToAsync(int queueIndex);

    Task<Result<Track?>> GetCurrentTrackAsync();
    Task<Result<List<QueueItem>>> GetQueueItemsAsync(bool truncate = true);

    void SubscribeToTrackEnd(Action onTrackEnded);
    Task<Result> AutoAdvanceAsync();
}
