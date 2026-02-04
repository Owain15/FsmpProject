using FluentAssertions;
using FsmpLibrary.Interfaces;
using FsmpLibrary.Models;
using FsmpLibrary.Services;
using FSMP.Tests.TestHelpers;
using Moq;

namespace FSMP.Tests.Services;

public class AudioServiceTests
{
    private readonly MockAudioPlayer _mockPlayer;
    private readonly MockAudioPlayerFactory _mockFactory;
    private readonly AudioService _service;

    public AudioServiceTests()
    {
        _mockPlayer = new MockAudioPlayer();
        _mockFactory = new MockAudioPlayerFactory(_mockPlayer);
        _service = new AudioService(_mockFactory);
    }

    [Fact]
    public void Constructor_WithNullFactory_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new AudioService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("playerFactory");
    }

    [Fact]
    public void Player_ShouldCreatePlayerOnFirstAccess()
    {
        // Act
        var player = _service.Player;

        // Assert
        player.Should().Be(_mockPlayer);
    }

    [Fact]
    public void Player_ShouldReturnSameInstanceOnSubsequentAccess()
    {
        // Act
        var player1 = _service.Player;
        var player2 = _service.Player;

        // Assert
        player1.Should().BeSameAs(player2);
    }

    [Fact]
    public async Task PlayTrackAsync_WithNullTrack_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => _service.PlayTrackAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task PlayTrackAsync_WithInvalidFilePath_ShouldThrowArgumentException(string filePath)
    {
        // Arrange
        var track = new Track { FilePath = filePath };

        // Act
        var act = () => _service.PlayTrackAsync(track);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("track");
    }

    [Fact]
    public async Task PlayTrackAsync_ShouldLoadAndPlayTrack()
    {
        // Arrange
        var track = new Track
        {
            TrackId = 1,
            Title = "Test Song",
            FilePath = @"C:\Music\test.mp3"
        };

        // Act
        await _service.PlayTrackAsync(track);

        // Assert
        _mockPlayer.LoadedFilePath.Should().Be(track.FilePath);
        _mockPlayer.LoadCallCount.Should().Be(1);
        _mockPlayer.PlayCallCount.Should().Be(1);
        _service.CurrentTrack.Should().Be(track);
    }

    [Fact]
    public async Task PlayTrackAsync_ShouldRaiseTrackChangedEvent()
    {
        // Arrange
        var track = new Track
        {
            TrackId = 1,
            FilePath = @"C:\Music\test.mp3"
        };
        TrackChangedEventArgs? receivedArgs = null;
        _service.TrackChanged += (s, e) => receivedArgs = e;

        // Act
        await _service.PlayTrackAsync(track);

        // Assert
        receivedArgs.Should().NotBeNull();
        receivedArgs!.PreviousTrack.Should().BeNull();
        receivedArgs.NewTrack.Should().Be(track);
    }

    [Fact]
    public async Task PlayTrackAsync_WhenChangingTracks_ShouldProvidePreviousTrack()
    {
        // Arrange
        var track1 = new Track { TrackId = 1, FilePath = @"C:\Music\test1.mp3" };
        var track2 = new Track { TrackId = 2, FilePath = @"C:\Music\test2.mp3" };
        TrackChangedEventArgs? receivedArgs = null;
        _service.TrackChanged += (s, e) => receivedArgs = e;

        // Act
        await _service.PlayTrackAsync(track1);
        await _service.PlayTrackAsync(track2);

        // Assert
        receivedArgs.Should().NotBeNull();
        receivedArgs!.PreviousTrack.Should().Be(track1);
        receivedArgs.NewTrack.Should().Be(track2);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task PlayFileAsync_WithInvalidPath_ShouldThrowArgumentException(string filePath)
    {
        // Act
        var act = () => _service.PlayFileAsync(filePath);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("filePath");
    }

    [Fact]
    public async Task PlayFileAsync_ShouldLoadAndPlayFile()
    {
        // Arrange
        var filePath = @"C:\Music\test.wav";

        // Act
        await _service.PlayFileAsync(filePath);

        // Assert
        _mockPlayer.LoadedFilePath.Should().Be(filePath);
        _mockPlayer.LoadCallCount.Should().Be(1);
        _mockPlayer.PlayCallCount.Should().Be(1);
        _service.CurrentTrack.Should().BeNull();
    }

    [Fact]
    public async Task PlayFileAsync_ShouldRaiseTrackChangedEventWithNullTrack()
    {
        // Arrange
        TrackChangedEventArgs? receivedArgs = null;
        _service.TrackChanged += (s, e) => receivedArgs = e;

        // Act
        await _service.PlayFileAsync(@"C:\Music\test.wav");

        // Assert
        receivedArgs.Should().NotBeNull();
        receivedArgs!.NewTrack.Should().BeNull();
    }

    [Fact]
    public async Task PauseAsync_ShouldCallPlayerPause()
    {
        // Act
        await _service.PauseAsync();

        // Assert
        _mockPlayer.PauseCallCount.Should().Be(1);
    }

    [Fact]
    public async Task ResumeAsync_ShouldCallPlayerPlay()
    {
        // Act
        await _service.ResumeAsync();

        // Assert
        _mockPlayer.PlayCallCount.Should().Be(1);
    }

    [Fact]
    public async Task StopAsync_ShouldCallPlayerStop()
    {
        // Act
        await _service.StopAsync();

        // Assert
        _mockPlayer.StopCallCount.Should().Be(1);
    }

    [Fact]
    public async Task SeekAsync_ShouldCallPlayerSeek()
    {
        // Arrange
        var position = TimeSpan.FromSeconds(30);

        // Act
        await _service.SeekAsync(position);

        // Assert
        _mockPlayer.SeekCallCount.Should().Be(1);
        _mockPlayer.Position.Should().Be(position);
    }

    [Fact]
    public void Volume_ShouldGetAndSetPlayerVolume()
    {
        // Act
        _service.Volume = 0.5f;

        // Assert
        _service.Volume.Should().Be(0.5f);
        _mockPlayer.Volume.Should().Be(0.5f);
    }

    [Fact]
    public void IsMuted_ShouldGetAndSetPlayerMute()
    {
        // Act
        _service.IsMuted = true;

        // Assert
        _service.IsMuted.Should().BeTrue();
        _mockPlayer.IsMuted.Should().BeTrue();
    }

    [Fact]
    public void Dispose_ShouldDisposePlayer()
    {
        // Arrange
        _ = _service.Player; // Force player creation

        // Act
        _service.Dispose();

        // Assert
        _mockPlayer.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_WhenPlayerNotCreated_ShouldNotThrow()
    {
        // Act
        var act = () => _service.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_WhenCalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        _ = _service.Player;

        // Act
        var act = () =>
        {
            _service.Dispose();
            _service.Dispose();
        };

        // Assert
        act.Should().NotThrow();
    }
}
