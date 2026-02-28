using FSMP.Core.Models;

namespace FSMP.Core.Interfaces;

public interface IAlbumRepository
{
    Task<IEnumerable<Album>> GetByArtistAsync(int artistId);
    Task<Album?> GetWithTracksAsync(int albumId);
}
