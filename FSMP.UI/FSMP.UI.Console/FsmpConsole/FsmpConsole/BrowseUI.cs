using FSMP.Core;
using FsmpDataAcsses;
using FSMP.Core.Models;
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
    private readonly ActivePlaylistService _activePlaylist;
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly Action? _onClear;

    public BrowseUI(UnitOfWork unitOfWork, IAudioService audioService, ActivePlaylistService activePlaylist, TextReader input, TextWriter output, Action? onClear = null)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _activePlaylist = activePlaylist ?? throw new ArgumentNullException(nameof(activePlaylist));
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _onClear = onClear;
    }

    /// <summary>
    /// Entry point — displays the artist list.
    /// </summary>
    public async Task RunAsync()
    {
        await DisplayArtistsAsync();
    }

    /// <summary>
    /// Lists all artists and lets the user select one to browse albums,
    /// or queue all tracks.
    /// </summary>
    public async Task DisplayArtistsAsync()
    {
        var artists = (await _unitOfWork.Artists.GetAllAsync()).ToList();

        if (artists.Count == 0)
        {
            _output.WriteLine("\nNo artists in library. Scan a library first.");
            return;
        }

        _onClear?.Invoke();
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
            var allTrackIds = new List<int>();
            foreach (var artist in artists)
            {
                var a = await _unitOfWork.Artists.GetWithTracksAsync(artist.ArtistId);
                if (a?.Tracks != null)
                    allTrackIds.AddRange(a.Tracks.Select(t => t.TrackId));
            }
            if (allTrackIds.Count > 0)
            {
                _activePlaylist.SetQueue(allTrackIds);
                _output.WriteLine($"  Set queue: {allTrackIds.Count} tracks.");
            }
            else
            {
                _output.WriteLine("  No tracks to queue.");
            }
            return;
        }

        if (input.Equals("a", StringComparison.OrdinalIgnoreCase))
        {
            var allTrackIds = new List<int>();
            foreach (var artist in artists)
            {
                var a = await _unitOfWork.Artists.GetWithTracksAsync(artist.ArtistId);
                if (a?.Tracks != null)
                    allTrackIds.AddRange(a.Tracks.Select(t => t.TrackId));
            }
            if (allTrackIds.Count > 0)
            {
                AppendToQueue(allTrackIds);
                _output.WriteLine($"  Added {allTrackIds.Count} tracks to queue.");
            }
            else
            {
                _output.WriteLine("  No tracks to add.");
            }
            return;
        }

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
    /// Lists albums for the given artist. Offers Q/A to queue all tracks by artist.
    /// </summary>
    public async Task DisplayAlbumsByArtistAsync(int artistId)
    {
        var albums = (await _unitOfWork.Albums.GetByArtistAsync(artistId)).ToList();
        var artist = await _unitOfWork.Artists.GetByIdAsync(artistId);

        if (artist == null)
        {
            _output.WriteLine("\nArtist not found.");
            return;
        }

        if (albums.Count == 0)
        {
            _output.WriteLine();
            _output.WriteLine($"== Albums by {artist.Name} ==");
            _output.WriteLine();
            _output.WriteLine("  No albums found.");
            return;
        }

        _onClear?.Invoke();
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
            var artistWithTracks = await _unitOfWork.Artists.GetWithTracksAsync(artistId);
            var trackIds = artistWithTracks?.Tracks?.Select(t => t.TrackId).ToList() ?? new List<int>();
            if (trackIds.Count > 0)
            {
                _activePlaylist.SetQueue(trackIds);
                _output.WriteLine($"  Set queue: {trackIds.Count} tracks by {artist.Name}.");
            }
            else
            {
                _output.WriteLine("  No tracks to queue.");
            }
            return;
        }

        if (input.Equals("a", StringComparison.OrdinalIgnoreCase))
        {
            var artistWithTracks = await _unitOfWork.Artists.GetWithTracksAsync(artistId);
            var trackIds = artistWithTracks?.Tracks?.Select(t => t.TrackId).ToList() ?? new List<int>();
            if (trackIds.Count > 0)
            {
                AppendToQueue(trackIds);
                _output.WriteLine($"  Added {trackIds.Count} tracks by {artist.Name} to queue.");
            }
            else
            {
                _output.WriteLine("  No tracks to add.");
            }
            return;
        }

        if (int.TryParse(input, out var idx) && idx >= 1 && idx <= albums.Count)
        {
            await DisplayTracksByAlbumAsync(albums[idx - 1].AlbumId);
        }
        else
        {
            _output.WriteLine("Invalid selection.");
        }
    }

    /// <summary>
    /// Lists tracks for the given album. Offers Q/A to queue all album tracks,
    /// and number selection for individual track queue actions.
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

        if (tracks.Count == 0)
        {
            _output.WriteLine();
            _output.WriteLine($"== {album.Title} ==");
            _output.WriteLine();
            _output.WriteLine("  No tracks in this album.");
            return;
        }

        _onClear?.Invoke();
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
            _activePlaylist.SetQueue(trackIds);
            _output.WriteLine($"  Set queue: {trackIds.Count} tracks from {album.Title}.");
            return;
        }

        if (input.Equals("a", StringComparison.OrdinalIgnoreCase))
        {
            var trackIds = tracks.Select(t => t.TrackId).ToList();
            AppendToQueue(trackIds);
            _output.WriteLine($"  Added {trackIds.Count} tracks from {album.Title} to queue.");
            return;
        }

        if (int.TryParse(input, out var idx) && idx >= 1 && idx <= tracks.Count)
        {
            await QueueTrackAsync(tracks[idx - 1].TrackId);
        }
        else
        {
            _output.WriteLine("Invalid selection.");
        }
    }

    /// <summary>
    /// Shows track details and offers Q (set as queue) / A (add to queue) for the selected track.
    /// </summary>
    public async Task QueueTrackAsync(int trackId)
    {
        var track = await _unitOfWork.Tracks.GetByIdAsync(trackId);

        if (track == null)
        {
            _output.WriteLine("Track not found.");
            return;
        }

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
            _activePlaylist.SetQueue(new[] { track.TrackId });
            _output.WriteLine($"  Set queue: {track.DisplayTitle}.");
        }
        else if (input?.Equals("a", StringComparison.OrdinalIgnoreCase) == true)
        {
            AppendToQueue(new List<int> { track.TrackId });
            _output.WriteLine($"  Added {track.DisplayTitle} to queue.");
        }
    }

    /// <summary>
    /// Writes the Q and A queue option lines to the output.
    /// </summary>
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

    private void AppendToQueue(List<int> trackIds)
    {
        if (_activePlaylist.Count == 0)
        {
            _activePlaylist.SetQueue(trackIds);
            return;
        }

        var currentQueue = _activePlaylist.PlayOrder.ToList();
        var currentIdx = _activePlaylist.CurrentIndex;

        foreach (var id in trackIds)
        {
            if (!currentQueue.Contains(id))
                currentQueue.Add(id);
        }

        _activePlaylist.SetQueue(currentQueue);
        if (currentIdx >= 0)
            _activePlaylist.JumpTo(currentIdx);
    }
}
