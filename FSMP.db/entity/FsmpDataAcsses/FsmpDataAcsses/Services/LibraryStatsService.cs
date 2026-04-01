using FSMP.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FsmpDataAcsses.Services;

public class LibraryStatsService : ILibraryStatsService
{
    private readonly FsmpDbContext _context;

    public LibraryStatsService(FsmpDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<DirectoryStats> GetDirectoryStatsAsync(string directoryPath)
    {
        ArgumentNullException.ThrowIfNull(directoryPath);

        // Normalize path separator and ensure trailing separator for prefix matching
        var normalizedPath = directoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        var tracks = await _context.Tracks
            .Where(t => t.FilePath.StartsWith(normalizedPath))
            .Select(t => new { t.AlbumId, t.ArtistId })
            .ToListAsync();

        var trackCount = tracks.Count;
        var albumCount = tracks.Where(t => t.AlbumId != null).Select(t => t.AlbumId).Distinct().Count();
        var artistCount = tracks.Where(t => t.ArtistId != null).Select(t => t.ArtistId).Distinct().Count();

        return new DirectoryStats(trackCount, albumCount, artistCount);
    }

    public async Task<DirectoryStats> GetTotalStatsAsync()
    {
        var trackCount = await _context.Tracks.CountAsync();
        var albumCount = await _context.Albums.CountAsync();
        var artistCount = await _context.Artists.CountAsync();
        return new DirectoryStats(trackCount, albumCount, artistCount);
    }
}
