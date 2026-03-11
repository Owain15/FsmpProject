using FluentAssertions;
using FSMP.Platform.Windows.Audio;
using FSMP.Tests.TestHelpers;

namespace FSMP.Tests.Audio;

/// <summary>
/// Integration tests for LibVlcMediaPlayerAdapter using real LibVLC.
/// Uses event flags + Task.Delay polling to avoid deadlocks with LibVLC's event thread.
/// </summary>
[Trait("Category", "Integration")]
public class LibVlcMediaPlayerAdapterTests : IDisposable
{
    private static readonly string[] TestVlcOptions = ["--quiet"];

    private readonly string _tempDir;
    private readonly string _testWavPath;

    public LibVlcMediaPlayerAdapterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "FSMP_AdapterTests", Guid.NewGuid().ToString());
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

    [Fact]
    public void Constructor_CreatesAdapterWithoutThrowing()
    {
        using var adapter = new LibVlcMediaPlayerAdapter(TestVlcOptions);
        adapter.HasMedia.Should().BeFalse();
    }

    [Fact]
    public async Task LoadMediaAsync_WithRealWav_SetsHasMediaAndDuration()
    {
        using var adapter = new LibVlcMediaPlayerAdapter(TestVlcOptions);

        await adapter.LoadMediaAsync(_testWavPath);

        adapter.HasMedia.Should().BeTrue();
        adapter.DurationMs.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Play_AfterLoad_ReturnsTrueAndFiresPlayingEvent()
    {
        using var adapter = new LibVlcMediaPlayerAdapter(TestVlcOptions);
        await adapter.LoadMediaAsync(_testWavPath);

        bool playingFired = false;
        adapter.Playing += (_, _) => playingFired = true;

        var result = adapter.Play();
        result.Should().BeTrue();

        await WaitForFlag(() => playingFired, message: "Playing event should fire");

        adapter.Stop();
    }

    [Fact]
    public async Task Stop_AfterPlaying_FiresStoppedEvent()
    {
        using var adapter = new LibVlcMediaPlayerAdapter(TestVlcOptions);
        await adapter.LoadMediaAsync(_testWavPath);

        bool playingFired = false;
        adapter.Playing += (_, _) => playingFired = true;
        adapter.Play();
        await WaitForFlag(() => playingFired);

        bool stoppedFired = false;
        adapter.Stopped += (_, _) => stoppedFired = true;

        adapter.Stop();

        await WaitForFlag(() => stoppedFired, message: "Stopped event should fire");
    }

    [Fact]
    public async Task EndReached_ShortWav_FiresEndReachedEvent()
    {
        var shortWavPath = Path.Combine(_tempDir, "short.wav");
        WavGenerator.CreateTestWav(shortWavPath, durationSeconds: 0.1);

        using var adapter = new LibVlcMediaPlayerAdapter(TestVlcOptions);
        await adapter.LoadMediaAsync(shortWavPath);

        bool endReachedFired = false;
        adapter.EndReached += (_, _) => endReachedFired = true;

        adapter.Play();

        await WaitForFlag(() => endReachedFired, timeoutMs: 10000,
            message: "EndReached event should fire when short WAV finishes");
    }

    /// <summary>
    /// Repeatedly sets and checks a volume value until it sticks.
    /// LibVLC's volume setter is async internally and may not take effect immediately.
    /// </summary>
    private static async Task SetVolumeAndVerify(LibVlcMediaPlayerAdapter adapter, int target, int timeoutMs = 10000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            adapter.Volume = target;
            await Task.Delay(100);
            if (adapter.Volume == target)
                return;
        }
        adapter.Volume.Should().Be(target, $"Volume should be {target} after repeated attempts");
    }

    [Fact]
    public async Task Volume_GetSet_WorksWhilePlaying()
    {
        // Use a longer WAV to prevent playback ending mid-assertion
        var longWavPath = Path.Combine(_tempDir, "long.wav");
        WavGenerator.CreateTestWav(longWavPath, durationSeconds: 2.0);

        using var adapter = new LibVlcMediaPlayerAdapter(TestVlcOptions);
        await adapter.LoadMediaAsync(longWavPath);

        bool playingFired = false;
        adapter.Playing += (_, _) => playingFired = true;
        adapter.Play();
        await WaitForFlag(() => playingFired);

        await SetVolumeAndVerify(adapter, 50);
        await SetVolumeAndVerify(adapter, 100);

        adapter.Stop();
    }

    [Fact]
    public async Task DisposeCurrentMedia_ClearsHasMedia()
    {
        using var adapter = new LibVlcMediaPlayerAdapter(TestVlcOptions);
        await adapter.LoadMediaAsync(_testWavPath);
        adapter.HasMedia.Should().BeTrue();

        adapter.DisposeCurrentMedia();

        adapter.HasMedia.Should().BeFalse();
    }

    /// <summary>
    /// Repeatedly sets and checks mute until it sticks.
    /// </summary>
    private static async Task SetMuteAndVerify(LibVlcMediaPlayerAdapter adapter, bool target, int timeoutMs = 10000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            adapter.Mute = target;
            await Task.Delay(100);
            if (adapter.Mute == target)
                return;
        }
        adapter.Mute.Should().Be(target, $"Mute should be {target} after repeated attempts");
    }

    [Fact]
    public async Task Mute_GetSet_WorksWhilePlaying()
    {
        // Use a longer WAV to prevent playback ending mid-assertion
        var longWavPath = Path.Combine(_tempDir, "long.wav");
        WavGenerator.CreateTestWav(longWavPath, durationSeconds: 2.0);

        using var adapter = new LibVlcMediaPlayerAdapter(TestVlcOptions);
        await adapter.LoadMediaAsync(longWavPath);

        bool playingFired = false;
        adapter.Playing += (_, _) => playingFired = true;
        adapter.Play();
        await WaitForFlag(() => playingFired);

        await SetMuteAndVerify(adapter, true);
        await SetMuteAndVerify(adapter, false);

        adapter.Stop();
    }
}
