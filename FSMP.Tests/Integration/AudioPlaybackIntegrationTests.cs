using FluentAssertions;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;
using FsmpLibrary.Audio;
using FsmpLibrary.Services;
using FSMP.Tests.TestHelpers;

namespace FSMP.Tests.Integration;

/// <summary>
/// Integration tests exercising the full audio playback chain:
/// AudioService → LibVlcAudioPlayer → LibVlcMediaPlayerAdapter → LibVLC.
/// Uses event flags + Task.Delay polling to avoid deadlocks with LibVLC's event thread.
/// </summary>
[Trait("Category", "Integration")]
public class AudioPlaybackIntegrationTests : IDisposable
{
    private static readonly string[] TestVlcOptions = ["--quiet"];

    private readonly string _tempDir;
    private readonly string _testWavPath;

    public AudioPlaybackIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "FSMP_PlaybackIntegration", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _testWavPath = Path.Combine(_tempDir, "test.wav");
        WavGenerator.CreateTestWav(_testWavPath, durationSeconds: 0.5);
    }

    public void Dispose()
    {
        for (int i = 0; i < 3; i++)
        {
            try
            {
                if (Directory.Exists(_tempDir))
                    Directory.Delete(_tempDir, recursive: true);
                return;
            }
            catch (IOException)
            {
                Thread.Sleep(100);
            }
        }
    }

    private static async Task WaitForFlag(Func<bool> flag, int timeoutMs = 5000, string message = "Flag should become true")
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!flag() && DateTime.UtcNow < deadline)
            await Task.Delay(50);
        flag().Should().BeTrue(message);
    }

    private static AudioService CreateTestAudioService()
    {
        return new AudioService(new LibVlcAudioPlayerFactory(
            () => new LibVlcMediaPlayerAdapter(TestVlcOptions)));
    }

    [Fact]
    public async Task PlayFileAsync_RealWav_PlayerReachesPlayingState()
    {
        using var service = CreateTestAudioService();

        bool reachedPlaying = false;
        service.Player.StateChanged += (_, args) =>
        {
            if (args.NewState == PlaybackState.Playing)
                reachedPlaying = true;
        };

        await service.PlayFileAsync(_testWavPath);

        await WaitForFlag(() => reachedPlaying, message: "Player should reach Playing state");

        await service.StopAsync();
    }

    [Fact]
    public async Task PlayTrackAsync_RealWav_PlaysAndSetsCurrentTrack()
    {
        using var service = CreateTestAudioService();
        var track = new Track
        {
            Title = "Integration Test Track",
            FilePath = _testWavPath,
            FileHash = "test-hash",
            ImportedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        bool reachedPlaying = false;
        service.Player.StateChanged += (_, args) =>
        {
            if (args.NewState == PlaybackState.Playing)
                reachedPlaying = true;
        };

        await service.PlayTrackAsync(track);

        service.CurrentTrack.Should().BeSameAs(track);

        await WaitForFlag(() => reachedPlaying, message: "Player should reach Playing state");

        await service.StopAsync();
    }

    [Fact]
    public async Task StopAsync_AfterPlaying_ReachesStopped()
    {
        using var service = CreateTestAudioService();

        bool reachedPlaying = false;
        bool reachedStopped = false;
        service.Player.StateChanged += (_, args) =>
        {
            if (args.NewState == PlaybackState.Playing)
                reachedPlaying = true;
            if (args.NewState == PlaybackState.Stopped && reachedPlaying)
                reachedStopped = true;
        };

        await service.PlayFileAsync(_testWavPath);
        await WaitForFlag(() => reachedPlaying);

        await service.StopAsync();

        await WaitForFlag(() => reachedStopped, message: "Player should reach Stopped state after StopAsync");
    }

    [Fact]
    public async Task PlaybackCompleted_ShortWav_FiresEvent()
    {
        var shortWavPath = Path.Combine(_tempDir, "short.wav");
        WavGenerator.CreateTestWav(shortWavPath, durationSeconds: 0.1);

        using var service = CreateTestAudioService();

        bool completed = false;
        service.Player.PlaybackCompleted += (_, args) =>
        {
            if (args.CompletedSuccessfully)
                completed = true;
        };

        await service.PlayFileAsync(shortWavPath);

        await WaitForFlag(() => completed, timeoutMs: 10000,
            message: "PlaybackCompleted should fire when short WAV finishes");
    }
}
