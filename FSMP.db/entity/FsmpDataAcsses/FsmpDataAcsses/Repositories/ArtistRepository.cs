using FSMP.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FsmpDataAcsses.Repositories;

/// <summary>
/// Specialized repository for Artist entities with music-specific queries.
/// </summary>
public class ArtistRepository : Repository<Artist>
{
    public ArtistRepository(FsmpDbContext context) : base(context)
    {
    }

    public async Task<Artist?> GetWithAlbumsAsync(int artistId)
    {
        return await DbSet
            .Include(a => a.Albums)
            .FirstOrDefaultAsync(a => a.ArtistId == artistId);
    }

    public async Task<Artist?> GetWithTracksAsync(int artistId)
    {
        return await DbSet
            .Include(a => a.Tracks)
            .FirstOrDefaultAsync(a => a.ArtistId == artistId);
    }

    public async Task<IEnumerable<Artist>> SearchAsync(string searchTerm)
    {
        return await DbSet
            .Where(a => a.Name.Contains(searchTerm))
            .ToListAsync();
    }
}
