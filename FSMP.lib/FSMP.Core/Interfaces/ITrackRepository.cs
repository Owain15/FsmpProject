using FSMP.Core.Models;

namespace FSMP.Core.Interfaces;

public interface ITrackRepository
{
    Task<Track?> GetByIdAsync(int id);
}
