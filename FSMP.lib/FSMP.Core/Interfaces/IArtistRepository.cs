using FSMP.Core.Models;

namespace FSMP.Core.Interfaces;

public interface IArtistRepository
{
    Task<IEnumerable<Artist>> GetAllAsync();
    Task<Artist?> GetByIdAsync(int id);
    Task<Artist?> GetWithTracksAsync(int id);
}
