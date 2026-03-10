using FSMP.Core.Models;

namespace FSMP.Core.Interfaces;

public interface ITagService
{
    Task<Result<List<Tags>>> GetAllTagsAsync();
    Task<Result<Tags>> CreateTagAsync(string name);
    Task<Result> DeleteTagAsync(int tagId);
    Task<Result<List<Tags>>> GetTagsForTrackAsync(int trackId);
    Task<Result<List<Tags>>> GetTagsForAlbumAsync(int albumId);
    Task<Result<List<Tags>>> GetTagsForArtistAsync(int artistId);
    Task<Result> AddTagToTrackAsync(int trackId, int tagId);
    Task<Result> RemoveTagFromTrackAsync(int trackId, int tagId);
    Task<Result> AddTagToAlbumAsync(int albumId, int tagId);
    Task<Result> RemoveTagFromAlbumAsync(int albumId, int tagId);
    Task<Result> AddTagToArtistAsync(int artistId, int tagId);
    Task<Result> RemoveTagFromArtistAsync(int artistId, int tagId);
}
