using FSMP.Core.Interfaces;
using FSMP.Core.Models;
using FsmpLibrary.Services;
using Moq;
using FluentAssertions;

namespace FSMP.Tests.Services;

public class LibraryBrowserTests
{
    private readonly Mock<IArtistRepository> _artistRepoMock;
    private readonly Mock<IAlbumRepository> _albumRepoMock;
    private readonly Mock<ITrackRepository> _trackRepoMock;
    private readonly LibraryBrowser _browser;

    public LibraryBrowserTests()
    {
        _artistRepoMock = new Mock<IArtistRepository>();
        _albumRepoMock = new Mock<IAlbumRepository>();
        _trackRepoMock = new Mock<ITrackRepository>();
        _browser = new LibraryBrowser(_artistRepoMock.Object, _albumRepoMock.Object, _trackRepoMock.Object);
    }

    [Fact]
    public void Constructor_ThrowsOnNullArtistRepo()
    {
        var act = () => new LibraryBrowser(null!, _albumRepoMock.Object, _trackRepoMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullAlbumRepo()
    {
        var act = () => new LibraryBrowser(_artistRepoMock.Object, null!, _trackRepoMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullTrackRepo()
    {
        var act = () => new LibraryBrowser(_artistRepoMock.Object, _albumRepoMock.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetAllArtistsAsync_ReturnsArtists()
    {
        var artists = new List<Artist> { new() { ArtistId = 1, Name = "Artist1" } };
        _artistRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(artists);

        var result = await _browser.GetAllArtistsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllArtistsAsync_ReturnsFailure_OnException()
    {
        _artistRepoMock.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("db error"));

        var result = await _browser.GetAllArtistsAsync();

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("db error");
    }

    [Fact]
    public async Task GetArtistByIdAsync_ReturnsArtist()
    {
        var artist = new Artist { ArtistId = 1, Name = "Test" };
        _artistRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(artist);

        var result = await _browser.GetArtistByIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetAlbumsByArtistAsync_ReturnsAlbums()
    {
        var albums = new List<Album> { new() { AlbumId = 1, Title = "Album1" } };
        _albumRepoMock.Setup(r => r.GetByArtistAsync(1)).ReturnsAsync(albums);

        var result = await _browser.GetAlbumsByArtistAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAlbumWithTracksAsync_ReturnsAlbum()
    {
        var album = new Album { AlbumId = 1, Title = "Album1" };
        _albumRepoMock.Setup(r => r.GetWithTracksAsync(1)).ReturnsAsync(album);

        var result = await _browser.GetAlbumWithTracksAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Album1");
    }

    [Fact]
    public async Task GetTrackByIdAsync_ReturnsTrack()
    {
        var track = new Track { TrackId = 1, Title = "T1", FilePath = "t.mp3", FileHash = "a" };
        _trackRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(track);

        var result = await _browser.GetTrackByIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("T1");
    }

    [Fact]
    public async Task GetAllTrackIdsByArtistAsync_ReturnsTrackIds()
    {
        var artist = new Artist
        {
            ArtistId = 1,
            Name = "Test",
            Tracks = new List<Track>
            {
                new() { TrackId = 10, Title = "T1", FilePath = "t1.mp3", FileHash = "a" },
                new() { TrackId = 20, Title = "T2", FilePath = "t2.mp3", FileHash = "b" }
            }
        };
        _artistRepoMock.Setup(r => r.GetWithTracksAsync(1)).ReturnsAsync(artist);

        var result = await _browser.GetAllTrackIdsByArtistAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new[] { 10, 20 });
    }

    [Fact]
    public async Task GetAllTrackIdsByArtistAsync_ReturnsEmpty_WhenArtistNotFound()
    {
        _artistRepoMock.Setup(r => r.GetWithTracksAsync(99)).ReturnsAsync((Artist?)null);

        var result = await _browser.GetAllTrackIdsByArtistAsync(99);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllTrackIdsAsync_AggregatesAllArtistTracks()
    {
        var artists = new List<Artist>
        {
            new() { ArtistId = 1, Name = "A1" },
            new() { ArtistId = 2, Name = "A2" }
        };
        _artistRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(artists);
        _artistRepoMock.Setup(r => r.GetWithTracksAsync(1)).ReturnsAsync(new Artist
        {
            ArtistId = 1, Name = "A1",
            Tracks = new List<Track> { new() { TrackId = 1, Title = "T1", FilePath = "t1.mp3", FileHash = "a" } }
        });
        _artistRepoMock.Setup(r => r.GetWithTracksAsync(2)).ReturnsAsync(new Artist
        {
            ArtistId = 2, Name = "A2",
            Tracks = new List<Track> { new() { TrackId = 2, Title = "T2", FilePath = "t2.mp3", FileHash = "b" } }
        });

        var result = await _browser.GetAllTrackIdsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new[] { 1, 2 });
    }
}
