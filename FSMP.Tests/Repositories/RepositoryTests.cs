using FluentAssertions;
using FsmpDataAcsses;
using FsmpDataAcsses.Repositories;
using FSMP.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.Repositories;

public class RepositoryTests : IDisposable
{
    private readonly FsmpDbContext _context;
    private readonly Repository<Artist> _repository;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new Repository<Artist>(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var artist = new Artist { Name = "Test Artist" };
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(artist.ArtistId);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Artist");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        _context.Artists.Add(new Artist { Name = "Artist 1" });
        _context.Artists.Add(new Artist { Name = "Artist 2" });
        _context.Artists.Add(new Artist { Name = "Artist 3" });
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoEntities()
    {
        var result = await _repository.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindAsync_ShouldReturnMatchingEntities()
    {
        _context.Artists.Add(new Artist { Name = "Rock Band" });
        _context.Artists.Add(new Artist { Name = "Jazz Trio" });
        _context.Artists.Add(new Artist { Name = "Rock Duo" });
        await _context.SaveChangesAsync();

        var result = await _repository.FindAsync(a => a.Name.Contains("Rock"));

        result.Should().HaveCount(2);
        result.All(a => a.Name.Contains("Rock")).Should().BeTrue();
    }

    [Fact]
    public async Task FindAsync_ShouldReturnEmpty_WhenNoMatch()
    {
        _context.Artists.Add(new Artist { Name = "Rock Band" });
        await _context.SaveChangesAsync();

        var result = await _repository.FindAsync(a => a.Name.Contains("Classical"));

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        var artist = new Artist { Name = "New Artist" };

        await _repository.AddAsync(artist);
        await _context.SaveChangesAsync();

        var result = await _context.Artists.FirstAsync(a => a.Name == "New Artist");
        result.ArtistId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        var artists = new List<Artist>
        {
            new Artist { Name = "Artist A" },
            new Artist { Name = "Artist B" },
            new Artist { Name = "Artist C" }
        };

        await _repository.AddRangeAsync(artists);
        await _context.SaveChangesAsync();

        var count = await _context.Artists.CountAsync();
        count.Should().Be(3);
    }

    [Fact]
    public async Task Update_ShouldModifyEntity()
    {
        var artist = new Artist { Name = "Original Name" };
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();

        artist.Name = "Updated Name";
        _repository.Update(artist);
        await _context.SaveChangesAsync();

        var result = await _context.Artists.FindAsync(artist.ArtistId);
        result!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Remove_ShouldDeleteEntity()
    {
        var artist = new Artist { Name = "To Remove" };
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();

        _repository.Remove(artist);
        await _context.SaveChangesAsync();

        var result = await _context.Artists.FindAsync(artist.ArtistId);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        _context.Artists.Add(new Artist { Name = "Artist 1" });
        _context.Artists.Add(new Artist { Name = "Artist 2" });
        await _context.SaveChangesAsync();

        var count = await _repository.CountAsync();

        count.Should().Be(2);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnZero_WhenEmpty()
    {
        var count = await _repository.CountAsync();

        count.Should().Be(0);
    }
}
