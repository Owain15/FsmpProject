using FSMP.Core;
using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using FsmpLibrary.Services;

namespace FsmpConsole;

/// <summary>
/// Thin wrapper that launches the PlayerUI as the main application screen.
/// </summary>
public class MenuSystem
{
    private readonly IAudioService _audioService;
    private readonly ConfigurationService _configService;
    private readonly LibraryScanService _scanService;
    private readonly UnitOfWork _unitOfWork;
    private readonly PlaylistService _playlistService;
    private readonly ActivePlaylistService _activePlaylist;
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public MenuSystem(
        IAudioService audioService,
        ConfigurationService configService,
        StatisticsService statsService,
        LibraryScanService scanService,
        UnitOfWork unitOfWork,
        PlaylistService playlistService,
        ActivePlaylistService activePlaylist,
        TextReader input,
        TextWriter output)
    {
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _scanService = scanService ?? throw new ArgumentNullException(nameof(scanService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _playlistService = playlistService ?? throw new ArgumentNullException(nameof(playlistService));
        _activePlaylist = activePlaylist ?? throw new ArgumentNullException(nameof(activePlaylist));
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    /// <summary>
    /// Launches the PlayerUI as the main application screen.
    /// </summary>
    public async Task RunAsync()
    {
        var playerUI = new PlayerUI(
            _activePlaylist, _audioService, _unitOfWork,
            _playlistService, _configService, _scanService,
            _input, _output);
        await playerUI.RunAsync();
    }
}
