using FSMP.Core;
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
    private readonly ActivePlaylistService _activePlaylist;
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public BrowseUI(UnitOfWork unitOfWork, IAudioService audioService, ActivePlaylistService activePlaylist, TextReader input, TextWriter output)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _activePlaylist = activePlaylist ?? throw new ArgumentNullException(nameof(activePlaylist));
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
            _output.WriteLine("\nNo artists in library. Scan a library first.");
            return;
        }

        Print.WriteSelectionMenu(_output, "Artists",
            artists.Select(a => a.Name).ToList(),
            "Select artist");

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

        Print.WriteSelectionMenu(_output, $"Albums by {artist.Name}",
            albums.Select(a =>
            {
                var yearStr = a.Year.HasValue ? $" ({a.Year})" : "";
                return $"{a.Title}{yearStr}";
            }).ToList(),
            "Select album");

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

        if (tracks.Count == 0)
        {
            _output.WriteLine();
            _output.WriteLine($"== {album.Title} ==");
            _output.WriteLine();
            _output.WriteLine("  No tracks in this album.");
            return;
        }

        Print.WriteSelectionMenu(_output, album.Title,
            tracks.Select(t =>
            {
                var durationStr = t.Duration.HasValue
                    ? $" [{t.Duration.Value.Minutes}:{t.Duration.Value.Seconds:D2}]"
                    : "";
                return $"{t.DisplayTitle}{durationStr}";
            }).ToList(),
            "Select track");

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

        Print.WriteDetailCard(_output, "Now Playing", fields);

        // Add to active playlist queue and play
        if (_activePlaylist.Count == 0)
        {
            _activePlaylist.SetQueue(new[] { track.TrackId });
        }
        else
        {
            // Append to end of queue — set queue to current + new track
            var currentQueue = _activePlaylist.PlayOrder.ToList();
            if (!currentQueue.Contains(track.TrackId))
            {
                currentQueue.Add(track.TrackId);
                var currentIdx = _activePlaylist.CurrentIndex;
                _activePlaylist.SetQueue(currentQueue);
                if (currentIdx >= 0)
                    _activePlaylist.JumpTo(currentIdx);
            }
        }

        await _audioService.PlayTrackAsync(track);
        _output.WriteLine("  Added to player queue.");
    }
}
