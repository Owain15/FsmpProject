using FsmpLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace FsmpDataAcsses.Repositories;

/// <summary>
/// Specialized repository for Track entities with music-specific queries.
/// </summary>
public class TrackRepository : Repository<Track>
{
    public TrackRepository(FsmpDbContext context) : base(context)
    {
    }

    public async Task<Track?> GetByFilePathAsync(string filePath)
    {
        return await DbSet.FirstOrDefaultAsync(t => t.FilePath == filePath);
    }

    public async Task<IEnumerable<Track>> GetFavoritesAsync()
    {
        return await DbSet.Where(t => t.IsFavorite).ToListAsync();
    }

    public async Task<IEnumerable<Track>> GetMostPlayedAsync(int count)
    {
        return await DbSet
            .OrderByDescending(t => t.PlayCount)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Track>> GetRecentlyPlayedAsync(int count)
    {
        return await DbSet
            .Where(t => t.LastPlayedAt != null)
            .OrderByDescending(t => t.LastPlayedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Track?> GetByFileHashAsync(string fileHash)
    {
        return await DbSet.FirstOrDefaultAsync(t => t.FileHash == fileHash);
    }
}
