using FsmpLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace FsmpDataAcsses.Repositories;

/// <summary>
/// Specialized repository for PlaybackHistory entities with listening statistics queries.
/// </summary>
public class PlaybackHistoryRepository : Repository<PlaybackHistory>
{
    public PlaybackHistoryRepository(FsmpDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PlaybackHistory>> GetRecentPlaysAsync(int count)
    {
        return await DbSet
            .OrderByDescending(ph => ph.PlayedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<PlaybackHistory>> GetByTrackAsync(int trackId)
    {
        return await DbSet
            .Where(ph => ph.TrackId == trackId)
            .OrderByDescending(ph => ph.PlayedAt)
            .ToListAsync();
    }

    public async Task<int> GetTotalPlayCountAsync()
    {
        return await DbSet.CountAsync();
    }

    public async Task<TimeSpan> GetTotalListeningTimeAsync()
    {
        var durations = await DbSet
            .Where(ph => ph.PlayDuration != null)
            .Select(ph => ph.PlayDuration!.Value)
            .ToListAsync();

        var totalTicks = durations.Sum(d => d.Ticks);
        return TimeSpan.FromTicks(totalTicks);
    }
}
