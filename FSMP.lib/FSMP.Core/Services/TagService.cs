using FSMP.Core.Interfaces;
using FSMP.Core.Models;

namespace FSMP.Core.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly ITrackRepository _trackRepository;
    private readonly IAlbumRepository _albumRepository;
    private readonly IArtistRepository _artistRepository;
    private readonly Func<Task<int>> _saveAsync;

    public TagService(
        ITagRepository tagRepository,
        ITrackRepository trackRepository,
        IAlbumRepository albumRepository,
        IArtistRepository artistRepository,
        Func<Task<int>> saveAsync)
    {
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
        _albumRepository = albumRepository ?? throw new ArgumentNullException(nameof(albumRepository));
        _artistRepository = artistRepository ?? throw new ArgumentNullException(nameof(artistRepository));
        _saveAsync = saveAsync ?? throw new ArgumentNullException(nameof(saveAsync));
    }

    public async Task<Result<List<Tags>>> GetAllTagsAsync()
    {
        try
        {
            var tags = (await _tagRepository.GetAllAsync()).ToList();
            return Result.Success(tags);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Tags>>($"Error loading tags: {ex.Message}");
        }
    }

    public async Task<Result<Tags>> CreateTagAsync(string name)
    {
        try
        {
            var existing = await _tagRepository.GetByNameAsync(name);
            if (existing != null)
                return Result.Failure<Tags>($"Tag '{name}' already exists.");

            var tag = new Tags { Name = name };
            await _tagRepository.AddAsync(tag);
            await _saveAsync();
            return Result.Success(tag);
        }
        catch (Exception ex)
        {
            return Result.Failure<Tags>($"Error creating tag: {ex.Message}");
        }
    }

    public async Task<Result> DeleteTagAsync(int tagId)
    {
        try
        {
            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag == null)
                return Result.Failure($"Tag with ID {tagId} not found.");

            _tagRepository.Remove(tag);
            await _saveAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting tag: {ex.Message}");
        }
    }

    public async Task<Result<List<Tags>>> GetTagsForTrackAsync(int trackId)
    {
        try
        {
            var tags = (await _tagRepository.GetTagsForTrackAsync(trackId)).ToList();
            return Result.Success(tags);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Tags>>($"Error loading tags: {ex.Message}");
        }
    }

    public async Task<Result<List<Tags>>> GetTagsForAlbumAsync(int albumId)
    {
        try
        {
            var tags = (await _tagRepository.GetTagsForAlbumAsync(albumId)).ToList();
            return Result.Success(tags);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Tags>>($"Error loading tags: {ex.Message}");
        }
    }

    public async Task<Result<List<Tags>>> GetTagsForArtistAsync(int artistId)
    {
        try
        {
            var tags = (await _tagRepository.GetTagsForArtistAsync(artistId)).ToList();
            return Result.Success(tags);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Tags>>($"Error loading tags: {ex.Message}");
        }
    }

    public async Task<Result> AddTagToTrackAsync(int trackId, int tagId)
    {
        try
        {
            var track = await _trackRepository.GetByIdAsync(trackId);
            if (track == null)
                return Result.Failure($"Track with ID {trackId} not found.");

            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag == null)
                return Result.Failure($"Tag with ID {tagId} not found.");

            // Load existing tags for the track
            var existingTags = (await _tagRepository.GetTagsForTrackAsync(trackId)).ToList();
            if (existingTags.Any(t => t.TagId == tagId))
                return Result.Failure("Tag is already assigned to this track.");

            track.Tags.Add(tag);
            await _saveAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error adding tag to track: {ex.Message}");
        }
    }

    public async Task<Result> RemoveTagFromTrackAsync(int trackId, int tagId)
    {
        try
        {
            var track = await _trackRepository.GetByIdAsync(trackId);
            if (track == null)
                return Result.Failure($"Track with ID {trackId} not found.");

            // Load existing tags
            var existingTags = (await _tagRepository.GetTagsForTrackAsync(trackId)).ToList();
            var tagToRemove = existingTags.FirstOrDefault(t => t.TagId == tagId);
            if (tagToRemove == null)
                return Result.Failure("Tag is not assigned to this track.");

            track.Tags.Remove(tagToRemove);
            await _saveAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error removing tag from track: {ex.Message}");
        }
    }

    public async Task<Result> AddTagToAlbumAsync(int albumId, int tagId)
    {
        try
        {
            var album = await _albumRepository.GetWithTracksAsync(albumId);
            if (album == null)
                return Result.Failure($"Album with ID {albumId} not found.");

            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag == null)
                return Result.Failure($"Tag with ID {tagId} not found.");

            var existingTags = (await _tagRepository.GetTagsForAlbumAsync(albumId)).ToList();
            if (existingTags.Any(t => t.TagId == tagId))
                return Result.Failure("Tag is already assigned to this album.");

            album.Tags.Add(tag);
            await _saveAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error adding tag to album: {ex.Message}");
        }
    }

    public async Task<Result> RemoveTagFromAlbumAsync(int albumId, int tagId)
    {
        try
        {
            var album = await _albumRepository.GetWithTracksAsync(albumId);
            if (album == null)
                return Result.Failure($"Album with ID {albumId} not found.");

            var existingTags = (await _tagRepository.GetTagsForAlbumAsync(albumId)).ToList();
            var tagToRemove = existingTags.FirstOrDefault(t => t.TagId == tagId);
            if (tagToRemove == null)
                return Result.Failure("Tag is not assigned to this album.");

            album.Tags.Remove(tagToRemove);
            await _saveAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error removing tag from album: {ex.Message}");
        }
    }

    public async Task<Result> AddTagToArtistAsync(int artistId, int tagId)
    {
        try
        {
            var artist = await _artistRepository.GetByIdAsync(artistId);
            if (artist == null)
                return Result.Failure($"Artist with ID {artistId} not found.");

            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag == null)
                return Result.Failure($"Tag with ID {tagId} not found.");

            var existingTags = (await _tagRepository.GetTagsForArtistAsync(artistId)).ToList();
            if (existingTags.Any(t => t.TagId == tagId))
                return Result.Failure("Tag is already assigned to this artist.");

            artist.Tags.Add(tag);
            await _saveAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error adding tag to artist: {ex.Message}");
        }
    }

    public async Task<Result> RemoveTagFromArtistAsync(int artistId, int tagId)
    {
        try
        {
            var artist = await _artistRepository.GetByIdAsync(artistId);
            if (artist == null)
                return Result.Failure($"Artist with ID {artistId} not found.");

            var existingTags = (await _tagRepository.GetTagsForArtistAsync(artistId)).ToList();
            var tagToRemove = existingTags.FirstOrDefault(t => t.TagId == tagId);
            if (tagToRemove == null)
                return Result.Failure("Tag is not assigned to this artist.");

            artist.Tags.Remove(tagToRemove);
            await _saveAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error removing tag from artist: {ex.Message}");
        }
    }
}
