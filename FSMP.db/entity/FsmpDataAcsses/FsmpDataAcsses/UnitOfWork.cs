using FsmpDataAcsses.Repositories;
using FsmpLibrary.Models;

namespace FsmpDataAcsses;

/// <summary>
/// Coordinates multiple repositories and manages database transactions.
/// </summary>
public class UnitOfWork : IDisposable
{
    private readonly FsmpDbContext _context;
    private bool _disposed;

    private TrackRepository? _tracks;
    private AlbumRepository? _albums;
    private ArtistRepository? _artists;
    private PlaybackHistoryRepository? _playbackHistories;
    private Repository<LibraryPath>? _libraryPaths;
    private Repository<Genre>? _genres;
    private Repository<FileExtension>? _fileExtensions;

    public UnitOfWork(FsmpDbContext context)
    {
        _context = context;
    }

    public TrackRepository Tracks =>
        _tracks ??= new TrackRepository(_context);

    public AlbumRepository Albums =>
        _albums ??= new AlbumRepository(_context);

    public ArtistRepository Artists =>
        _artists ??= new ArtistRepository(_context);

    public PlaybackHistoryRepository PlaybackHistories =>
        _playbackHistories ??= new PlaybackHistoryRepository(_context);

    public Repository<LibraryPath> LibraryPaths =>
        _libraryPaths ??= new Repository<LibraryPath>(_context);

    public Repository<Genre> Genres =>
        _genres ??= new Repository<Genre>(_context);

    public Repository<FileExtension> FileExtensions =>
        _fileExtensions ??= new Repository<FileExtension>(_context);

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
