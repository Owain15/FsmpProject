using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;
using FluentAssertions;
using FsmpConsole;
using Moq;

namespace FSMP.Tests.UI;

public class BrowseUITests
{
    private readonly Mock<ILibraryBrowser> _browserMock;
    private readonly Mock<IPlaybackController> _playbackMock;

    public BrowseUITests()
    {
        _browserMock = new Mock<ILibraryBrowser>();
        _playbackMock = new Mock<IPlaybackController>();
    }

    private BrowseUI CreateBrowse(string inputLines)
    {
        return CreateBrowseWithOutput(inputLines).browse;
    }

    private (BrowseUI browse, StringWriter output) CreateBrowseWithOutput(string inputLines)
    {
        var input = new StringReader(inputLines);
        var output = new StringWriter();
        var browse = new BrowseUI(_browserMock.Object, _playbackMock.Object, input, output);
        return (browse, output);
    }

    // ========== Constructor Tests ==========

    [Fact]
    public void Constructor_WithNullBrowser_ShouldThrow()
    {
        var act = () => new BrowseUI(null!, _playbackMock.Object, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("browser");
    }

    [Fact]
    public void Constructor_WithNullPlayback_ShouldThrow()
    {
        var act = () => new BrowseUI(_browserMock.Object, null!, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("playback");
    }

    [Fact]
    public void Constructor_WithNullInput_ShouldThrow()
    {
        var act = () => new BrowseUI(_browserMock.Object, _playbackMock.Object, null!, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("input");
    }

    [Fact]
    public void Constructor_WithNullOutput_ShouldThrow()
    {
        var act = () => new BrowseUI(_browserMock.Object, _playbackMock.Object, TextReader.Null, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("output");
    }

    // ========== DisplayArtistsAsync Tests ==========

    [Fact]
    public async Task DisplayArtistsAsync_NoArtists_ShouldShowMessage()
    {
        _browserMock.Setup(b => b.GetAllArtistsAsync()).ReturnsAsync(Result.Success(new List<Artist>()));
        var (browse, output) = CreateBrowseWithOutput("");

        await browse.DisplayArtistsAsync();

        output.ToString().Should().Contain("No artists in library");
    }

    [Fact]
    public async Task DisplayArtistsAsync_ShouldListAllArtists()
    {
        _browserMock.Setup(b => b.GetAllArtistsAsync()).ReturnsAsync(Result.Success(new List<Artist>
        {
            new() { ArtistId = 1, Name = "Bonobo" },
            new() { ArtistId = 2, Name = "AC/DC" }
        }));

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
        _browserMock.Setup(b => b.GetAllArtistsAsync()).ReturnsAsync(Result.Success(new List<Artist>
        {
            new() { ArtistId = 1, Name = "Bonobo" }
        }));

        var (browse, output) = CreateBrowseWithOutput("0\n");

        await browse.DisplayArtistsAsync();

        var text = output.ToString();
        text.Should().Contain("0) Back");
        text.Should().NotContain("Albums by");
    }

    [Fact]
    public async Task DisplayArtistsAsync_InvalidSelection_ShouldShowError()
    {
        _browserMock.Setup(b => b.GetAllArtistsAsync()).ReturnsAsync(Result.Success(new List<Artist>
        {
            new() { ArtistId = 1, Name = "Bonobo" }
        }));

        var (browse, output) = CreateBrowseWithOutput("999\n0\n");

        await browse.DisplayArtistsAsync();

        output.ToString().Should().Contain("Invalid selection");
    }

    [Fact]
    public async Task DisplayArtistsAsync_SelectArtist_ShouldNavigateToAlbums()
    {
        _browserMock.Setup(b => b.GetAllArtistsAsync()).ReturnsAsync(Result.Success(new List<Artist>
        {
            new() { ArtistId = 1, Name = "Bonobo" }
        }));
        _browserMock.Setup(b => b.GetArtistByIdAsync(1)).ReturnsAsync(Result.Success<Artist?>(new Artist { ArtistId = 1, Name = "Bonobo" }));
        _browserMock.Setup(b => b.GetAlbumsByArtistAsync(1)).ReturnsAsync(Result.Success(new List<Album>
        {
            new() { AlbumId = 1, Title = "Migration", Year = 2017, ArtistId = 1 }
        }));

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
        _browserMock.Setup(b => b.GetArtistByIdAsync(9999)).ReturnsAsync(Result.Success<Artist?>(null));
        _browserMock.Setup(b => b.GetAlbumsByArtistAsync(9999)).ReturnsAsync(Result.Success(new List<Album>()));

        var (browse, output) = CreateBrowseWithOutput("");

        await browse.DisplayAlbumsByArtistAsync(9999);

        output.ToString().Should().Contain("Artist not found");
    }

    [Fact]
    public async Task DisplayAlbumsByArtistAsync_NoAlbums_ShouldShowMessage()
    {
        _browserMock.Setup(b => b.GetArtistByIdAsync(1)).ReturnsAsync(Result.Success<Artist?>(new Artist { ArtistId = 1, Name = "Unknown" }));
        _browserMock.Setup(b => b.GetAlbumsByArtistAsync(1)).ReturnsAsync(Result.Success(new List<Album>()));

        var (browse, output) = CreateBrowseWithOutput("");

        await browse.DisplayAlbumsByArtistAsync(1);

        output.ToString().Should().Contain("No albums found");
    }

    [Fact]
    public async Task DisplayAlbumsByArtistAsync_ShouldListAlbums()
    {
        _browserMock.Setup(b => b.GetArtistByIdAsync(1)).ReturnsAsync(Result.Success<Artist?>(new Artist { ArtistId = 1, Name = "Bonobo" }));
        _browserMock.Setup(b => b.GetAlbumsByArtistAsync(1)).ReturnsAsync(Result.Success(new List<Album>
        {
            new() { AlbumId = 1, Title = "Migration", Year = 2017 },
            new() { AlbumId = 2, Title = "The North Borders", Year = 2013 }
        }));

        var (browse, output) = CreateBrowseWithOutput("0\n");

        await browse.DisplayAlbumsByArtistAsync(1);

        var text = output.ToString();
        text.Should().Contain("Albums by Bonobo");
        text.Should().Contain("Migration (2017)");
        text.Should().Contain("The North Borders (2013)");
    }

    [Fact]
    public async Task DisplayAlbumsByArtistAsync_AlbumWithNoYear_ShouldOmitYear()
    {
        _browserMock.Setup(b => b.GetArtistByIdAsync(1)).ReturnsAsync(Result.Success<Artist?>(new Artist { ArtistId = 1, Name = "Bonobo" }));
        _browserMock.Setup(b => b.GetAlbumsByArtistAsync(1)).ReturnsAsync(Result.Success(new List<Album>
        {
            new() { AlbumId = 1, Title = "Singles" }
        }));

        var (browse, output) = CreateBrowseWithOutput("0\n");

        await browse.DisplayAlbumsByArtistAsync(1);

        var text = output.ToString();
        text.Should().Contain("Singles");
        text.Should().NotContain("Singles (");
    }

    [Fact]
    public async Task DisplayAlbumsByArtistAsync_InvalidSelection_ShouldShowError()
    {
        _browserMock.Setup(b => b.GetArtistByIdAsync(1)).ReturnsAsync(Result.Success<Artist?>(new Artist { ArtistId = 1, Name = "Bonobo" }));
        _browserMock.Setup(b => b.GetAlbumsByArtistAsync(1)).ReturnsAsync(Result.Success(new List<Album>
        {
            new() { AlbumId = 1, Title = "Migration" }
        }));

        var (browse, output) = CreateBrowseWithOutput("999\n0\n");

        await browse.DisplayAlbumsByArtistAsync(1);

        output.ToString().Should().Contain("Invalid selection");
    }

    // ========== DisplayTracksByAlbumAsync Tests ==========

    [Fact]
    public async Task DisplayTracksByAlbumAsync_AlbumNotFound_ShouldShowMessage()
    {
        _browserMock.Setup(b => b.GetAlbumWithTracksAsync(9999)).ReturnsAsync(Result.Success<Album?>(null));

        var (browse, output) = CreateBrowseWithOutput("");

        await browse.DisplayTracksByAlbumAsync(9999);

        output.ToString().Should().Contain("Album not found");
    }

    [Fact]
    public async Task DisplayTracksByAlbumAsync_NoTracks_ShouldShowMessage()
    {
        _browserMock.Setup(b => b.GetAlbumWithTracksAsync(1)).ReturnsAsync(Result.Success<Album?>(new Album
        {
            AlbumId = 1, Title = "Empty Album", Tracks = new List<Track>()
        }));

        var (browse, output) = CreateBrowseWithOutput("");

        await browse.DisplayTracksByAlbumAsync(1);

        output.ToString().Should().Contain("No tracks in this album");
    }

    [Fact]
    public async Task DisplayTracksByAlbumAsync_ShouldListTracks()
    {
        _browserMock.Setup(b => b.GetAlbumWithTracksAsync(1)).ReturnsAsync(Result.Success<Album?>(new Album
        {
            AlbumId = 1, Title = "Migration",
            Tracks = new List<Track>
            {
                new() { TrackId = 1, Title = "Kerala", FilePath = "k.mp3", FileHash = "a" },
                new() { TrackId = 2, Title = "Bambro Koyo Ganda", FilePath = "b.mp3", FileHash = "b" }
            }
        }));

        var (browse, output) = CreateBrowseWithOutput("0\n");

        await browse.DisplayTracksByAlbumAsync(1);

        var text = output.ToString();
        text.Should().Contain("Migration");
        text.Should().Contain("Kerala");
        text.Should().Contain("Bambro Koyo Ganda");
    }

    [Fact]
    public async Task DisplayTracksByAlbumAsync_ShouldShowDuration()
    {
        _browserMock.Setup(b => b.GetAlbumWithTracksAsync(1)).ReturnsAsync(Result.Success<Album?>(new Album
        {
            AlbumId = 1, Title = "Migration",
            Tracks = new List<Track>
            {
                new() { TrackId = 1, Title = "Kerala", FilePath = "k.mp3", FileHash = "a", Duration = TimeSpan.FromSeconds(195) }
            }
        }));

        var (browse, output) = CreateBrowseWithOutput("0\n");

        await browse.DisplayTracksByAlbumAsync(1);

        output.ToString().Should().Contain("[3:15]");
    }

    [Fact]
    public async Task DisplayTracksByAlbumAsync_QueueAll_ShouldSetQueue()
    {
        _browserMock.Setup(b => b.GetAlbumWithTracksAsync(1)).ReturnsAsync(Result.Success<Album?>(new Album
        {
            AlbumId = 1, Title = "Migration",
            Tracks = new List<Track>
            {
                new() { TrackId = 10, Title = "Kerala", FilePath = "k.mp3", FileHash = "a" },
                new() { TrackId = 20, Title = "Bambro", FilePath = "b.mp3", FileHash = "b" }
            }
        }));

        var (browse, output) = CreateBrowseWithOutput("Q\n");

        await browse.DisplayTracksByAlbumAsync(1);

        _playbackMock.Verify(p => p.SetQueue(It.Is<IReadOnlyList<int>>(ids => ids.Count == 2)), Times.Once);
        output.ToString().Should().Contain("Set queue: 2 tracks from Migration");
    }

    [Fact]
    public async Task DisplayTracksByAlbumAsync_AddAll_ShouldAppendToQueue()
    {
        _browserMock.Setup(b => b.GetAlbumWithTracksAsync(1)).ReturnsAsync(Result.Success<Album?>(new Album
        {
            AlbumId = 1, Title = "Migration",
            Tracks = new List<Track>
            {
                new() { TrackId = 10, Title = "Kerala", FilePath = "k.mp3", FileHash = "a" },
                new() { TrackId = 20, Title = "Bambro", FilePath = "b.mp3", FileHash = "b" }
            }
        }));

        var (browse, output) = CreateBrowseWithOutput("A\n");

        await browse.DisplayTracksByAlbumAsync(1);

        _playbackMock.Verify(p => p.AppendToQueue(It.Is<List<int>>(ids => ids.Count == 2)), Times.Once);
        output.ToString().Should().Contain("Added 2 tracks from Migration to queue");
    }

    // ========== Queue All (Q/A) at Artist Level ==========

    [Fact]
    public async Task DisplayAlbumsByArtistAsync_QueueAll_ShouldSetQueue()
    {
        _browserMock.Setup(b => b.GetArtistByIdAsync(1)).ReturnsAsync(Result.Success<Artist?>(new Artist { ArtistId = 1, Name = "Bonobo" }));
        _browserMock.Setup(b => b.GetAlbumsByArtistAsync(1)).ReturnsAsync(Result.Success(new List<Album>
        {
            new() { AlbumId = 1, Title = "Migration" }
        }));
        _browserMock.Setup(b => b.GetAllTrackIdsByArtistAsync(1)).ReturnsAsync(Result.Success(new List<int> { 10, 20 }));

        var (browse, output) = CreateBrowseWithOutput("Q\n");

        await browse.DisplayAlbumsByArtistAsync(1);

        _playbackMock.Verify(p => p.SetQueue(It.Is<IReadOnlyList<int>>(ids => ids.Count == 2)), Times.Once);
        output.ToString().Should().Contain("Set queue: 2 tracks by Bonobo");
    }

    [Fact]
    public async Task DisplayAlbumsByArtistAsync_AddAll_ShouldAppendToQueue()
    {
        _browserMock.Setup(b => b.GetArtistByIdAsync(1)).ReturnsAsync(Result.Success<Artist?>(new Artist { ArtistId = 1, Name = "Bonobo" }));
        _browserMock.Setup(b => b.GetAlbumsByArtistAsync(1)).ReturnsAsync(Result.Success(new List<Album>
        {
            new() { AlbumId = 1, Title = "Migration" }
        }));
        _browserMock.Setup(b => b.GetAllTrackIdsByArtistAsync(1)).ReturnsAsync(Result.Success(new List<int> { 10 }));

        var (browse, output) = CreateBrowseWithOutput("A\n");

        await browse.DisplayAlbumsByArtistAsync(1);

        _playbackMock.Verify(p => p.AppendToQueue(It.Is<List<int>>(ids => ids.Count == 1)), Times.Once);
        output.ToString().Should().Contain("Added 1 tracks by Bonobo to queue");
    }

    // ========== QueueTrackAsync Tests ==========

    [Fact]
    public async Task QueueTrackAsync_TrackNotFound_ShouldShowMessage()
    {
        _browserMock.Setup(b => b.GetTrackByIdAsync(9999)).ReturnsAsync(Result.Success<Track?>(null));

        var (browse, output) = CreateBrowseWithOutput("");

        await browse.QueueTrackAsync(9999);

        output.ToString().Should().Contain("Track not found");
    }

    [Fact]
    public async Task QueueTrackAsync_ShouldDisplayTrackDetails()
    {
        var track = new Track { TrackId = 1, Title = "Kerala", FilePath = "k.mp3", FileHash = "a",
            Artist = new Artist { Name = "Bonobo" } };
        _browserMock.Setup(b => b.GetTrackByIdAsync(1)).ReturnsAsync(Result.Success<Track?>(track));

        var (browse, output) = CreateBrowseWithOutput("0\n");

        await browse.QueueTrackAsync(1);

        var text = output.ToString();
        text.Should().Contain("Track Details");
        text.Should().Contain("Kerala");
    }

    [Fact]
    public async Task QueueTrackAsync_ChooseQ_ShouldSetQueue()
    {
        var track = new Track { TrackId = 1, Title = "Kerala", FilePath = "k.mp3", FileHash = "a" };
        _browserMock.Setup(b => b.GetTrackByIdAsync(1)).ReturnsAsync(Result.Success<Track?>(track));

        var (browse, output) = CreateBrowseWithOutput("Q\n");

        await browse.QueueTrackAsync(1);

        _playbackMock.Verify(p => p.SetQueue(It.Is<IReadOnlyList<int>>(ids => ids.Contains(1))), Times.Once);
        output.ToString().Should().Contain("Set queue:");
    }

    [Fact]
    public async Task QueueTrackAsync_ChooseA_ShouldAddToQueue()
    {
        var track = new Track { TrackId = 2, Title = "Cirrus", FilePath = "c.mp3", FileHash = "b" };
        _browserMock.Setup(b => b.GetTrackByIdAsync(2)).ReturnsAsync(Result.Success<Track?>(track));

        var (browse, output) = CreateBrowseWithOutput("A\n");

        await browse.QueueTrackAsync(2);

        _playbackMock.Verify(p => p.AppendToQueue(It.Is<List<int>>(ids => ids.Contains(2))), Times.Once);
        output.ToString().Should().Contain("Added");
    }

    [Fact]
    public async Task QueueTrackAsync_WithDurationAndBitRate_ShouldDisplayDetails()
    {
        var track = new Track
        {
            TrackId = 1, Title = "Kerala", FilePath = "k.mp3", FileHash = "a",
            Duration = TimeSpan.FromSeconds(195), BitRate = 320,
            PlayCount = 5, Rating = 4
        };
        _browserMock.Setup(b => b.GetTrackByIdAsync(1)).ReturnsAsync(Result.Success<Track?>(track));

        var (browse, output) = CreateBrowseWithOutput("0\n");

        await browse.QueueTrackAsync(1);

        var text = output.ToString();
        text.Should().Contain("3:15");
        text.Should().Contain("320kbps");
        text.Should().Contain("Plays:    5");
        text.Should().Contain("Rating:   4/5");
    }

    // ========== Q/A Return to Main ==========

    [Fact]
    public async Task DisplayArtistsAsync_QueueAll_ShouldReturnImmediately()
    {
        _browserMock.Setup(b => b.GetAllArtistsAsync()).ReturnsAsync(Result.Success(new List<Artist>
        {
            new() { ArtistId = 1, Name = "Bonobo" }
        }));
        _browserMock.Setup(b => b.GetAllTrackIdsAsync()).ReturnsAsync(Result.Success(new List<int> { 1 }));

        var (browse, output) = CreateBrowseWithOutput("Q\n");

        await browse.DisplayArtistsAsync();

        _playbackMock.Verify(p => p.SetQueue(It.IsAny<IReadOnlyList<int>>()), Times.Once);
    }

    // ========== RunAsync Integration Test ==========

    [Fact]
    public async Task RunAsync_ShouldDelegateToDisplayArtistsAsync()
    {
        _browserMock.Setup(b => b.GetAllArtistsAsync()).ReturnsAsync(Result.Success(new List<Artist>()));

        var (browse, output) = CreateBrowseWithOutput("");

        await browse.RunAsync();

        output.ToString().Should().Contain("No artists in library");
    }
}
