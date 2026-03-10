using FSMP.Core.Interfaces;
using FSMP.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FsmpDataAcsses.Repositories;

/// <summary>
/// Specialized repository for Tags entities with tag-specific queries.
/// </summary>
public class TagRepository : Repository<Tags>, ITagRepository
{
    public TagRepository(FsmpDbContext context) : base(context)
    {
    }

    public new async Task<Tags?> GetByIdAsync(int id)
    {
        return await DbSet.FirstOrDefaultAsync(t => t.TagId == id);
    }

    public async Task<Tags?> GetByNameAsync(string name)
    {
        return await DbSet.FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task<IEnumerable<Tags>> GetTagsForTrackAsync(int trackId)
    {
        var track = await Context.Set<Track>()
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.TrackId == trackId);
        return track?.Tags ?? new List<Tags>();
    }

    public async Task<IEnumerable<Tags>> GetTagsForAlbumAsync(int albumId)
    {
        var album = await Context.Set<Album>()
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.AlbumId == albumId);
        return album?.Tags ?? new List<Tags>();
    }

    public async Task<IEnumerable<Tags>> GetTagsForArtistAsync(int artistId)
    {
        var artist = await Context.Set<Artist>()
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.ArtistId == artistId);
        return artist?.Tags ?? new List<Tags>();
    }
}
