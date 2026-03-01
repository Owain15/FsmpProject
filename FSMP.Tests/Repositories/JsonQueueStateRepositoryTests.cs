using FluentAssertions;
using FSMP.Core;
using FSMP.Core.Models;
using FsmpDataAcsses.Repositories;

namespace FSMP.Tests.Repositories;

public class JsonQueueStateRepositoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _filePath;

    public JsonQueueStateRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "fsmp-test-" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
        _filePath = Path.Combine(_tempDir, "queue-state.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public async Task LoadAsync_MissingFile_ReturnsNull()
    {
        var repo = new JsonQueueStateRepository(_filePath);
        var result = await repo.LoadAsync();
        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveAndLoad_RoundTrip_PreservesState()
    {
        var repo = new JsonQueueStateRepository(_filePath);
        var state = new QueueState
        {
            OriginalOrder = new List<int> { 1, 2, 3 },
            PlayOrder = new List<int> { 3, 1, 2 },
            CurrentIndex = 1,
            RepeatMode = RepeatMode.All,
            IsShuffled = true
        };

        await repo.SaveAsync(state);
        var loaded = await repo.LoadAsync();

        loaded.Should().NotBeNull();
        loaded!.OriginalOrder.Should().Equal(1, 2, 3);
        loaded.PlayOrder.Should().Equal(3, 1, 2);
        loaded.CurrentIndex.Should().Be(1);
        loaded.RepeatMode.Should().Be(RepeatMode.All);
        loaded.IsShuffled.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_CorruptFile_ReturnsNull()
    {
        await File.WriteAllTextAsync(_filePath, "not valid json {{{");
        var repo = new JsonQueueStateRepository(_filePath);

        var result = await repo.LoadAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_CreatesDirectoryIfNeeded()
    {
        var nestedPath = Path.Combine(_tempDir, "sub", "dir", "queue-state.json");
        var repo = new JsonQueueStateRepository(nestedPath);

        await repo.SaveAsync(new QueueState());

        File.Exists(nestedPath).Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_OverwritesExistingFile()
    {
        var repo = new JsonQueueStateRepository(_filePath);

        await repo.SaveAsync(new QueueState { CurrentIndex = 5, PlayOrder = new List<int> { 1 } });
        await repo.SaveAsync(new QueueState { CurrentIndex = 10, PlayOrder = new List<int> { 2 } });

        var loaded = await repo.LoadAsync();
        loaded!.CurrentIndex.Should().Be(10);
        loaded.PlayOrder.Should().Equal(2);
    }
}
