using FsmpLibrary.Models;

namespace FsmpDataAcsses.Services;

/// <summary>
/// Manages playlist CRUD operations and track assignment via the database.
/// </summary>
public class PlaylistService
{
    private readonly UnitOfWork _unitOfWork;

    public PlaylistService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Creates a new playlist with the given name and optional description.
    /// </summary>
    public async Task<Playlist> CreatePlaylistAsync(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Playlist name cannot be empty.", nameof(name));

        var playlist = new Playlist
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Playlists.AddAsync(playlist);
        await _unitOfWork.SaveAsync();
        return playlist;
    }

    /// <summary>
    /// Gets all playlists ordered by most recently updated.
    /// </summary>
    public async Task<IEnumerable<Playlist>> GetAllPlaylistsAsync()
    {
        return await _unitOfWork.Playlists.GetRecentAsync(int.MaxValue);
    }

    /// <summary>
    /// Gets a playlist by ID with its tracks loaded.
    /// </summary>
    public async Task<Playlist?> GetPlaylistWithTracksAsync(int playlistId)
    {
        return await _unitOfWork.Playlists.GetWithTracksAsync(playlistId);
    }

    /// <summary>
    /// Renames a playlist.
    /// </summary>
    public async Task RenamePlaylistAsync(int playlistId, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Playlist name cannot be empty.", nameof(newName));

        var playlist = await _unitOfWork.Playlists.GetByIdAsync(playlistId);
        if (playlist == null)
            throw new InvalidOperationException($"Playlist with ID {playlistId} not found.");

        playlist.Name = newName.Trim();
        playlist.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Playlists.Update(playlist);
        await _unitOfWork.SaveAsync();
    }

    /// <summary>
    /// Deletes a playlist and all its track associations.
    /// </summary>
    public async Task DeletePlaylistAsync(int playlistId)
    {
        var playlist = await _unitOfWork.Playlists.GetByIdAsync(playlistId);
        if (playlist == null)
            throw new InvalidOperationException($"Playlist with ID {playlistId} not found.");

        _unitOfWork.Playlists.Remove(playlist);
        await _unitOfWork.SaveAsync();
    }

    /// <summary>
    /// Adds a track to the end of a playlist.
    /// </summary>
    public async Task AddTrackAsync(int playlistId, int trackId)
    {
        var playlist = await _unitOfWork.Playlists.GetWithTracksAsync(playlistId);
        if (playlist == null)
            throw new InvalidOperationException($"Playlist with ID {playlistId} not found.");

        var track = await _unitOfWork.Tracks.GetByIdAsync(trackId);
        if (track == null)
            throw new InvalidOperationException($"Track with ID {trackId} not found.");

        var nextPosition = playlist.PlaylistTracks.Count > 0
            ? playlist.PlaylistTracks.Max(pt => pt.Position) + 1
            : 0;

        var playlistTrack = new PlaylistTrack
        {
            PlaylistId = playlistId,
            TrackId = trackId,
            Position = nextPosition,
            AddedAt = DateTime.UtcNow
        };

        playlist.PlaylistTracks.Add(playlistTrack);
        playlist.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Playlists.Update(playlist);
        await _unitOfWork.SaveAsync();
    }

    /// <summary>
    /// Removes a track from a playlist by its position and reorders remaining tracks.
    /// </summary>
    public async Task RemoveTrackAtPositionAsync(int playlistId, int position)
    {
        var playlist = await _unitOfWork.Playlists.GetWithTracksAsync(playlistId);
        if (playlist == null)
            throw new InvalidOperationException($"Playlist with ID {playlistId} not found.");

        var trackToRemove = playlist.PlaylistTracks.FirstOrDefault(pt => pt.Position == position);
        if (trackToRemove == null)
            throw new InvalidOperationException($"No track at position {position} in playlist.");

        playlist.PlaylistTracks.Remove(trackToRemove);

        // Reorder remaining tracks
        var remaining = playlist.PlaylistTracks.OrderBy(pt => pt.Position).ToList();
        for (int i = 0; i < remaining.Count; i++)
        {
            remaining[i].Position = i;
        }

        playlist.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Playlists.Update(playlist);
        await _unitOfWork.SaveAsync();
    }

    /// <summary>
    /// Searches playlists by name.
    /// </summary>
    public async Task<IEnumerable<Playlist>> SearchPlaylistsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllPlaylistsAsync();

        return await _unitOfWork.Playlists.SearchAsync(searchTerm.Trim());
    }
}