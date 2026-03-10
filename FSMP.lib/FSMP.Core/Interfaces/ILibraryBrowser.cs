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
    Task<Result<List<Tags>>> GetAllTagsAsync();
    Task<Result<List<Artist>>> GetArtistsByTagAsync(int tagId);
    Task<Result<List<Album>>> GetAlbumsByTagAsync(int tagId);
    Task<Result<List<Track>>> GetTracksByTagAsync(int tagId);
}
