using FluentAssertions;
using FsmpDataAcsses;
using FsmpDataAcsses.Repositories;
using FSMP.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.Repositories;

public class UnitOfWorkTests : IDisposable
{
    private readonly DbContextOptions<FsmpDbContext> _options;
    private readonly FsmpDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        _options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(_options);
        _context.Database.EnsureCreated();
        _unitOfWork = new UnitOfWork(_context);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
    }

    [Fact]
    public void Tracks_ShouldBeInitialized()
    {
        _unitOfWork.Tracks.Should().NotBeNull();
        _unitOfWork.Tracks.Should().BeOfType<TrackRepository>();
    }

    [Fact]
    public void Albums_ShouldBeInitialized()
    {
        _unitOfWork.Albums.Should().NotBeNull();
        _unitOfWork.Albums.Should().BeOfType<AlbumRepository>();
    }

    [Fact]
    public void Artists_ShouldBeInitialized()
    {
        _unitOfWork.Artists.Should().NotBeNull();
        _unitOfWork.Artists.Should().BeOfType<ArtistRepository>();
    }

    [Fact]
    public void PlaybackHistories_ShouldBeInitialized()
    {
        _unitOfWork.PlaybackHistories.Should().NotBeNull();
        _unitOfWork.PlaybackHistories.Should().BeOfType<PlaybackHistoryRepository>();
    }

    [Fact]
    public void LibraryPaths_ShouldBeInitialized()
    {
        _unitOfWork.LibraryPaths.Should().NotBeNull();
        _unitOfWork.LibraryPaths.Should().BeOfType<Repository<LibraryPath>>();
    }

    [Fact]
    public void Tags_ShouldBeInitialized()
    {
        _unitOfWork.Tags.Should().NotBeNull();
        _unitOfWork.Tags.Should().BeOfType<Repository<Tags>>();
    }

    [Fact]
    public void FileExtensions_ShouldBeInitialized()
    {
        _unitOfWork.FileExtensions.Should().NotBeNull();
        _unitOfWork.FileExtensions.Should().BeOfType<Repository<FileExtension>>();
    }

    [Fact]
    public void RepositoryProperties_ShouldReturnSameInstance()
    {
        var tracks1 = _unitOfWork.Tracks;
        var tracks2 = _unitOfWork.Tracks;

        tracks1.Should().BeSameAs(tracks2);
    }

    [Fact]
    public async Task SaveAsync_ShouldCommitChanges()
    {
        var artist = new Artist { Name = "Saved Artist" };
        await _unitOfWork.Artists.AddAsync(artist);

        var result = await _unitOfWork.SaveAsync();

        result.Should().BeGreaterThan(0);

        var retrieved = await _unitOfWork.Artists.GetByIdAsync(artist.ArtistId);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Saved Artist");
    }

    [Fact]
    public async Task SaveAsync_ShouldReturnZero_WhenNoChanges()
    {
        var result = await _unitOfWork.SaveAsync();

        result.Should().Be(0);
    }

    [Fact]
    public void Dispose_ShouldReleaseContext()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new FsmpDbContext(options);
        var uow = new UnitOfWork(context);

        uow.Dispose();

        // Accessing the context after dispose should throw
        var act = () => context.Artists.ToList();
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void Dispose_ShouldBeSafeToCallMultipleTimes()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new FsmpDbContext(options);
        var uow = new UnitOfWork(context);

        var act = () =>
        {
            uow.Dispose();
            uow.Dispose();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public async Task MultipleRepositories_ShouldShareContext()
    {
        // Add an artist, album, and track through different repositories
        var artist = new Artist { Name = "Shared Context Artist" };
        await _unitOfWork.Artists.AddAsync(artist);
        await _unitOfWork.SaveAsync();

        var album = new Album { Title = "Shared Album", ArtistId = artist.ArtistId };
        await _unitOfWork.Albums.AddAsync(album);
        await _unitOfWork.SaveAsync();

        var track = new Track
        {
            Title = "Shared Track",
            FilePath = @"C:\shared.mp3",
            FileHash = "shash",
            ArtistId = artist.ArtistId,
            AlbumId = album.AlbumId
        };
        await _unitOfWork.Tracks.AddAsync(track);
        await _unitOfWork.SaveAsync();

        // All should be retrievable
        (await _unitOfWork.Artists.CountAsync()).Should().Be(1);
        (await _unitOfWork.Albums.CountAsync()).Should().Be(1);
        (await _unitOfWork.Tracks.CountAsync()).Should().Be(1);
    }
}
