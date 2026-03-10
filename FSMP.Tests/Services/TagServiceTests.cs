using FSMP.Core.Interfaces;
using FSMP.Core.Models;
using FSMP.Core.Services;
using Moq;
using FluentAssertions;

namespace FSMP.Tests.Services;

public class TagServiceTests
{
    private readonly Mock<ITagRepository> _tagRepoMock;
    private readonly Mock<ITrackRepository> _trackRepoMock;
    private readonly Mock<IAlbumRepository> _albumRepoMock;
    private readonly Mock<IArtistRepository> _artistRepoMock;
    private readonly Mock<Func<Task<int>>> _saveMock;
    private readonly TagService _service;

    public TagServiceTests()
    {
        _tagRepoMock = new Mock<ITagRepository>();
        _trackRepoMock = new Mock<ITrackRepository>();
        _albumRepoMock = new Mock<IAlbumRepository>();
        _artistRepoMock = new Mock<IArtistRepository>();
        _saveMock = new Mock<Func<Task<int>>>();
        _saveMock.Setup(s => s()).ReturnsAsync(1);

        _service = new TagService(
            _tagRepoMock.Object,
            _trackRepoMock.Object,
            _albumRepoMock.Object,
            _artistRepoMock.Object,
            _saveMock.Object);
    }

