using FsmpDataAcsses;
using FsmpLibrary.Models;
using FsmpLibrary.Services;

namespace FsmpConsole;

/// <summary>
/// Hierarchical browse UI for navigating Artists → Albums → Tracks.
/// Uses TextReader/TextWriter for testability.
/// </summary>
public class BrowseUI
{
    private readonly UnitOfWork _unitOfWork;
    private readonly IAudioService _audioService;
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public BrowseUI(UnitOfWork unitOfWork, IAudioService audioService, TextReader input, TextWriter output)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    /// <summary>
    /// Entry point — displays the artist list.
    /// </summary>
    public async Task RunAsync()
    {
        await DisplayArtistsAsync();
    }

    /// <summary>
    /// Lists all artists and lets the user select one to browse albums.
    /// </summary>
    public async Task DisplayArtistsAsync()
    {
        var artists = (await _unitOfWork.Artists.GetAllAsync()).ToList();

        if (artists.Count == 0)
        {
            _output.WriteLine("No artists in library. Scan a library first.");
            return;
        }

        _output.WriteLine();
        _output.WriteLine("== Artists ==");
        for (int i = 0; i < artists.Count; i++)
        {
            _output.WriteLine($"  {i + 1}) {artists[i].Name}");
        }
        _output.WriteLine("  0) Back");
        _output.Write("Select artist: ");

        var input = _input.ReadLine()?.Trim();
        if (input == "0" || string.IsNullOrEmpty(input))
            return;

        if (int.TryParse(input, out var index) && index >= 1 && index <= artists.Count)
        {
            await DisplayAlbumsByArtistAsync(artists[index - 1].ArtistId);
        }
        else
        {
            _output.WriteLine("Invalid selection.");
        }
    }

    /// <summary>
    /// Lists albums for the given artist. If no albums exist, shows tracks directly.
    /// </summary>
    public async Task DisplayAlbumsByArtistAsync(int artistId)
    {
        var albums = (await _unitOfWork.Albums.GetByArtistAsync(artistId)).ToList();
        var artist = await _unitOfWork.Artists.GetByIdAsync(artistId);

        if (artist == null)
        {
            _output.WriteLine("Artist not found.");
            return;
        }

        _output.WriteLine();
        _output.WriteLine($"== Albums by {artist.Name} ==");

        if (albums.Count == 0)
        {
            _output.WriteLine("  No albums found.");
            return;
        }

        for (int i = 0; i < albums.Count; i++)
        {
            var yearStr = albums[i].Year.HasValue ? $" ({albums[i].Year})" : "";
            _output.WriteLine($"  {i + 1}) {albums[i].Title}{yearStr}");
        }
        _output.WriteLine("  0) Back");
        _output.Write("Select album: ");

        var input = _input.ReadLine()?.Trim();
        if (input == "0" || string.IsNullOrEmpty(input))
            return;

        if (int.TryParse(input, out var index) && index >= 1 && index <= albums.Count)
        {
            await DisplayTracksByAlbumAsync(albums[index - 1].AlbumId);
        }
        else
        {
            _output.WriteLine("Invalid selection.");
        }
    }

    /// <summary>
    /// Lists tracks for the given album and lets the user select one to play.
    /// </summary>
    public async Task DisplayTracksByAlbumAsync(int albumId)
    {
        var album = await _unitOfWork.Albums.GetWithTracksAsync(albumId);

        if (album == null)
        {
            _output.WriteLine("Album not found.");
            return;
        }

        var tracks = album.Tracks.ToList();

        _output.WriteLine();
        _output.WriteLine($"== {album.Title} ==");

        if (tracks.Count == 0)
        {
            _output.WriteLine("  No tracks in this album.");
            return;
        }

        for (int i = 0; i < tracks.Count; i++)
        {
            var t = tracks[i];
            var durationStr = t.Duration.HasValue
                ? $" [{t.Duration.Value.Minutes}:{t.Duration.Value.Seconds:D2}]"
                : "";
            _output.WriteLine($"  {i + 1}) {t.DisplayTitle}{durationStr}");
        }
        _output.WriteLine("  0) Back");
        _output.Write("Select track: ");

        var input = _input.ReadLine()?.Trim();
        if (input == "0" || string.IsNullOrEmpty(input))
            return;

        if (int.TryParse(input, out var index) && index >= 1 && index <= tracks.Count)
        {
            await PlayTrackAsync(tracks[index - 1].TrackId);
        }
        else
        {
            _output.WriteLine("Invalid selection.");
        }
    }

    /// <summary>
    /// Plays the track with the given ID and displays now-playing info.
    /// </summary>
    public async Task PlayTrackAsync(int trackId)
    {
        var track = await _unitOfWork.Tracks.GetByIdAsync(trackId);

        if (track == null)
        {
            _output.WriteLine("Track not found.");
            return;
        }

        _output.WriteLine();
        _output.WriteLine("== Now Playing ==");
        _output.WriteLine($"  Title:  {track.DisplayTitle}");
        _output.WriteLine($"  Artist: {track.DisplayArtist}");
        _output.WriteLine($"  Album:  {track.DisplayAlbum}");
        if (track.Duration.HasValue)
            _output.WriteLine($"  Duration: {track.Duration.Value.Minutes}:{track.Duration.Value.Seconds:D2}");
        if (track.BitRate.HasValue)
            _output.WriteLine($"  BitRate:  {track.BitRate}kbps");
        _output.WriteLine($"  Plays:  {track.PlayCount}");
        if (track.Rating.HasValue)
            _output.WriteLine($"  Rating: {track.Rating}/5");

        await _audioService.PlayTrackAsync(track);
    }
}
