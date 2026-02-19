using FSMP.Core.Audio;

namespace FSMP.Tests.TestHelpers;

/// <summary>
/// Mock adapter for testing LibVlcAudioPlayer without real LibVLC.
/// </summary>
public class MockMediaPlayerAdapter : IMediaPlayerAdapter
{
    // Call tracking
    public int PlayCallCount { get; private set; }
    public int PauseCallCount { get; private set; }
    public int StopCallCount { get; private set; }
    public int LoadCallCount { get; private set; }
    public int SetMediaCallCount { get; private set; }
    public int DisposeCurrentMediaCallCount { get; private set; }
    public string? LastLoadedFilePath { get; private set; }
    public bool IsDisposed { get; private set; }

    // Configurable behavior
    public bool ShouldThrowOnLoad { get; set; }
    public bool ShouldThrowOnPlay { get; set; }

    // Configurable state
    public bool HasMedia { get; set; }
    public long TimeMs { get; set; }
    public long DurationMs { get; set; } = 180000; // 3 minutes default
    public int Volume { get; set; } = 75;
    public bool Mute { get; set; }

    // Events
    public event EventHandler? Playing;
    public event EventHandler? Paused;
    public event EventHandler? Stopped;
    public event EventHandler? EndReached;
    public event EventHandler? EncounteredError;
    public event EventHandler<long>? TimeChanged;

    public void Play()
    {
        if (ShouldThrowOnPlay)
            throw new InvalidOperationException("Test exception");
        PlayCallCount++;
    }

    public void Pause()
    {
        PauseCallCount++;
    }

    public void Stop()
    {
        StopCallCount++;
    }

    public Task LoadMediaAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (ShouldThrowOnLoad)
            throw new InvalidOperationException("Test exception");
        LoadCallCount++;
        LastLoadedFilePath = filePath;
        HasMedia = true;
        return Task.CompletedTask;
    }

    public void SetMedia()
    {
        SetMediaCallCount++;
    }

    public void DisposeCurrentMedia()
    {
        DisposeCurrentMediaCallCount++;
        HasMedia = false;
    }

    // Simulate event methods
    public void SimulatePlaying() => Playing?.Invoke(this, EventArgs.Empty);
    public void SimulatePaused() => Paused?.Invoke(this, EventArgs.Empty);
    public void SimulateStopped() => Stopped?.Invoke(this, EventArgs.Empty);
    public void SimulateEndReached() => EndReached?.Invoke(this, EventArgs.Empty);
    public void SimulateError() => EncounteredError?.Invoke(this, EventArgs.Empty);
    public void SimulateTimeChanged(long timeMs) => TimeChanged?.Invoke(this, timeMs);

    public void Dispose()
    {
        IsDisposed = true;
    }
}