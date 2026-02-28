using FSMP.Core.Interfaces;
using FSMP.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FsmpDataAcsses.Repositories;

/// <summary>
/// Specialized repository for Album entities with music-specific queries.
/// </summary>
public class AlbumRepository : Repository<Album>, IAlbumRepository
{
    public AlbumRepository(FsmpDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Album>> GetByArtistAsync(int artistId)
    {
        return await DbSet.Where(a => a.ArtistId == artistId).ToListAsync();
    }

    public async Task<IEnumerable<Album>> GetByYearAsync(int year)
    {
        return await DbSet.Where(a => a.Year == year).ToListAsync();
    }

    public async Task<Album?> GetWithTracksAsync(int albumId)
    {
        return await DbSet
            .Include(a => a.Tracks)
            .FirstOrDefaultAsync(a => a.AlbumId == albumId);
    }
}
