using FSMP.Core.Models;

namespace FSMP.Core.Interfaces;

public interface ITagRepository
{
    Task<Tags?> GetByIdAsync(int id);
    Task<Tags?> GetByNameAsync(string name);
    Task<IEnumerable<Tags>> GetAllAsync();
    Task<IEnumerable<Tags>> GetTagsForTrackAsync(int trackId);
    Task<IEnumerable<Tags>> GetTagsForAlbumAsync(int albumId);
    Task<IEnumerable<Tags>> GetTagsForArtistAsync(int artistId);
    Task AddAsync(Tags tag);
    void Remove(Tags tag);
}
