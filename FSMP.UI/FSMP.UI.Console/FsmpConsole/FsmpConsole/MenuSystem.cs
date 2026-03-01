using FSMP.Core.Interfaces;

namespace FsmpConsole;

/// <summary>
/// Thin wrapper that launches the PlayerUI as the main application screen.
/// </summary>
public class MenuSystem
{
    private readonly IPlaybackController _playback;
    private readonly IPlaylistManager _playlists;
    private readonly ILibraryManager _library;
    private readonly ILibraryBrowser _browser;
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly Action? _onClear;

    public MenuSystem(
        IPlaybackController playback,
        IPlaylistManager playlists,
        ILibraryManager library,
        ILibraryBrowser browser,
        TextReader input,
        TextWriter output,
        Action? onClear = null)
    {
        _playback = playback ?? throw new ArgumentNullException(nameof(playback));
        _playlists = playlists ?? throw new ArgumentNullException(nameof(playlists));
        _library = library ?? throw new ArgumentNullException(nameof(library));
        _browser = browser ?? throw new ArgumentNullException(nameof(browser));
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _onClear = onClear;
    }

    /// <summary>
    /// Launches the PlayerUI as the main application screen.
    /// </summary>
    public async Task RunAsync()
    {
        var playerUI = new PlayerUI(
            _playback, _playlists, _library, _browser,
            _input, _output, _onClear);
        await playerUI.RunAsync();
    }
}
