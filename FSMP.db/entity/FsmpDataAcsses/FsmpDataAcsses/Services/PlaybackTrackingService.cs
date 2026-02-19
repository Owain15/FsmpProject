using FSMP.Core.Models;

namespace FsmpDataAcsses.Services;

/// <summary>
/// Records playback history and updates track statistics in the database.
/// </summary>
public class PlaybackTrackingService
{
    private readonly UnitOfWork _unitOfWork;

    public PlaybackTrackingService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Records a playback event and updates the track's statistics.
    /// </summary>
    public async Task RecordPlaybackAsync(Track track, TimeSpan playDuration, bool completed, bool skipped)
    {
        ArgumentNullException.ThrowIfNull(track);

        var history = new PlaybackHistory
        {
            TrackId = track.TrackId,
            PlayedAt = DateTime.UtcNow,
            PlayDuration = playDuration,
            CompletedPlayback = completed,
            WasSkipped = skipped,
        };

        await _unitOfWork.PlaybackHistories.AddAsync(history);

        // Update track statistics
        track.LastPlayedAt = DateTime.UtcNow;

        if (completed)
            track.PlayCount++;

        if (skipped)
            track.SkipCount++;

        track.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Tracks.Update(track);

        await _unitOfWork.SaveAsync();
    }

    /// <summary>
    /// Gets the playback history for a specific track.
    /// </summary>
    public async Task<IEnumerable<PlaybackHistory>> GetTrackHistoryAsync(int trackId)
    {
        return await _unitOfWork.PlaybackHistories.GetByTrackAsync(trackId);
    }

    /// <summary>
    /// Gets the most recent playback records.
    /// </summary>
    public async Task<IEnumerable<PlaybackHistory>> GetRecentPlaysAsync(int count = 10)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");

        return await _unitOfWork.PlaybackHistories.GetRecentPlaysAsync(count);
    }
}
