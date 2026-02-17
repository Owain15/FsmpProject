using FluentAssertions;
using FSMO;

namespace FSMP.Tests.FSMO;

public class FsmoReferenceTests : IDisposable
{
    private readonly string _tempDir;

    public FsmoReferenceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"fsmo_test_{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void DirectoryManager_CreateDirectory_ShouldCreateDirectory()
    {
        DirectoryManager.CreateDirectory(_tempDir);

        Directory.Exists(_tempDir).Should().BeTrue();
    }
}
