using FluentAssertions;
using FsmpConsole;
using FsmpDataAcsses;
using FsmpLibrary.Models;
using FsmpLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FSMP.Tests.UI;

public class BrowseUITests : IDisposable
{
    private readonly FsmpDbContext _context;
    private readonly UnitOfWork _unitOfWork;
    private readonly Mock<IAudioService> _audioMock;

    public BrowseUITests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();

        _unitOfWork = new UnitOfWork(_context);
        _audioMock = new Mock<IAudioService>();
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
    }

    // --- Helpers ---

    private BrowseUI CreateBrowse(string inputLines)
    {
        return CreateBrowseWithOutput(inputLines).browse;
    }

    private (BrowseUI browse, StringWriter output) CreateBrowseWithOutput(string inputLines)
    {
        var input = new StringReader(inputLines);
        var output = new StringWriter();
        var browse = new BrowseUI(_unitOfWork, _audioMock.Object, input, output);
        return (browse, output);
    }

    private async Task<Artist> CreateArtistAsync(string name)
    {
        var artist = new Artist
        {
            Name = name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _unitOfWork.Artists.AddAsync(artist);
        await _unitOfWork.SaveAsync();
        return artist;
    }

    private async Task<Album> CreateAlbumAsync(string title, int artistId, int? year = null)
    {
        var album = new Album
        {
            Title = title,
            ArtistId = artistId,
            Year = year,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _unitOfWork.Albums.AddAsync(album);
        await _unitOfWork.SaveAsync();
        return album;
    }

    private async Task<Track> CreateTrackAsync(string title, int? albumId = null, int? artistId = null, TimeSpan? duration = null)
    {
        var track = new Track
        {
            Title = title,
            FilePath = $@"C:\Music\{title}.mp3",
            FileHash = Guid.NewGuid().ToString(),
            AlbumId = albumId,
            ArtistId = artistId,
            Duration = duration,
            ImportedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _unitOfWork.Tracks.AddAsync(track);
        await _unitOfWork.SaveAsync();
        return track;
    }

    // ========== Constructor Tests ==========

    [Fact]
    public void Constructor_WithNullUnitOfWork_ShouldThrow()
    {
        var act = () => new BrowseUI(null!, _audioMock.Object, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("unitOfWork");
    }

    [Fact]
    public void Constructor_WithNullAudioService_ShouldThrow()
    {
        var act = () => new BrowseUI(_unitOfWork, null!, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("audioService");
    }

    [Fact]
    public void Constructor_WithNullInput_ShouldThrow()
    {
        var act = () => new BrowseUI(_unitOfWork, _audioMock.Object, null!, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("input");
    }

    [Fact]
    public void Constructor_WithNullOutput_ShouldThrow()
    {
        var act = () => new BrowseUI(_unitOfWork, _audioMock.Object, TextReader.Null, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("output");
    }

    // ========== DisplayArtistsAsync Tests ==========

    [Fact]
    public async Task DisplayArtistsAsync_NoArtists_ShouldShowMessage()
    {
        var (browse, output) = CreateBrowseWithOutput("");

        await browse.DisplayArtistsAsync();

        output.ToString().Should().Contain("No artists in library");
    }

    [Fact]
    public async Task DisplayArtistsAsync_ShouldListAllArtists()
    {
        await CreateArtistAsync("Bonobo");
        await CreateArtistAsync("AC/DC");

        var (browse, output) = CreateBrowseWithOutput("0\n");

        await browse.DisplayArtistsAsync();

        var text = output.ToString();
        text.Should().Contain("Artists");
        text.Should().Contain("Bonobo");
        text.Should().Contain("AC/DC");
    }

    [Fact]
    public async Task DisplayArtistsAsync_BackOption_ShouldReturn()
    {
        await CreateArtistAsync("Bonobo");

        var (browse, output) = CreateBrowseWithOutput("0\n");

        await browse.DisplayArtistsAsync();

        var text = output.ToString();
        text.Should().Contain("0) Back");
        text.Should().NotContain("Albums by");
    }

    [Fact]
    public async Task DisplayArtistsAsync_InvalidSelection_ShouldShowError()
    {
        await CreateArtistAsync("Bonobo");

        var (browse, output) = CreateBrowseWithOutput("999\n");

        await browse.DisplayArtistsAsync();

        output.ToString().Should().Contain("Invalid selection");
    }

    [Fact]
    public async Task DisplayArtistsAsync_SelectArtist_ShouldNavigateToAlbums()
    {
        var artist = await CreateArtistAsync("Bonobo");
        await CreateAlbumAsync("Migration", artist.ArtistId, 2017);

        // Select artist 1, then back from albums
        var (browse, output) = CreateBrowseWithOutput("1\n0\n");

        await browse.DisplayArtistsAsync();

        var text = output.ToString();
        text.Should().Contain("Albums by Bonobo");
        text.Should().Contain("Migration");
    }

    // ========== DisplayAlbumsByArtistAsync Tests ==========

    [Fact]
    public async Task DisplayAlbumsByArtistAsync_ArtistNotFound_ShouldShowMessage()
    {
        var (browse, output) = CreateBrowseWithOutput("");

        await browse.DisplayAlbumsByArtistAsync(9999);

        output.ToString().Should().Contain("Artist not found");
    }

    [Fact]
    public async Task DisplayAlbumsByArtistAsync_NoAlbums_ShouldShowMessage()
    {
        var artist = await CreateArtistAsync("Unknown");

        var (browse, output) = CreateBrowseWithOutput("");

        await browse.DisplayAlbumsByArtistAsync(artist.ArtistId);

        output.ToString().Should().Contain("No albums found");
    }

    [Fact]
    public async Task DisplayAlbumsByArtistAsync_ShouldListAlbums()
    {
        var artist = await CreateArtistAsync("Bonobo");
        await CreateAlbumAsync("Migration", artist.ArtistId, 2017);
        await CreateAlbumAsync("The North Borders", artist.ArtistId, 2013);

        var (browse, output) = CreateBrowseWithOutput("0\n");

        await browse.DisplayAlbumsByArtistAsync(artist.ArtistId);

        var text = output.ToString();
        text.Should().Contain("Albums by Bonobo");
        text.Should().Contain("Migration (2017)");
        text.Should().Contain("The North Borders (2013)");
    }

    [Fact]
    public async Task DisplayAlbumsByArtistAsync_AlbumWithNoYear_ShouldOmitYear()
    {
        var artist = await CreateArtistAsync("Bonobo");
        await CreateAlbumAsync("Singles", artist.ArtistId);

        var (browse, output) = CreateBrowseWithOutput("0\n");

        await browse.DisplayAlbumsByArtistAsync(artist.ArtistId);

        var text = output.ToString();
        text.Should().Contain("Singles");
        text.Should().NotContain("Singles (");
    }

    [Fact]
    public async Task DisplayAlbumsByArtistAsync_InvalidSelection_ShouldShowError()
    {
        var artist = await CreateArtistAsync("Bonobo");
        await CreateAlbumAsync("Migration", artist.ArtistId);

        var (browse, output) = CreateBrowseWithOutput("999\n");

        await browse.DisplayAlbumsByArtistAsync(artist.ArtistId);

        output.ToString().Should().Contain("Invalid selection");
    }

    [Fact]
    public async Task DisplayAlbumsByArtistAsync_SelectAlbum_ShouldNavigateToTracks()
    {
        var artist = await CreateArtistAsync("Bonobo");
        var album = await CreateAlbumAsync("Migration", artist.ArtistId);
        await CreateTrackAsync("Kerala", album.AlbumId, artist.ArtistId);

        // Select album 1, then back from tracks
        var (browse, output) = CreateBrowseWithOutput("1\n0\n");

        await browse.DisplayAlbumsByArtistAsync(artist.ArtistId);

        var text = output.ToString();
        text.Should().Contain("Migration");
        text.Should().Contain("Kerala");
    }

    // ========== DisplayTracksByAlbumAsync Tests ==========

    [Fact]
    public async Task DisplayTracksByAlbumAsync_AlbumNotFound_ShouldShowMessage()
    {
        var (browse, output) = CreateBrowseWithOutput("");

        await browse.DisplayTracksByAlbumAsync(9999);

        output.ToString().Should().Contain("Album not found");
    }

    [Fact]
    public async Task DisplayTracksByAlbumAsync_NoTracks_ShouldShowMessage()
    {
        var artist = await CreateArtistAsync("Bonobo");
        var album = await CreateAlbumAsync("Empty Album", artist.ArtistId);

        var (browse, output) = CreateBrowseWithOutput("");

        await browse.DisplayTracksByAlbumAsync(album.AlbumId);

        output.ToString().Should().Contain("No tracks in this album");
    }

    [Fact]
    public async Task DisplayTracksByAlbumAsync_ShouldListTracks()
    {
        var artist = await CreateArtistAsync("Bonobo");
        var album = await CreateAlbumAsync("Migration", artist.ArtistId);
        await CreateTrackAsync("Kerala", album.AlbumId, artist.ArtistId);
        await CreateTrackAsync("Bambro Koyo Ganda", album.AlbumId, artist.ArtistId);

        var (browse, output) = CreateBrowseWithOutput("0\n");

        await browse.DisplayTracksByAlbumAsync(album.AlbumId);

        var text = output.ToString();
        text.Should().Contain("Migration");
        text.Should().Contain("Kerala");
        text.Should().Contain("Bambro Koyo Ganda");
    }

    [Fact]
    public async Task DisplayTracksByAlbumAsync_ShouldShowDuration()
    {
        var artist = await CreateArtistAsync("Bonobo");
        var album = await CreateAlbumAsync("Migration", artist.ArtistId);
        await CreateTrackAsync("Kerala", album.AlbumId, artist.ArtistId, TimeSpan.FromSeconds(195));

        var (browse, output) = CreateBrowseWithOutput("0\n");

        await browse.DisplayTracksByAlbumAsync(album.AlbumId);

        output.ToString().Should().Contain("[3:15]");
    }

    [Fact]
    public async Task DisplayTracksByAlbumAsync_InvalidSelection_ShouldShowError()
    {
        var artist = await CreateArtistAsync("Bonobo");
        var album = await CreateAlbumAsync("Migration", artist.ArtistId);
        await CreateTrackAsync("Kerala", album.AlbumId, artist.ArtistId);

        var (browse, output) = CreateBrowseWithOutput("999\n");

        await browse.DisplayTracksByAlbumAsync(album.AlbumId);

        output.ToString().Should().Contain("Invalid selection");
    }

    [Fact]
    public async Task DisplayTracksByAlbumAsync_SelectTrack_ShouldCallPlayTrackAsync()
    {
        var artist = await CreateArtistAsync("Bonobo");
        var album = await CreateAlbumAsync("Migration", artist.ArtistId);
        var track = await CreateTrackAsync("Kerala", album.AlbumId, artist.ArtistId);

        var (browse, output) = CreateBrowseWithOutput("1\n");

        await browse.DisplayTracksByAlbumAsync(album.AlbumId);

        _audioMock.Verify(a => a.PlayTrackAsync(
            It.Is<Track>(t => t.TrackId == track.TrackId),
            It.IsAny<CancellationToken>()), Times.Once);
        output.ToString().Should().Contain("Now Playing");
    }

    // ========== PlayTrackAsync Tests ==========

    [Fact]
    public async Task PlayTrackAsync_TrackNotFound_ShouldShowMessage()
    {
        var (browse, output) = CreateBrowseWithOutput("");

        await browse.PlayTrackAsync(9999);

        output.ToString().Should().Contain("Track not found");
    }

    [Fact]
    public async Task PlayTrackAsync_ShouldDisplayNowPlaying()
    {
        var artist = await CreateArtistAsync("Bonobo");
        var track = await CreateTrackAsync("Kerala", artistId: artist.ArtistId);

        var (browse, output) = CreateBrowseWithOutput("");

        await browse.PlayTrackAsync(track.TrackId);

        var text = output.ToString();
        text.Should().Contain("Now Playing");
        text.Should().Contain("Kerala");
    }

    [Fact]
    public async Task PlayTrackAsync_ShouldCallAudioService()
    {
        var track = await CreateTrackAsync("Kerala");

        var (browse, output) = CreateBrowseWithOutput("");

        await browse.PlayTrackAsync(track.TrackId);

        _audioMock.Verify(a => a.PlayTrackAsync(
            It.Is<Track>(t => t.TrackId == track.TrackId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PlayTrackAsync_WithDurationAndBitRate_ShouldDisplayDetails()
    {
        var track = new Track
        {
            Title = "Kerala",
            FilePath = @"C:\Music\Kerala.mp3",
            FileHash = Guid.NewGuid().ToString(),
            Duration = TimeSpan.FromSeconds(195),
            BitRate = 320,
            PlayCount = 5,
            Rating = 4,
            ImportedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _unitOfWork.Tracks.AddAsync(track);
        await _unitOfWork.SaveAsync();

        var (browse, output) = CreateBrowseWithOutput("");

        await browse.PlayTrackAsync(track.TrackId);

        var text = output.ToString();
        text.Should().Contain("3:15");
        text.Should().Contain("320kbps");
        text.Should().Contain("Plays:  5");
        text.Should().Contain("Rating: 4/5");
    }

    // ========== RunAsync Integration Test ==========

    [Fact]
    public async Task RunAsync_ShouldDelegateToDisplayArtistsAsync()
    {
        var (browse, output) = CreateBrowseWithOutput("");

        await browse.RunAsync();

        output.ToString().Should().Contain("No artists in library");
    }
}
