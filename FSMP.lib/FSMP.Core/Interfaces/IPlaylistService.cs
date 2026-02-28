using FSMP.Core.Models;

namespace FSMP.Core.Interfaces;

public interface IPlaylistService
{
    Task<IEnumerable<Playlist>> GetAllPlaylistsAsync();
    Task<Playlist?> GetPlaylistWithTracksAsync(int playlistId);
    Task<Playlist> CreatePlaylistAsync(string name, string? description = null);
    Task DeletePlaylistAsync(int playlistId);
    Task AddTrackAsync(int playlistId, int trackId);
    Task RemoveTrackAtPositionAsync(int playlistId, int position);
    Task RenamePlaylistAsync(int playlistId, string newName);
    Task<IEnumerable<Playlist>> SearchPlaylistsAsync(string searchTerm);
}
