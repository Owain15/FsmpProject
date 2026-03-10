using FluentAssertions;
using FsmpDataAcsses;
using FsmpDataAcsses.Repositories;
using FSMP.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.Repositories;

public class TagRepositoryTests : IDisposable
{
    private readonly FsmpDbContext _context;
    private readonly TagRepository _repository;

    public TagRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new TagRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldFindTag()
    {
        var result = await _repository.GetByNameAsync("Rock");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Rock");
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repository.GetByNameAsync("Nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSeedTags()
    {
        var result = (await _repository.GetAllAsync()).ToList();

        result.Should().HaveCountGreaterThanOrEqualTo(5);
        result.Select(t => t.Name).Should().Contain("Rock")
            .And.Contain("Jazz")
            .And.Contain("Classic")
            .And.Contain("Metal")
            .And.Contain("Comedy");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldFindTag()
    {
        var result = await _repository.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Rock");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddTag()
    {
        var tag = new Tags { Name = "Pop" };
        await _repository.AddAsync(tag);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByNameAsync("Pop");
        result.Should().NotBeNull();
        result!.Name.Should().Be("Pop");
    }

    [Fact]
    public async Task Remove_ShouldDeleteTag()
    {
        var tag = new Tags { Name = "Temporary" };
        await _repository.AddAsync(tag);
        await _context.SaveChangesAsync();

        _repository.Remove(tag);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByNameAsync("Temporary");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTagsForTrackAsync_ShouldReturnTags()
    {
        var artist = new Artist { Name = "Test Artist" };
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();

        var rockTag = await _repository.GetByNameAsync("Rock");
        var jazzTag = await _repository.GetByNameAsync("Jazz");

        var track = new Track
        {
            Title = "Test Track",
            FilePath = @"C:\test.mp3",
            FileHash = "hash1",
            ArtistId = artist.ArtistId,
            Tags = new List<Tags> { rockTag!, jazzTag! }
        };
        _context.Tracks.Add(track);
        await _context.SaveChangesAsync();

        var result = (await _repository.GetTagsForTrackAsync(track.TrackId)).ToList();

        result.Should().HaveCount(2);
        result.Select(t => t.Name).Should().Contain("Rock").And.Contain("Jazz");
    }

    [Fact]
    public async Task GetTagsForTrackAsync_ShouldReturnEmpty_WhenNoTags()
    {
        var artist = new Artist { Name = "Test Artist 2" };
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();

        var track = new Track
        {
            Title = "Untagged Track",
            FilePath = @"C:\untagged.mp3",
            FileHash = "hash2",
            ArtistId = artist.ArtistId
        };
        _context.Tracks.Add(track);
        await _context.SaveChangesAsync();

        var result = (await _repository.GetTagsForTrackAsync(track.TrackId)).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTagsForTrackAsync_ShouldReturnEmpty_WhenTrackNotFound()
    {
        var result = (await _repository.GetTagsForTrackAsync(999)).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTagsForAlbumAsync_ShouldReturnTags()
    {
        var artist = new Artist { Name = "Album Artist" };
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();

        var metalTag = await _repository.GetByNameAsync("Metal");
        var album = new Album
        {
            Title = "Test Album",
            ArtistId = artist.ArtistId,
            Tags = new List<Tags> { metalTag! }
        };
        _context.Albums.Add(album);
        await _context.SaveChangesAsync();

        var result = (await _repository.GetTagsForAlbumAsync(album.AlbumId)).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Metal");
    }

    [Fact]
    public async Task GetTagsForArtistAsync_ShouldReturnTags()
    {
        var comedyTag = await _repository.GetByNameAsync("Comedy");
        var artist = new Artist
        {
            Name = "Funny Artist",
            Tags = new List<Tags> { comedyTag! }
        };
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();

        var result = (await _repository.GetTagsForArtistAsync(artist.ArtistId)).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Comedy");
    }
}