    [Fact]
    public void Constructor_ThrowsOnNullTagRepo()
    {
        var act = () => new TagService(null!, _trackRepoMock.Object, _albumRepoMock.Object, _artistRepoMock.Object, _saveMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullTrackRepo()
    {
        var act = () => new TagService(_tagRepoMock.Object, null!, _albumRepoMock.Object, _artistRepoMock.Object, _saveMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullAlbumRepo()
    {
        var act = () => new TagService(_tagRepoMock.Object, _trackRepoMock.Object, null!, _artistRepoMock.Object, _saveMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullArtistRepo()
    {
        var act = () => new TagService(_tagRepoMock.Object, _trackRepoMock.Object, _albumRepoMock.Object, null!, _saveMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullSave()
    {
        var act = () => new TagService(_tagRepoMock.Object, _trackRepoMock.Object, _albumRepoMock.Object, _artistRepoMock.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // GetAllTagsAsync
    [Fact]
    public async Task GetAllTagsAsync_ReturnsAllTags()
    {
        var tags = new List<Tags> { new() { TagId = 1, Name = "Rock" } };
        _tagRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(tags);

        var result = await _service.GetAllTagsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllTagsAsync_ReturnsFailure_OnException()
    {
        _tagRepoMock.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("db error"));

        var result = await _service.GetAllTagsAsync();

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("db error");
    }

    // CreateTagAsync
    [Fact]
    public async Task CreateTagAsync_CreatesTag()
    {
        _tagRepoMock.Setup(r => r.GetByNameAsync("Pop")).ReturnsAsync((Tags?)null);

        var result = await _service.CreateTagAsync("Pop");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Pop");
        _tagRepoMock.Verify(r => r.AddAsync(It.IsAny<Tags>()), Times.Once);
        _saveMock.Verify(s => s(), Times.Once);
    }

    [Fact]
    public async Task CreateTagAsync_FailsOnDuplicate()
    {
        _tagRepoMock.Setup(r => r.GetByNameAsync("Rock")).ReturnsAsync(new Tags { TagId = 1, Name = "Rock" });

        var result = await _service.CreateTagAsync("Rock");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already exists");
    }

    // DeleteTagAsync
    [Fact]
    public async Task DeleteTagAsync_DeletesTag()
    {
        var tag = new Tags { TagId = 1, Name = "Rock" };
        _tagRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tag);

        var result = await _service.DeleteTagAsync(1);

        result.IsSuccess.Should().BeTrue();
        _tagRepoMock.Verify(r => r.Remove(tag), Times.Once);
        _saveMock.Verify(s => s(), Times.Once);
    }

    [Fact]
    public async Task DeleteTagAsync_FailsWhenNotFound()
    {
        _tagRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Tags?)null);

        var result = await _service.DeleteTagAsync(999);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    // GetTagsForTrackAsync
    [Fact]
    public async Task GetTagsForTrackAsync_ReturnsTags()
    {
        var tags = new List<Tags> { new() { TagId = 1, Name = "Rock" } };
        _tagRepoMock.Setup(r => r.GetTagsForTrackAsync(1)).ReturnsAsync(tags);

        var result = await _service.GetTagsForTrackAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    // GetTagsForAlbumAsync
    [Fact]
    public async Task GetTagsForAlbumAsync_ReturnsTags()
    {
        var tags = new List<Tags> { new() { TagId = 2, Name = "Jazz" } };
        _tagRepoMock.Setup(r => r.GetTagsForAlbumAsync(1)).ReturnsAsync(tags);

        var result = await _service.GetTagsForAlbumAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    // GetTagsForArtistAsync
    [Fact]
    public async Task GetTagsForArtistAsync_ReturnsTags()
    {
        var tags = new List<Tags> { new() { TagId = 3, Name = "Classic" } };
        _tagRepoMock.Setup(r => r.GetTagsForArtistAsync(1)).ReturnsAsync(tags);

        var result = await _service.GetTagsForArtistAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    // AddTagToTrackAsync
    [Fact]
    public async Task AddTagToTrackAsync_Success()
    {
        var track = new Track { TrackId = 1, Title = "T", FilePath = "f", FileHash = "h", Tags = new List<Tags>() };
        var tag = new Tags { TagId = 1, Name = "Rock" };
        _trackRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(track);
        _tagRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tag);
        _tagRepoMock.Setup(r => r.GetTagsForTrackAsync(1)).ReturnsAsync(new List<Tags>());

        var result = await _service.AddTagToTrackAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
        track.Tags.Should().Contain(tag);
        _saveMock.Verify(s => s(), Times.Once);
    }

    [Fact]
    public async Task AddTagToTrackAsync_FailsWhenTrackNotFound()
    {
        _trackRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Track?)null);

        var result = await _service.AddTagToTrackAsync(999, 1);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Track");
    }

    [Fact]
    public async Task AddTagToTrackAsync_FailsWhenTagNotFound()
    {
        var track = new Track { TrackId = 1, Title = "T", FilePath = "f", FileHash = "h" };
        _trackRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(track);
        _tagRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Tags?)null);

        var result = await _service.AddTagToTrackAsync(1, 999);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Tag");
    }

    // RemoveTagFromTrackAsync
    [Fact]
    public async Task RemoveTagFromTrackAsync_Success()
    {
        var tag = new Tags { TagId = 1, Name = "Rock" };
        var track = new Track { TrackId = 1, Title = "T", FilePath = "f", FileHash = "h", Tags = new List<Tags> { tag } };
        _trackRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(track);
        _tagRepoMock.Setup(r => r.GetTagsForTrackAsync(1)).ReturnsAsync(new List<Tags> { tag });

        var result = await _service.RemoveTagFromTrackAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
        _saveMock.Verify(s => s(), Times.Once);
    }

    // AddTagToAlbumAsync
    [Fact]
    public async Task AddTagToAlbumAsync_Success()
    {
        var album = new Album { AlbumId = 1, Title = "A", Tags = new List<Tags>() };
        var tag = new Tags { TagId = 1, Name = "Rock" };
        _albumRepoMock.Setup(r => r.GetWithTracksAsync(1)).ReturnsAsync(album);
        _tagRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tag);
        _tagRepoMock.Setup(r => r.GetTagsForAlbumAsync(1)).ReturnsAsync(new List<Tags>());

        var result = await _service.AddTagToAlbumAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
        album.Tags.Should().Contain(tag);
    }

    [Fact]
    public async Task AddTagToAlbumAsync_FailsWhenAlbumNotFound()
    {
        _albumRepoMock.Setup(r => r.GetWithTracksAsync(999)).ReturnsAsync((Album?)null);

        var result = await _service.AddTagToAlbumAsync(999, 1);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Album");
    }

    // RemoveTagFromAlbumAsync
    [Fact]
    public async Task RemoveTagFromAlbumAsync_Success()
    {
        var tag = new Tags { TagId = 1, Name = "Rock" };
        var album = new Album { AlbumId = 1, Title = "A", Tags = new List<Tags> { tag } };
        _albumRepoMock.Setup(r => r.GetWithTracksAsync(1)).ReturnsAsync(album);
        _tagRepoMock.Setup(r => r.GetTagsForAlbumAsync(1)).ReturnsAsync(new List<Tags> { tag });

        var result = await _service.RemoveTagFromAlbumAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
    }

    // AddTagToArtistAsync
    [Fact]
    public async Task AddTagToArtistAsync_Success()
    {
        var artist = new Artist { ArtistId = 1, Name = "A", Tags = new List<Tags>() };
        var tag = new Tags { TagId = 1, Name = "Rock" };
        _artistRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(artist);
        _tagRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tag);
        _tagRepoMock.Setup(r => r.GetTagsForArtistAsync(1)).ReturnsAsync(new List<Tags>());

        var result = await _service.AddTagToArtistAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
        artist.Tags.Should().Contain(tag);
    }

    [Fact]
    public async Task AddTagToArtistAsync_FailsWhenArtistNotFound()
    {
        _artistRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Artist?)null);

        var result = await _service.AddTagToArtistAsync(999, 1);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Artist");
    }

    // RemoveTagFromArtistAsync
    [Fact]
    public async Task RemoveTagFromArtistAsync_Success()
    {
        var tag = new Tags { TagId = 1, Name = "Rock" };
        var artist = new Artist { ArtistId = 1, Name = "A", Tags = new List<Tags> { tag } };
        _artistRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(artist);
        _tagRepoMock.Setup(r => r.GetTagsForArtistAsync(1)).ReturnsAsync(new List<Tags> { tag });

        var result = await _service.RemoveTagFromArtistAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveTagFromArtistAsync_FailsWhenNotAssigned()
    {
        var artist = new Artist { ArtistId = 1, Name = "A", Tags = new List<Tags>() };
        _artistRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(artist);
        _tagRepoMock.Setup(r => r.GetTagsForArtistAsync(1)).ReturnsAsync(new List<Tags>());

        var result = await _service.RemoveTagFromArtistAsync(1, 1);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not assigned");
    }
}
