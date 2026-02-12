using FsmpLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace FsmpDataAcsses.Repositories;

/// <summary>
/// Specialized repository for Playlist entities with playlist-specific queries.
/// </summary>
public class PlaylistRepository : Repository<Playlist>
{
    public PlaylistRepository(FsmpDbContext context) : base(context)
    {
    }

    public async Task<Playlist?> GetWithTracksAsync(int playlistId)
    {
        return await DbSet
            .Include(p => p.PlaylistTracks)
            .FirstOrDefaultAsync(p => p.PlaylistId == playlistId);
    }

    public async Task<Playlist?> GetByNameAsync(string name)
    {
        return await DbSet.FirstOrDefaultAsync(p => p.Name == name);
    }

    public async Task<IEnumerable<Playlist>> SearchAsync(string searchTerm)
    {
        return await DbSet
            .Where(p => p.Name.Contains(searchTerm))
            .ToListAsync();
    }

    public async Task<IEnumerable<Playlist>> GetRecentAsync(int count)
    {
        return await DbSet
            .OrderByDescending(p => p.UpdatedAt)
            .Take(count)
            .ToListAsync();
    }
}