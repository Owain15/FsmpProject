using FSMP.Core.Interfaces;
using FSMP.Core.Models;

namespace FSMP.Core.Services;

public class LibraryBrowser : ILibraryBrowser
{
    private readonly IArtistRepository _artistRepository;
    private readonly IAlbumRepository _albumRepository;
    private readonly ITrackRepository _trackRepository;
    private readonly ITagRepository? _tagRepository;

    public LibraryBrowser(
        IArtistRepository artistRepository,
        IAlbumRepository albumRepository,
        ITrackRepository trackRepository,
        ITagRepository? tagRepository = null)
    {
        _artistRepository = artistRepository ?? throw new ArgumentNullException(nameof(artistRepository));
        _albumRepository = albumRepository ?? throw new ArgumentNullException(nameof(albumRepository));
        _trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
        _tagRepository = tagRepository;
    }

    public async Task<Result<List<Artist>>> GetAllArtistsAsync()
    {
        try
        {
            var artists = (await _artistRepository.GetAllAsync()).ToList();
            return Result.Success(artists);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Artist>>($"Error loading artists: {ex.Message}");
        }
    }

    public async Task<Result<Artist?>> GetArtistByIdAsync(int artistId)
    {
        try
        {
            var artist = await _artistRepository.GetByIdAsync(artistId);
            return Result.Success<Artist?>(artist);
        }
        catch (Exception ex)
        {
            return Result.Failure<Artist?>($"Error loading artist: {ex.Message}");
        }
    }

    public async Task<Result<List<Album>>> GetAlbumsByArtistAsync(int artistId)
    {
        try
        {
            var albums = (await _albumRepository.GetByArtistAsync(artistId)).ToList();
            return Result.Success(albums);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Album>>($"Error loading albums: {ex.Message}");
        }
    }

    public async Task<Result<Album?>> GetAlbumWithTracksAsync(int albumId)
    {
        try
        {
            var album = await _albumRepository.GetWithTracksAsync(albumId);
            return Result.Success<Album?>(album);
        }
        catch (Exception ex)
        {
            return Result.Failure<Album?>($"Error loading album: {ex.Message}");
        }
    }

    public async Task<Result<Track?>> GetTrackByIdAsync(int trackId)
    {
        try
        {
            var track = await _trackRepository.GetByIdAsync(trackId);
            return Result.Success<Track?>(track);
        }
        catch (Exception ex)
        {
            return Result.Failure<Track?>($"Error loading track: {ex.Message}");
        }
    }

    public async Task<Result<List<int>>> GetAllTrackIdsByArtistAsync(int artistId)
    {
        try
        {
            var artist = await _artistRepository.GetWithTracksAsync(artistId);
            var trackIds = artist?.Tracks?.Select(t => t.TrackId).ToList() ?? new List<int>();
            return Result.Success(trackIds);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<int>>($"Error loading tracks: {ex.Message}");
        }
    }

    public async Task<Result<List<int>>> GetAllTrackIdsAsync()
    {
        try
        {
            var artists = (await _artistRepository.GetAllAsync()).ToList();
            var allTrackIds = new List<int>();
            foreach (var artist in artists)
            {
                var a = await _artistRepository.GetWithTracksAsync(artist.ArtistId);
                if (a?.Tracks != null)
                    allTrackIds.AddRange(a.Tracks.Select(t => t.TrackId));
            }
            return Result.Success(allTrackIds);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<int>>($"Error loading tracks: {ex.Message}");
        }
    }

    public async Task<Result<List<Tags>>> GetAllTagsAsync()
    {
        try
        {
            if (_tagRepository == null)
                return Result.Failure<List<Tags>>("Tag repository not available.");
            var tags = (await _tagRepository.GetAllAsync()).ToList();
            return Result.Success(tags);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Tags>>($"Error loading tags: {ex.Message}");
        }
    }

    public async Task<Result<List<Artist>>> GetArtistsByTagAsync(int tagId)
    {
        try
        {
            var artists = (await _artistRepository.GetByTagAsync(tagId)).ToList();
            return Result.Success(artists);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Artist>>($"Error loading artists by tag: {ex.Message}");
        }
    }

    public async Task<Result<List<Album>>> GetAlbumsByTagAsync(int tagId)
    {
        try
        {
            var albums = (await _albumRepository.GetByTagAsync(tagId)).ToList();
            return Result.Success(albums);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Album>>($"Error loading albums by tag: {ex.Message}");
        }
    }

    public async Task<Result<List<Track>>> GetTracksByTagAsync(int tagId)
    {
        try
        {
            var tracks = (await _trackRepository.GetByTagAsync(tagId)).ToList();
            return Result.Success(tracks);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Track>>($"Error loading tracks by tag: {ex.Message}");
        }
    }
}
