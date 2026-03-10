using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;
using FluentAssertions;
using FsmpConsole;
using Moq;

namespace FSMP.Tests.UI;

public class MenuSystemTests
{
    private readonly Mock<IPlaybackController> _playbackMock;
    private readonly Mock<IPlaylistManager> _playlistsMock;
    private readonly Mock<ILibraryManager> _libraryMock;
    private readonly Mock<ILibraryBrowser> _browserMock;
    private readonly Mock<ITagService> _tagServiceMock;

    public MenuSystemTests()
    {
        _playbackMock = new Mock<IPlaybackController>();
        _playlistsMock = new Mock<IPlaylistManager>();
        _libraryMock = new Mock<ILibraryManager>();
        _browserMock = new Mock<ILibraryBrowser>();
        _tagServiceMock = new Mock<ITagService>();

        // Default setups
        _playbackMock.Setup(p => p.GetCurrentTrackAsync()).ReturnsAsync(Result.Success<Track?>(null));
        _playbackMock.Setup(p => p.GetQueueItemsAsync(It.IsAny<bool>())).ReturnsAsync(Result.Success(new List<QueueItem>()));
        _playbackMock.Setup(p => p.RepeatMode).Returns(RepeatMode.None);
        _playbackMock.Setup(p => p.IsShuffled).Returns(false);
        _playbackMock.Setup(p => p.IsPlaying).Returns(false);
        _playbackMock.Setup(p => p.QueueCount).Returns(0);
    }

    private (MenuSystem menu, StringWriter output) CreateMenuWithOutput(string inputLines)
    {
        var input = new StringReader(inputLines);
        var output = new StringWriter();
        var menu = new MenuSystem(
            _playbackMock.Object, _playlistsMock.Object, _libraryMock.Object, _browserMock.Object,
            _tagServiceMock.Object, input, output);
        return (menu, output);
    }

    // ========== Constructor Tests ==========

    [Fact]
    public void Constructor_WithNullPlayback_ShouldThrow()
    {
        var act = () => new MenuSystem(null!, _playlistsMock.Object, _libraryMock.Object, _browserMock.Object, _tagServiceMock.Object, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("playback");
    }

    [Fact]
    public void Constructor_WithNullPlaylists_ShouldThrow()
    {
        var act = () => new MenuSystem(_playbackMock.Object, null!, _libraryMock.Object, _browserMock.Object, _tagServiceMock.Object, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("playlists");
    }

    [Fact]
    public void Constructor_WithNullLibrary_ShouldThrow()
    {
        var act = () => new MenuSystem(_playbackMock.Object, _playlistsMock.Object, null!, _browserMock.Object, _tagServiceMock.Object, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("library");
    }

    [Fact]
    public void Constructor_WithNullBrowser_ShouldThrow()
    {
        var act = () => new MenuSystem(_playbackMock.Object, _playlistsMock.Object, _libraryMock.Object, null!, _tagServiceMock.Object, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("browser");
    }

    [Fact]
    public void Constructor_WithNullTagService_ShouldThrow()
    {
        var act = () => new MenuSystem(_playbackMock.Object, _playlistsMock.Object, _libraryMock.Object, _browserMock.Object, null!, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("tagService");
    }

    [Fact]
    public void Constructor_WithNullInput_ShouldThrow()
    {
        var act = () => new MenuSystem(_playbackMock.Object, _playlistsMock.Object, _libraryMock.Object, _browserMock.Object, _tagServiceMock.Object, null!, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("input");
    }

    [Fact]
    public void Constructor_WithNullOutput_ShouldThrow()
    {
        var act = () => new MenuSystem(_playbackMock.Object, _playlistsMock.Object, _libraryMock.Object, _browserMock.Object, _tagServiceMock.Object, TextReader.Null, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("output");
    }

    // ========== RunAsync Tests ==========

    [Fact]
    public async Task RunAsync_ShouldLaunchPlayerUI_AndExitWithX()
    {
        var (menu, output) = CreateMenuWithOutput("X\n");

        await menu.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Queue: (empty)");
        text.Should().Contain("Goodbye!");
    }

    [Fact]
    public async Task RunAsync_ShouldShowPlayerControls()
    {
        var (menu, output) = CreateMenuWithOutput("X\n");

        await menu.RunAsync();

        var text = output.ToString();
        text.Should().Contain("[N] Next");
        text.Should().Contain("[B] Browse");
        text.Should().Contain("[L] Playlists");
        text.Should().Contain("[D] Directories");
        text.Should().Contain("[X] Exit");
    }
}
