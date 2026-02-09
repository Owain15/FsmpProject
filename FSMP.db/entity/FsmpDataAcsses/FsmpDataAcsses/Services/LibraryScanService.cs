using System.Security.Cryptography;
using FsmpLibrary.Models;
using FsmpLibrary.Services;

namespace FsmpDataAcsses.Services;

/// <summary>
/// Scans library directories for audio files and imports them into the database.
/// Creates or reuses Artist and Album entities based on file metadata.
/// </summary>
public class LibraryScanService
{
    private static readonly HashSet<string> SupportedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".wav", ".wma", ".mp3" };

    private readonly UnitOfWork _unitOfWork;
    private readonly IMetadataService _metadataService;

    // Tracks hashes seen during the current scan to detect duplicates before SaveAsync
    private readonly HashSet<string> _seenHashes = new();

    public LibraryScanService(UnitOfWork unitOfWork, IMetadataService metadataService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
    }

    public async Task<ScanResult> ScanAllLibrariesAsync(List<string> libraryPaths)
    {
        ArgumentNullException.ThrowIfNull(libraryPaths);

        var aggregate = new ScanResult();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        foreach (var path in libraryPaths)
        {
            try
            {
                var result = await ScanLibraryAsync(path);
                aggregate.TracksAdded += result.TracksAdded;
                aggregate.TracksUpdated += result.TracksUpdated;
                aggregate.TracksRemoved += result.TracksRemoved;
                aggregate.Errors.AddRange(result.Errors);
            }
            catch (Exception ex)
            {
                aggregate.Errors.Add($"{path}: {ex.Message}");
            }
        }

        sw.Stop();
        aggregate.Duration = sw.Elapsed;
        return aggregate;
    }

    public async Task<ScanResult> ScanLibraryAsync(string libraryPath)
    {
        ArgumentNullException.ThrowIfNull(libraryPath);
        if (!Directory.Exists(libraryPath))
            throw new DirectoryNotFoundException($"Library path not found: {libraryPath}");

        var result = new ScanResult();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var files = Directory.EnumerateFiles(libraryPath, "*.*", SearchOption.AllDirectories)
            .Where(f => IsSupportedFormat(Path.GetExtension(f)));

        foreach (var filePath in files)
        {
            try
            {
                var track = await ImportTrackAsync(new FileInfo(filePath));
                if (track != null)
                    result.TracksAdded++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"{filePath}: {ex.Message}");
            }
        }

        await _unitOfWork.SaveAsync();
        sw.Stop();
        result.Duration = sw.Elapsed;
        return result;
    }

    public async Task<Track?> ImportTrackAsync(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

        var hash = CalculateFileHash(fileInfo.FullName);

        // Check local cache first (catches duplicates within the same scan batch)
        if (!_seenHashes.Add(hash))
            return null;

        // Check database for previously imported duplicates
        var existing = await _unitOfWork.Tracks.GetByFileHashAsync(hash);
        if (existing != null)
            return null;

        // Also check by file path
        existing = await _unitOfWork.Tracks.GetByFilePathAsync(fileInfo.FullName);
        if (existing != null)
            return null;

        var metadata = _metadataService.ReadMetadata(fileInfo.FullName);

        var track = new Track
        {
            Title = metadata.Title ?? Path.GetFileNameWithoutExtension(fileInfo.Name),
            FilePath = fileInfo.FullName,
            FileSizeBytes = fileInfo.Length,
            FileHash = hash,
            Duration = metadata.Duration,
            BitRate = metadata.BitRate,
            SampleRate = metadata.SampleRate,
            TrackNumber = metadata.TrackNumber,
            DiscNumber = metadata.DiscNumber,
            ImportedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        // Resolve file extension
        var ext = fileInfo.Extension.TrimStart('.').ToLowerInvariant();
        var extensions = await _unitOfWork.FileExtensions.FindAsync(fe => fe.Extension == ext);
        var fileExtension = extensions.FirstOrDefault();
        if (fileExtension != null)
            track.FileExtensionId = fileExtension.FileExtensionId;

        // Resolve or create artist
        if (!string.IsNullOrWhiteSpace(metadata.Artist))
        {
            var artist = await FindOrCreateArtistAsync(metadata.Artist);
            track.ArtistId = artist.ArtistId;
        }

        // Resolve or create album
        if (!string.IsNullOrWhiteSpace(metadata.Album))
        {
            var album = await FindOrCreateAlbumAsync(metadata.Album, track.ArtistId, metadata.Year);
            track.AlbumId = album.AlbumId;
        }

        await _unitOfWork.Tracks.AddAsync(track);
        return track;
    }

    public static string CalculateFileHash(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        using var stream = File.OpenRead(filePath);
        var hashBytes = SHA256.HashData(stream);
        return Convert.ToHexStringLower(hashBytes);
    }

    public static bool IsSupportedFormat(string extension)
    {
        return SupportedExtensions.Contains(extension);
    }

    private async Task<Artist> FindOrCreateArtistAsync(string artistName)
    {
        var matches = await _unitOfWork.Artists.FindAsync(a => a.Name == artistName);
        var artist = matches.FirstOrDefault();
        if (artist != null)
            return artist;

        artist = new Artist
        {
            Name = artistName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _unitOfWork.Artists.AddAsync(artist);
        await _unitOfWork.SaveAsync();
        return artist;
    }

    private async Task<Album> FindOrCreateAlbumAsync(string albumTitle, int? artistId, int? year)
    {
        var matches = await _unitOfWork.Albums.FindAsync(
            a => a.Title == albumTitle && a.ArtistId == artistId);
        var album = matches.FirstOrDefault();
        if (album != null)
            return album;

        album = new Album
        {
            Title = albumTitle,
            ArtistId = artistId,
            Year = year,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _unitOfWork.Albums.AddAsync(album);
        await _unitOfWork.SaveAsync();
        return album;
    }
}
