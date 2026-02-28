using FSMP.Core.Models;

namespace FSMP.Core.Interfaces;

public interface ILibraryBrowser
{
    Task<Result<List<Artist>>> GetAllArtistsAsync();
    Task<Result<Artist?>> GetArtistByIdAsync(int artistId);
    Task<Result<List<Album>>> GetAlbumsByArtistAsync(int artistId);
    Task<Result<Album?>> GetAlbumWithTracksAsync(int albumId);
    Task<Result<Track?>> GetTrackByIdAsync(int trackId);
    Task<Result<List<int>>> GetAllTrackIdsByArtistAsync(int artistId);
    Task<Result<List<int>>> GetAllTrackIdsAsync();
}
