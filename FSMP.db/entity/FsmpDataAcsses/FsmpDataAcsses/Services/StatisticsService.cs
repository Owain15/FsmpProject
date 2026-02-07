using FsmpLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace FsmpDataAcsses.Services;

/// <summary>
/// Provides library and playback statistics queries.
/// </summary>
public class StatisticsService
{
    private readonly UnitOfWork _unitOfWork;

    public StatisticsService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets the tracks with the highest play counts.
    /// </summary>
    public async Task<IEnumerable<Track>> GetMostPlayedTracksAsync(int count)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");

        return await _unitOfWork.Tracks.GetMostPlayedAsync(count);
    }

    /// <summary>
    /// Gets the most recently played tracks.
    /// </summary>
    public async Task<IEnumerable<Track>> GetRecentlyPlayedTracksAsync(int count)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");

        return await _unitOfWork.Tracks.GetRecentlyPlayedAsync(count);
    }

    /// <summary>
    /// Gets all tracks marked as favorites.
    /// </summary>
    public async Task<IEnumerable<Track>> GetFavoritesAsync()
    {
        return await _unitOfWork.Tracks.GetFavoritesAsync();
    }

    /// <summary>
    /// Gets the number of tracks per genre.
    /// </summary>
    public async Task<Dictionary<string, int>> GetGenreStatisticsAsync()
    {
        var genres = await _unitOfWork.Genres.GetAllAsync();
        var result = new Dictionary<string, int>();

        foreach (var genre in genres)
        {
            var tracks = await _unitOfWork.Tracks.FindAsync(t => t.Genres.Any(g => g.GenreId == genre.GenreId));
            var trackCount = tracks.Count();
            if (trackCount > 0)
                result[genre.Name] = trackCount;
        }

        return result;
    }

    /// <summary>
    /// Gets the total number of completed playback events.
    /// </summary>
    public async Task<int> GetTotalPlayCountAsync()
    {
        return await _unitOfWork.PlaybackHistories.GetTotalPlayCountAsync();
    }

    /// <summary>
    /// Gets the total listening time across all playback history.
    /// </summary>
    public async Task<TimeSpan> GetTotalListeningTimeAsync()
    {
        return await _unitOfWork.PlaybackHistories.GetTotalListeningTimeAsync();
    }

    /// <summary>
    /// Gets the total number of tracks in the library.
    /// </summary>
    public async Task<int> GetTotalTrackCountAsync()
    {
        return await _unitOfWork.Tracks.CountAsync();
    }
}
