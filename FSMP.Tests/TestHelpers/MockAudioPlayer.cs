using FSMP.Core.Interfaces;
using FSMP.Core.Interfaces.EventArgs;

namespace FSMP.Tests.TestHelpers;

/// <summary>
/// Mock audio player for testing without actual audio playback.
/// </summary>
public class MockAudioPlayer : IAudioPlayer
{
    public PlaybackState State { get; private set; } = PlaybackState.Stopped;
    public TimeSpan Position { get; set; } = TimeSpan.Zero;
    public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(3);
    public float Volume { get; set; } = 0.75f;
    public bool IsMuted { get; set; }

    // Test tracking properties
    public string? LoadedFilePath { get; private set; }
    public int LoadCallCount { get; private set; }
    public int PlayCallCount { get; private set; }
    public int PauseCallCount { get; private set; }
    public int StopCallCount { get; private set; }
    public int SeekCallCount { get; private set; }
    public bool IsDisposed { get; private set; }

    // Configuration for test scenarios
    public bool ShouldThrowOnLoad { get; set; }
    public bool ShouldThrowOnPlay { get; set; }

    public event EventHandler<PlaybackStateChangedEventArgs>? StateChanged;
    public event EventHandler<PlaybackCompletedEventArgs>? PlaybackCompleted;
    public event EventHandler<PlaybackErrorEventArgs>? PlaybackError;
    public event EventHandler<PositionChangedEventArgs>? PositionChanged;

    public Task LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (ShouldThrowOnLoad)
            throw new FileNotFoundException("Test exception", filePath);

        LoadedFilePath = filePath;
        LoadCallCount++;
        return Task.CompletedTask;
    }

    public Task PlayAsync(CancellationToken cancellationToken = default)
    {
        if (ShouldThrowOnPlay)
            throw new InvalidOperationException("Test exception");

        PlayCallCount++;
        SetState(PlaybackState.Playing);
        return Task.CompletedTask;
    }

    public Task PauseAsync()
    {
        PauseCallCount++;
        SetState(PlaybackState.Paused);
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        StopCallCount++;
        SetState(PlaybackState.Stopped);
        return Task.CompletedTask;
    }

    public Task SeekAsync(TimeSpan position)
    {
        SeekCallCount++;
        Position = position;
        return Task.CompletedTask;
    }

    // Test helper methods
    public void SimulatePlaybackCompleted()
    {
        SetState(PlaybackState.Stopped);
        PlaybackCompleted?.Invoke(this, new PlaybackCompletedEventArgs
        {
            FilePath = LoadedFilePath ?? string.Empty,
            CompletedSuccessfully = true
        });
    }

    public void SimulateError(string message)
    {
        SetState(PlaybackState.Error);
        PlaybackError?.Invoke(this, new PlaybackErrorEventArgs
        {
            FilePath = LoadedFilePath ?? string.Empty,
            ErrorMessage = message
        });
    }

    public void SimulatePositionChanged(TimeSpan position)
    {
        Position = position;
        PositionChanged?.Invoke(this, new PositionChangedEventArgs
        {
            Position = position,
            Duration = Duration
        });
    }

    public void SetState(PlaybackState newState)
    {
        var oldState = State;
        State = newState;
        StateChanged?.Invoke(this, new PlaybackStateChangedEventArgs
        {
            OldState = oldState,
            NewState = newState
        });
    }

    public void Dispose()
    {
        IsDisposed = true;
    }
}
