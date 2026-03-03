using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;
using FSMP.Core.ViewModels;
using FluentAssertions;
using Moq;

namespace FSMP.Tests.ViewModels;

public class LibraryBrowseViewModelTests
{
    private readonly Mock<ILibraryBrowser> _browserMock;
    private readonly Mock<IPlaybackController> _playbackMock;
    private readonly LibraryBrowseViewModel _vm;

    public LibraryBrowseViewModelTests()
    {
        _browserMock = new Mock<ILibraryBrowser>();
        _playbackMock = new Mock<IPlaybackController>();
        _vm = new LibraryBrowseViewModel(_browserMock.Object, _playbackMock.Object);
    }

    [Fact]
    public async Task LoadAsync_PopulatesArtists()
    {
        var artists = new List<Artist>
        {
            new() { ArtistId = 1, Name = "Artist A" },
            new() { ArtistId = 2, Name = "Artist B" }
        };
        _browserMock.Setup(b => b.GetAllArtistsAsync())
            .ReturnsAsync(Result.Success(artists));

        await _vm.LoadAsync();

        _vm.Items.Should().HaveCount(2);
        _vm.BrowseLevel.Should().Be(BrowseLevel.Artists);
        _vm.PageTitle.Should().Be("Artists");
        _vm.CanGoBack.Should().BeFalse();
    }

    [Fact]
    public async Task SelectArtist_LoadsAlbums()
    {
        var artist = new Artist { ArtistId = 1, Name = "Test Artist" };
        var albums = new List<Album>
        {
            new() { AlbumId = 10, Title = "Album 1" },
            new() { AlbumId = 11, Title = "Album 2" }
        };
        _browserMock.Setup(b => b.GetAlbumsByArtistAsync(1))
            .ReturnsAsync(Result.Success(albums));

        _vm.SelectItemCommand.Execute(artist);
        await Task.Delay(50); // allow async to complete

        _vm.Items.Should().HaveCount(2);
        _vm.BrowseLevel.Should().Be(BrowseLevel.Albums);
        _vm.PageTitle.Should().Be("Test Artist");
        _vm.CanGoBack.Should().BeTrue();
    }

    [Fact]
    public async Task SelectAlbum_LoadsTracks()
    {
        var album = new Album
        {
            AlbumId = 10,
            Title = "Test Album"
        };
        var albumWithTracks = new Album
        {
            AlbumId = 10,
            Title = "Test Album",
            Tracks = new List<Track>
            {
                new() { TrackId = 100, Title = "Track 1" },
                new() { TrackId = 101, Title = "Track 2" }
            }
        };
        _browserMock.Setup(b => b.GetAlbumWithTracksAsync(10))
            .ReturnsAsync(Result.Success<Album?>(albumWithTracks));

        _vm.SelectItemCommand.Execute(album);
        await Task.Delay(50);

        _vm.Items.Should().HaveCount(2);
        _vm.BrowseLevel.Should().Be(BrowseLevel.Tracks);
        _vm.PageTitle.Should().Be("Test Album");
    }

    [Fact]
    public async Task PlayNowCommand_SetsQueueAndJumps()
    {
        // Setup: navigate to an album's tracks first
        var album = new Album { AlbumId = 10, Title = "Test Album" };
        var albumWithTracks = new Album
        {
            AlbumId = 10,
            Title = "Test Album",
            Tracks = new List<Track>
            {
                new() { TrackId = 100, Title = "Track 1" },
                new() { TrackId = 101, Title = "Track 2" }
            }
        };
        _browserMock.Setup(b => b.GetAlbumWithTracksAsync(10))
            .ReturnsAsync(Result.Success<Album?>(albumWithTracks));
        _playbackMock.Setup(p => p.JumpToAsync(It.IsAny<int>()))
            .ReturnsAsync(Result.Success());

        // Navigate to album tracks
        _vm.SelectItemCommand.Execute(album);
        await Task.Delay(50);

        // Play the second track
        var track = new Track { TrackId = 101, Title = "Track 2" };
        _vm.PlayNowCommand.Execute(track);
        await Task.Delay(50);

        _playbackMock.Verify(p => p.SetQueue(It.Is<IReadOnlyList<int>>(ids =>
            ids.Count == 2 && ids[0] == 100 && ids[1] == 101)), Times.Once);
        _playbackMock.Verify(p => p.JumpToAsync(1), Times.Once);
    }

    [Fact]
    public void AddToQueueCommand_AppendsTrack()
    {
        var track = new Track { TrackId = 42, Title = "Queued Track" };

        _vm.AddToQueueCommand.Execute(track);

        _playbackMock.Verify(p => p.AppendToQueue(
            It.Is<List<int>>(ids => ids.Count == 1 && ids[0] == 42)), Times.Once);
    }

    [Fact]
    public async Task GoBack_FromAlbums_ReturnsToArtists()
    {
        // Navigate to albums first
        var artist = new Artist { ArtistId = 1, Name = "Test Artist" };
        _browserMock.Setup(b => b.GetAlbumsByArtistAsync(1))
            .ReturnsAsync(Result.Success(new List<Album>()));
        _browserMock.Setup(b => b.GetAllArtistsAsync())
            .ReturnsAsync(Result.Success(new List<Artist> { artist }));

        _vm.SelectItemCommand.Execute(artist);
        await Task.Delay(50);
        _vm.BrowseLevel.Should().Be(BrowseLevel.Albums);

        _vm.GoBackCommand.Execute(null);
        await Task.Delay(50);

        _vm.BrowseLevel.Should().Be(BrowseLevel.Artists);
        _vm.PageTitle.Should().Be("Artists");
    }

    [Fact]
    public async Task GoBack_FromTracks_ReturnsToAlbums()
    {
        // Navigate to artist → album → tracks
        var artist = new Artist { ArtistId = 1, Name = "Test Artist" };
        var album = new Album { AlbumId = 10, Title = "Test Album" };
        var albumWithTracks = new Album
        {
            AlbumId = 10,
            Title = "Test Album",
            Tracks = new List<Track> { new() { TrackId = 100, Title = "Track 1" } }
        };

        _browserMock.Setup(b => b.GetAlbumsByArtistAsync(1))
            .ReturnsAsync(Result.Success(new List<Album> { album }));
        _browserMock.Setup(b => b.GetAlbumWithTracksAsync(10))
            .ReturnsAsync(Result.Success<Album?>(albumWithTracks));
        _browserMock.Setup(b => b.GetArtistByIdAsync(1))
            .ReturnsAsync(Result.Success<Artist?>(artist));

        _vm.SelectItemCommand.Execute(artist);
        await Task.Delay(50);

        _vm.SelectItemCommand.Execute(album);
        await Task.Delay(50);
        _vm.BrowseLevel.Should().Be(BrowseLevel.Tracks);

        _vm.GoBackCommand.Execute(null);
        await Task.Delay(50);

        _vm.BrowseLevel.Should().Be(BrowseLevel.Albums);
        _vm.PageTitle.Should().Be("Test Artist");
    }
}
