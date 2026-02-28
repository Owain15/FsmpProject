using FSMP.Core.Models;

namespace FSMP.Core.Interfaces;

public interface IPlaylistManager
{
    Task<Result<List<Playlist>>> GetAllPlaylistsAsync();
    Task<Result<Playlist?>> GetPlaylistWithTracksAsync(int playlistId);
    Task<Result<Playlist>> CreatePlaylistAsync(string name, string? description = null);
    Task<Result> DeletePlaylistAsync(int playlistId);
    Task<Result> LoadPlaylistIntoQueueAsync(int playlistId);
}
