using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;

namespace FsmpLibrary.Services;

public class PlaylistManager : IPlaylistManager
{
    private readonly IPlaylistService _playlistService;
    private readonly IActivePlaylistService _activePlaylist;

    public PlaylistManager(
        IPlaylistService playlistService,
        IActivePlaylistService activePlaylist)
    {
        _playlistService = playlistService ?? throw new ArgumentNullException(nameof(playlistService));
        _activePlaylist = activePlaylist ?? throw new ArgumentNullException(nameof(activePlaylist));
    }

    public async Task<Result<List<Playlist>>> GetAllPlaylistsAsync()
    {
        try
        {
            var playlists = (await _playlistService.GetAllPlaylistsAsync()).ToList();
            return Result.Success(playlists);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Playlist>>($"Error loading playlists: {ex.Message}");
        }
    }

    public async Task<Result<Playlist?>> GetPlaylistWithTracksAsync(int playlistId)
    {
        try
        {
            var playlist = await _playlistService.GetPlaylistWithTracksAsync(playlistId);
            return Result.Success<Playlist?>(playlist);
        }
        catch (Exception ex)
        {
            return Result.Failure<Playlist?>($"Error loading playlist: {ex.Message}");
        }
    }

    public async Task<Result<Playlist>> CreatePlaylistAsync(string name, string? description = null)
    {
        try
        {
            var playlist = await _playlistService.CreatePlaylistAsync(name, description);
            return Result.Success(playlist);
        }
        catch (Exception ex)
        {
            return Result.Failure<Playlist>($"Error creating playlist: {ex.Message}");
        }
    }

    public async Task<Result> DeletePlaylistAsync(int playlistId)
    {
        try
        {
            await _playlistService.DeletePlaylistAsync(playlistId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting playlist: {ex.Message}");
        }
    }

    public async Task<Result> LoadPlaylistIntoQueueAsync(int playlistId)
    {
        try
        {
            var playlist = await _playlistService.GetPlaylistWithTracksAsync(playlistId);
            if (playlist == null)
                return Result.Failure("Playlist not found.");

            var tracks = playlist.PlaylistTracks.OrderBy(pt => pt.Position).ToList();
            if (tracks.Count == 0)
                return Result.Failure("No tracks to load.");

            _activePlaylist.SetQueue(tracks.Select(pt => pt.TrackId).ToList());
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error loading playlist: {ex.Message}");
        }
    }
}
