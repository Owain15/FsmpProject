using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;

namespace FsmpConsole;

/// <summary>
/// Hierarchical browse UI for navigating Artists → Albums → Tracks.
/// Uses TextReader/TextWriter for testability.
/// </summary>
public class BrowseUI
{
    private readonly ILibraryBrowser _browser;
    private readonly IPlaybackController _playback;
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly Action? _onClear;
    private string? _statusMessage;
    private bool _returnToMain;

    public BrowseUI(ILibraryBrowser browser, IPlaybackController playback, TextReader input, TextWriter output, Action? onClear = null)
    {
        _browser = browser ?? throw new ArgumentNullException(nameof(browser));
        _playback = playback ?? throw new ArgumentNullException(nameof(playback));
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _onClear = onClear;
    }

    /// <summary>
    /// Entry point — displays the artist list.
    /// </summary>
    public async Task RunAsync()
    {
        _returnToMain = false;
        await DisplayArtistsAsync();
    }

    /// <summary>
    /// Lists all artists and lets the user select one to browse albums,
    /// or queue all tracks.
    /// </summary>
    public async Task DisplayArtistsAsync()
    {
        while (true)
        {
            var artistsResult = await _browser.GetAllArtistsAsync();
            if (!artistsResult.IsSuccess)
            {
                _output.WriteLine($"\nError: {artistsResult.ErrorMessage}");
                return;
            }
            var artists = artistsResult.Value!;

            if (artists.Count == 0)
            {
                _output.WriteLine("\nNo artists in library. Scan a library first.");
                return;
            }

            _onClear?.Invoke();
            if (_statusMessage != null)
            {
                _output.WriteLine(_statusMessage);
                _statusMessage = null;
            }
            Print.WriteSelectionMenu(_output, "Artists",
                artists.Select(a => a.Name).ToList(),
                prompt: null, backLabel: null);

            WriteDivider();
            WriteQueueOptions("all artists");
            _output.WriteLine("  0) Back");
            WritePrompt();

            var input = _input.ReadLine()?.Trim();
            if (input == "0" || string.IsNullOrEmpty(input))
                return;

            if (input.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                var trackIdsResult = await _browser.GetAllTrackIdsAsync();
                if (trackIdsResult.IsSuccess && trackIdsResult.Value!.Count > 0)
                {
                    _playback.SetQueue(trackIdsResult.Value);
                    _output.WriteLine($"  Set queue: {trackIdsResult.Value.Count} tracks.");
                }
                else
                {
                    _output.WriteLine("  No tracks to queue.");
                }
                _returnToMain = true;
                return;
            }

            if (input.Equals("a", StringComparison.OrdinalIgnoreCase))
            {
                var trackIdsResult = await _browser.GetAllTrackIdsAsync();
                if (trackIdsResult.IsSuccess && trackIdsResult.Value!.Count > 0)
                {
                    _playback.AppendToQueue(trackIdsResult.Value);
                    _output.WriteLine($"  Added {trackIdsResult.Value.Count} tracks to queue.");
                }
                else
                {
                    _output.WriteLine("  No tracks to add.");
                }
                _returnToMain = true;
                return;
            }

            if (int.TryParse(input, out var index) && index >= 1 && index <= artists.Count)
            {
                await DisplayAlbumsByArtistAsync(artists[index - 1].ArtistId);
                if (_returnToMain) return;
            }
            else
            {
                _output.WriteLine("Invalid selection.");
            }
        }
    }

    /// <summary>
    /// Lists albums for the given artist. Offers Q/A to queue all tracks by artist.
    /// </summary>
    public async Task DisplayAlbumsByArtistAsync(int artistId)
    {
        while (true)
        {
            var albumsResult = await _browser.GetAlbumsByArtistAsync(artistId);
            var artistResult = await _browser.GetArtistByIdAsync(artistId);

            if (!artistResult.IsSuccess || artistResult.Value == null)
            {
                _output.WriteLine("\nArtist not found.");
                return;
            }
            var artist = artistResult.Value;
            var albums = albumsResult.IsSuccess ? albumsResult.Value! : new List<Album>();

            if (albums.Count == 0)
            {
                _output.WriteLine();
                _output.WriteLine($"== Albums by {artist.Name} ==");
                _output.WriteLine();
                _output.WriteLine("  No albums found.");
                return;
            }

            _onClear?.Invoke();
            if (_statusMessage != null)
            {
                _output.WriteLine(_statusMessage);
                _statusMessage = null;
            }
            var items = albums.Select(a =>
            {
                var yearStr = a.Year.HasValue ? $" ({a.Year})" : "";
                return $"{a.Title}{yearStr}";
            }).ToList();

            Print.WriteSelectionMenu(_output, $"Albums by {artist.Name}",
                items,
                prompt: null, backLabel: null);

            WriteDivider();
            WriteQueueOptions($"all by {artist.Name}");
            _output.WriteLine("  0) Back");
            WritePrompt();

            var input = _input.ReadLine()?.Trim();
            if (input == "0" || string.IsNullOrEmpty(input))
                return;

            if (input.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                var trackIdsResult = await _browser.GetAllTrackIdsByArtistAsync(artistId);
                var trackIds = trackIdsResult.IsSuccess ? trackIdsResult.Value! : new List<int>();
                if (trackIds.Count > 0)
                {
                    _playback.SetQueue(trackIds);
                    _output.WriteLine($"  Set queue: {trackIds.Count} tracks by {artist.Name}.");
                }
                else
                {
                    _output.WriteLine("  No tracks to queue.");
                }
                _returnToMain = true;
                return;
            }

            if (input.Equals("a", StringComparison.OrdinalIgnoreCase))
            {
                var trackIdsResult = await _browser.GetAllTrackIdsByArtistAsync(artistId);
                var trackIds = trackIdsResult.IsSuccess ? trackIdsResult.Value! : new List<int>();
                if (trackIds.Count > 0)
                {
                    _playback.AppendToQueue(trackIds);
                    _output.WriteLine($"  Added {trackIds.Count} tracks by {artist.Name} to queue.");
                }
                else
                {
                    _output.WriteLine("  No tracks to add.");
                }
                _returnToMain = true;
                return;
            }

            if (int.TryParse(input, out var idx) && idx >= 1 && idx <= albums.Count)
            {
                await DisplayTracksByAlbumAsync(albums[idx - 1].AlbumId);
                if (_returnToMain) return;
            }
            else
            {
                _output.WriteLine("Invalid selection.");
            }
        }
    }

