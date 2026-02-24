using FSMP.Core;
using FluentAssertions;
using FsmpConsole;
using FsmpDataAcsses;
using FSMP.Core.Models;
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

    private (BrowseUI browse, StringWriter output, ActivePlaylistService playlist) CreateBrowseWithOutputAndPlaylist(string inputLines)
    {
        var input = new StringReader(inputLines);
        var output = new StringWriter();
        var activePlaylist = new ActivePlaylistService();
        var browse = new BrowseUI(_unitOfWork, _audioMock.Object, activePlaylist, input, output);
        return (browse, output, activePlaylist);
    }

    private (BrowseUI browse, StringWriter output) CreateBrowseWithOutput(string inputLines)
    {
        var (browse, output, _) = CreateBrowseWithOutputAndPlaylist(inputLines);
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
        var act = () => new BrowseUI(null!, _audioMock.Object, new ActivePlaylistService(), TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("unitOfWork");
    }

    [Fact]
    public void Constructor_WithNullAudioService_ShouldThrow()
    {
        var act = () => new BrowseUI(_unitOfWork, null!, new ActivePlaylistService(), TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("audioService");
    }

    [Fact]
    public void Constructor_WithNullActivePlaylist_ShouldThrow()
    {
        var act = () => new BrowseUI(_unitOfWork, _audioMock.Object, null!, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("activePlaylist");
    }

    [Fact]
    public void Constructor_WithNullInput_ShouldThrow()
    {
        var act = () => new BrowseUI(_unitOfWork, _audioMock.Object, new ActivePlaylistService(), null!, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("input");
    }

    [Fact]
    public void Constructor_WithNullOutput_ShouldThrow()
    {
        var act = () => new BrowseUI(_unitOfWork, _audioMock.Object, new ActivePlaylistService(), TextReader.Null, null!);
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

        var (browse, output) = CreateBrowseWithOutput("999\n0\n");

        await browse.DisplayArtistsAsync();

        output.ToString().Should().Contain("Invalid selection");
    }

    [Fact]
    public async Task DisplayArtistsAsync_SelectArtist_ShouldNavigateToAlbums()
    {
        var artist = await CreateArtistAsync("Bonobo");
        await CreateAlbumAsync("Migration", artist.ArtistId, 2017);

        // Select artist 1, then back from albums, then back from artists
        var (browse, output) = CreateBrowseWithOutput("1\n0\n0\n");

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

        var (browse, output) = CreateBrowseWithOutput("999\n0\n");

        await browse.DisplayAlbumsByArtistAsync(artist.ArtistId);

        output.ToString().Should().Contain("Invalid selection");
    }

    [Fact]
    public async Task DisplayAlbumsByArtistAsync_SelectAlbum_ShouldNavigateToTracks()
    {
        var artist = await CreateArtistAsync("Bonobo");
        var album = await CreateAlbumAsync("Migration", artist.ArtistId);
        await CreateTrackAsync("Kerala", album.AlbumId, artist.ArtistId);

        // Select album 1, then back from tracks, then back from albums
        var (browse, output) = CreateBrowseWithOutput("1\n0\n0\n");

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

        var (browse, output) = CreateBrowseWithOutput("999\n0\n");

        await browse.DisplayTracksByAlbumAsync(album.AlbumId);

        output.ToString().Should().Contain("Invalid selection");
    }

    [Fact]
    public async Task DisplayTracksByAlbumAsync_SelectTrack_ShouldShowQueueOptions()
    {
        var artist = await CreateArtistAsync("Bonobo");
        var album = await CreateAlbumAsync("Migration", artist.ArtistId);
        var track = await CreateTrackAsync("Kerala", album.AlbumId, artist.ArtistId);

        // Select track 1, then choose Q to set as queue, then exit
        var (browse, output, playlist) = CreateBrowseWithOutputAndPlaylist("1\nQ\n0\n");

        await browse.DisplayTracksByAlbumAsync(album.AlbumId);

        var text = output.ToString();
        text.Should().Contain("Track Details");
        text.Should().Contain("Set queue: Kerala");
        playlist.Count.Should().Be(1);
        playlist.PlayOrder.Should().Contain(track.TrackId);
    }

    // ========== QueueTrackAsync Tests ==========

    [Fact]
    public async Task QueueTrackAsync_TrackNotFound_ShouldShowMessage()
    {
        var (browse, output) = CreateBrowseWithOutput("");

        await browse.QueueTrackAsync(9999);

        output.ToString().Should().Contain("Track not found");
    }

    [Fact]
    public async Task QueueTrackAsync_ShouldDisplayTrackDetails()
    {
        var artist = await CreateArtistAsync("Bonobo");
        var track = await CreateTrackAsync("Kerala", artistId: artist.ArtistId);

        var (browse, output) = CreateBrowseWithOutput("0\n");

        await browse.QueueTrackAsync(track.TrackId);

        var text = output.ToString();
        text.Should().Contain("Track Details");
        text.Should().Contain("Kerala");
    }

    [Fact]
    public async Task QueueTrackAsync_ChooseQ_ShouldSetQueue()
    {
        var track = await CreateTrackAsync("Kerala");

        var input = new StringReader("Q\n");
        var output = new StringWriter();
        var activePlaylist = new ActivePlaylistService();
        var browse = new BrowseUI(_unitOfWork, _audioMock.Object, activePlaylist, input, output);

        await browse.QueueTrackAsync(track.TrackId);

        activePlaylist.Count.Should().Be(1);
        activePlaylist.PlayOrder.Should().Contain(track.TrackId);
        output.ToString().Should().Contain("Set queue:");
    }

    [Fact]
    public async Task QueueTrackAsync_ChooseA_ShouldAddToQueue()
    {
        var track1 = await CreateTrackAsync("Kerala");
        var track2 = await CreateTrackAsync("Cirrus");

        var input = new StringReader("A\n");
        var output = new StringWriter();
        var activePlaylist = new ActivePlaylistService();
        activePlaylist.SetQueue(new[] { track1.TrackId });

        var browse = new BrowseUI(_unitOfWork, _audioMock.Object, activePlaylist, input, output);

        await browse.QueueTrackAsync(track2.TrackId);

        activePlaylist.Count.Should().Be(2);
        activePlaylist.PlayOrder.Should().Contain(track2.TrackId);
        output.ToString().Should().Contain("Added");
    }

    [Fact]
    public async Task QueueTrackAsync_WithDurationAndBitRate_ShouldDisplayDetails()
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

        var (browse, output) = CreateBrowseWithOutput("0\n");

        await browse.QueueTrackAsync(track.TrackId);

        var text = output.ToString();
        text.Should().Contain("3:15");
        text.Should().Contain("320kbps");
        text.Should().Contain("Plays:    5");
        text.Should().Contain("Rating:   4/5");
    }

    [Fact]
    public async Task QueueTrackAsync_DuplicateTrack_ShouldNotAppendAgain()
    {
        var track = await CreateTrackAsync("Kerala");

        var input = new StringReader("A\n");
        var output = new StringWriter();
        var activePlaylist = new ActivePlaylistService();
        activePlaylist.SetQueue(new[] { track.TrackId });

        var browse = new BrowseUI(_unitOfWork, _audioMock.Object, activePlaylist, input, output);

        await browse.QueueTrackAsync(track.TrackId);

        activePlaylist.Count.Should().Be(1);
    }

    [Fact]
    public async Task QueueTrackAsync_AppendTrack_ShouldPreserveCurrentIndex()
    {
        var track1 = await CreateTrackAsync("Kerala");
        var track2 = await CreateTrackAsync("Cirrus");
        var track3 = await CreateTrackAsync("Bambro");

        var input = new StringReader("A\n");
        var output = new StringWriter();
        var activePlaylist = new ActivePlaylistService();
        activePlaylist.SetQueue(new[] { track1.TrackId, track2.TrackId });
        activePlaylist.MoveNext(); // index 1 (track2)

        var browse = new BrowseUI(_unitOfWork, _audioMock.Object, activePlaylist, input, output);

        await browse.QueueTrackAsync(track3.TrackId);

        activePlaylist.CurrentIndex.Should().Be(1);
        activePlaylist.CurrentTrackId.Should().Be(track2.TrackId);
        activePlaylist.Count.Should().Be(3);
    }

    // ========== Queue All (Q/A) at Album Level ==========

    [Fact]
    public async Task DisplayTracksByAlbumAsync_QueueAll_ShouldSetQueue()
    {
        var artist = await CreateArtistAsync("Bonobo");
        var album = await CreateAlbumAsync("Migration", artist.ArtistId);
        var track1 = await CreateTrackAsync("Kerala", album.AlbumId, artist.ArtistId);
        var track2 = await CreateTrackAsync("Bambro", album.AlbumId, artist.ArtistId);

        var input = new StringReader("Q\n0\n");
        var output = new StringWriter();
        var activePlaylist = new ActivePlaylistService();
        var browse = new BrowseUI(_unitOfWork, _audioMock.Object, activePlaylist, input, output);

        await browse.DisplayTracksByAlbumAsync(album.AlbumId);

        activePlaylist.Count.Should().Be(2);
        output.ToString().Should().Contain("Set queue: 2 tracks from Migration");
    }

    [Fact]
    public async Task DisplayTracksByAlbumAsync_AddAll_ShouldAppendToQueue()
    {
        var artist = await CreateArtistAsync("Bonobo");
        var album = await CreateAlbumAsync("Migration", artist.ArtistId);
        var track1 = await CreateTrackAsync("Kerala", album.AlbumId, artist.ArtistId);
        var track2 = await CreateTrackAsync("Bambro", album.AlbumId, artist.ArtistId);
        var existingTrack = await CreateTrackAsync("Existing");

        var input = new StringReader("A\n0\n");
        var output = new StringWriter();
        var activePlaylist = new ActivePlaylistService();
        activePlaylist.SetQueue(new[] { existingTrack.TrackId });
        var browse = new BrowseUI(_unitOfWork, _audioMock.Object, activePlaylist, input, output);

        await browse.DisplayTracksByAlbumAsync(album.AlbumId);

        activePlaylist.Count.Should().Be(3);
        output.ToString().Should().Contain("Added 2 tracks from Migration to queue");
    }

    // ========== Queue All (Q/A) at Artist Level ==========

    [Fact]
    public async Task DisplayAlbumsByArtistAsync_QueueAll_ShouldSetQueue()
    {
        var artist = await CreateArtistAsync("Bonobo");
        var album = await CreateAlbumAsync("Migration", artist.ArtistId);
        var track1 = await CreateTrackAsync("Kerala", album.AlbumId, artist.ArtistId);
        var track2 = await CreateTrackAsync("Bambro", album.AlbumId, artist.ArtistId);

        var input = new StringReader("Q\n0\n");
        var output = new StringWriter();
        var activePlaylist = new ActivePlaylistService();
        var browse = new BrowseUI(_unitOfWork, _audioMock.Object, activePlaylist, input, output);

        await browse.DisplayAlbumsByArtistAsync(artist.ArtistId);

        activePlaylist.Count.Should().Be(2);
        output.ToString().Should().Contain("Set queue: 2 tracks by Bonobo");
    }

    [Fact]
    public async Task DisplayAlbumsByArtistAsync_AddAll_ShouldAppendToQueue()
    {
        var artist = await CreateArtistAsync("Bonobo");
        var album = await CreateAlbumAsync("Migration", artist.ArtistId);
        var track1 = await CreateTrackAsync("Kerala", album.AlbumId, artist.ArtistId);
        var existingTrack = await CreateTrackAsync("Existing");

        var input = new StringReader("A\n0\n");
        var output = new StringWriter();
        var activePlaylist = new ActivePlaylistService();
        activePlaylist.SetQueue(new[] { existingTrack.TrackId });
        var browse = new BrowseUI(_unitOfWork, _audioMock.Object, activePlaylist, input, output);

        await browse.DisplayAlbumsByArtistAsync(artist.ArtistId);

        activePlaylist.Count.Should().Be(2);
        output.ToString().Should().Contain("Added 1 tracks by Bonobo to queue");
    }

    // ========== Q Action Returns to Player ==========

    [Fact]
    public async Task DisplayTracksByAlbumAsync_QueueAll_ShouldReturnImmediately()
    {
        var artist = await CreateArtistAsync("Bonobo");
        var album = await CreateAlbumAsync("Migration", artist.ArtistId);
        await CreateTrackAsync("Kerala", album.AlbumId, artist.ArtistId);

        // Q should return — no trailing 0 needed
        var input = new StringReader("Q\n");
        var output = new StringWriter();
        var activePlaylist = new ActivePlaylistService();
        var browse = new BrowseUI(_unitOfWork, _audioMock.Object, activePlaylist, input, output);

        await browse.DisplayTracksByAlbumAsync(album.AlbumId);

        activePlaylist.Count.Should().Be(1);
    }

    [Fact]
    public async Task DisplayAlbumsByArtistAsync_QueueAll_ShouldReturnImmediately()
    {
        var artist = await CreateArtistAsync("Bonobo");
        var album = await CreateAlbumAsync("Migration", artist.ArtistId);
        await CreateTrackAsync("Kerala", album.AlbumId, artist.ArtistId);

        var input = new StringReader("Q\n");
        var output = new StringWriter();
        var activePlaylist = new ActivePlaylistService();
        var browse = new BrowseUI(_unitOfWork, _audioMock.Object, activePlaylist, input, output);

        await browse.DisplayAlbumsByArtistAsync(artist.ArtistId);

        activePlaylist.Count.Should().Be(1);
    }

    [Fact]
    public async Task DisplayArtistsAsync_QueueAll_ShouldReturnImmediately()
    {
        var artist = await CreateArtistAsync("Bonobo");
        var album = await CreateAlbumAsync("Migration", artist.ArtistId);
        await CreateTrackAsync("Kerala", album.AlbumId, artist.ArtistId);

        var input = new StringReader("Q\n");
        var output = new StringWriter();
        var activePlaylist = new ActivePlaylistService();
        var browse = new BrowseUI(_unitOfWork, _audioMock.Object, activePlaylist, input, output);

        await browse.DisplayArtistsAsync();

        activePlaylist.Count.Should().Be(1);
    }

    // ========== A Action Shows Status Message ==========

    [Fact]
    public async Task DisplayTracksByAlbumAsync_AddAll_ShouldShowStatusMessageAfterRedraw()
    {
        var artist = await CreateArtistAsync("Bonobo");
        var album = await CreateAlbumAsync("Migration", artist.ArtistId);
        await CreateTrackAsync("Kerala", album.AlbumId, artist.ArtistId);

        var input = new StringReader("A\n");
        var output = new StringWriter();
        var activePlaylist = new ActivePlaylistService();
        activePlaylist.SetQueue(new[] { 999 }); // existing track
        var clearCount = 0;
        var browse = new BrowseUI(_unitOfWork, _audioMock.Object, activePlaylist, input, output, () => clearCount++);

        await browse.DisplayTracksByAlbumAsync(album.AlbumId);

        clearCount.Should().Be(1); // initial draw only, then returns to main menu
        output.ToString().Should().Contain("Added 1 tracks from Migration to queue");
    }

    // ========== AppendToQueue Preserves Shuffle ==========

    [Fact]
    public async Task AppendToQueue_ShouldPreserveShuffleState()
    {
        var track1 = await CreateTrackAsync("Kerala");
        var track2 = await CreateTrackAsync("Cirrus");
        var track3 = await CreateTrackAsync("Bambro");

        var input = new StringReader("A\n");
        var output = new StringWriter();
        var activePlaylist = new ActivePlaylistService();
        activePlaylist.SetQueue(new[] { track1.TrackId, track2.TrackId });
        activePlaylist.ToggleShuffle(); // enable shuffle
        activePlaylist.IsShuffled.Should().BeTrue();

        var browse = new BrowseUI(_unitOfWork, _audioMock.Object, activePlaylist, input, output);

        await browse.QueueTrackAsync(track3.TrackId);

        activePlaylist.IsShuffled.Should().BeTrue();
        activePlaylist.Count.Should().Be(3);
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