    /// <summary>
    /// Lists tracks for the given album. Offers Q/A to queue all album tracks,
    /// and number selection for individual track queue actions.
    /// </summary>
    public async Task DisplayTracksByAlbumAsync(int albumId)
    {
        while (true)
        {
            var albumResult = await _browser.GetAlbumWithTracksAsync(albumId);

            if (!albumResult.IsSuccess || albumResult.Value == null)
            {
                _output.WriteLine("Album not found.");
                return;
            }
            var album = albumResult.Value;
            var tracks = album.Tracks.ToList();

            if (tracks.Count == 0)
            {
                _output.WriteLine();
                _output.WriteLine($"== {album.Title} ==");
                _output.WriteLine();
                _output.WriteLine("  No tracks in this album.");
                return;
            }

            _onClear?.Invoke();
            if (_statusMessage != null)
            {
                _output.WriteLine(_statusMessage);
                _statusMessage = null;
            }
            Print.WriteSelectionMenu(_output, album.Title,
                tracks.Select(t =>
                {
                    var durationStr = t.Duration.HasValue
                        ? $" [{t.Duration.Value.Minutes}:{t.Duration.Value.Seconds:D2}]"
                        : "";
                    return $"{t.DisplayTitle}{durationStr}";
                }).ToList(),
                prompt: null, backLabel: null);

            WriteDivider();
            WriteQueueOptions($"all from {album.Title}");
            _output.WriteLine("  0) Back");
            WritePrompt();

            var input = _input.ReadLine()?.Trim();
            if (input == "0" || string.IsNullOrEmpty(input))
                return;

            if (input.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                var trackIds = tracks.Select(t => t.TrackId).ToList();
                _playback.SetQueue(trackIds);
                _output.WriteLine($"  Set queue: {trackIds.Count} tracks from {album.Title}.");
                _returnToMain = true;
                return;
            }

            if (input.Equals("a", StringComparison.OrdinalIgnoreCase))
            {
                var trackIds = tracks.Select(t => t.TrackId).ToList();
                _playback.AppendToQueue(trackIds);
                _output.WriteLine($"  Added {trackIds.Count} tracks from {album.Title} to queue.");
                _returnToMain = true;
                return;
            }

            if (int.TryParse(input, out var idx) && idx >= 1 && idx <= tracks.Count)
            {
                await QueueTrackAsync(tracks[idx - 1].TrackId);
                if (_returnToMain) return;
            }
            else
            {
                _output.WriteLine("Invalid selection.");
            }
        }
    }

    /// <summary>
    /// Shows track details and offers Q (set as queue) / A (add to queue) for the selected track.
    /// </summary>
    public async Task QueueTrackAsync(int trackId)
    {
        var trackResult = await _browser.GetTrackByIdAsync(trackId);
        if (!trackResult.IsSuccess || trackResult.Value == null)
        {
            _output.WriteLine("Track not found.");
            return;
        }
        var track = trackResult.Value;

        var fields = new List<(string Label, string Value)>
        {
            ("Title:", track.DisplayTitle),
            ("Artist:", track.DisplayArtist),
            ("Album:", track.DisplayAlbum)
        };
        if (track.Duration.HasValue)
            fields.Add(("Duration:", $"{track.Duration.Value.Minutes}:{track.Duration.Value.Seconds:D2}"));
        if (track.BitRate.HasValue)
            fields.Add(("BitRate:", $"{track.BitRate}kbps"));
        fields.Add(("Plays:", track.PlayCount.ToString()));
        if (track.Rating.HasValue)
            fields.Add(("Rating:", $"{track.Rating}/5"));

        Print.WriteDetailCard(_output, "Track Details", fields);

        WriteDivider();
        _output.WriteLine("  Q) Play now (replace queue with this track)");
        _output.WriteLine("  A) Add to end of queue");
        _output.WriteLine("  0) Back");
        WritePrompt();

        var input = _input.ReadLine()?.Trim();

        if (input?.Equals("q", StringComparison.OrdinalIgnoreCase) == true)
        {
            _playback.SetQueue(new[] { track.TrackId });
            _output.WriteLine($"  Set queue: {track.DisplayTitle}.");
            _returnToMain = true;
        }
        else if (input?.Equals("a", StringComparison.OrdinalIgnoreCase) == true)
        {
            _playback.AppendToQueue(new List<int> { track.TrackId });
            _output.WriteLine($"  Added {track.DisplayTitle} to queue.");
            _returnToMain = true;
        }
    }

    private void WriteQueueOptions(string description)
    {
        _output.WriteLine($"  Q) Play {description} now (replace queue)");
        _output.WriteLine($"  A) Add {description} to end of queue");
    }

    private void WriteDivider()
    {
        _output.WriteLine();
        _output.WriteLine("-----------------");
        _output.WriteLine();
    }

    private void WritePrompt()
    {
        WriteDivider();
        _output.Write("Select: ");
    }
}
