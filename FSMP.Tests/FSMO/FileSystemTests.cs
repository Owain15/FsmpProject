using FluentAssertions;
using FSMO;

namespace FSMP.Tests.FSMO;

public class FileSystemTests : IDisposable
{
    private readonly string _tempDir;

    public FileSystemTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"fsmo_fs_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    #region CreateDirectory

    [Fact]
    public void CreateDirectory_ShouldCreateNewDirectory()
    {
        var path = Path.Combine(_tempDir, "newDir");

        FileSystem.CreateDirectory(path);

        Directory.Exists(path).Should().BeTrue();
    }

    [Fact]
    public void CreateDirectory_WhenExists_ShouldDoNothing()
    {
        var path = Path.Combine(_tempDir, "existingDir");
        Directory.CreateDirectory(path);

        var act = () => FileSystem.CreateDirectory(path);

        act.Should().NotThrow();
        Directory.Exists(path).Should().BeTrue();
    }

    #endregion

    #region CreateFile

    [Fact]
    public void CreateFile_ShouldCreateNewFile()
    {
        var path = Path.Combine(_tempDir, "newFile.txt");

        FileSystem.CreateFile(path);

        File.Exists(path).Should().BeTrue();
    }

    [Fact]
    public void CreateFile_WhenExists_ShouldDoNothing()
    {
        var path = Path.Combine(_tempDir, "existingFile.txt");
        File.WriteAllText(path, "original content");

        FileSystem.CreateFile(path);

        File.Exists(path).Should().BeTrue();
        File.ReadAllText(path).Should().Be("original content");
    }

    #endregion

    #region DeleteFile

    [Fact]
    public void DeleteFile_ShouldRemoveExistingFile()
    {
        var path = Path.Combine(_tempDir, "toDelete.txt");
        File.WriteAllText(path, "delete me");

        FileSystem.DeleteFile(path);

        File.Exists(path).Should().BeFalse();
    }

    [Fact]
    public void DeleteFile_WhenMissing_ShouldDoNothing()
    {
        var path = Path.Combine(_tempDir, "nonexistent.txt");

        var act = () => FileSystem.DeleteFile(path);

        act.Should().NotThrow();
    }

    #endregion

    #region DeleteDirectory

    [Fact]
    public void DeleteDirectory_ShouldRemoveDirectoryRecursively()
    {
        var path = Path.Combine(_tempDir, "toDeleteDir");
        Directory.CreateDirectory(path);
        File.WriteAllText(Path.Combine(path, "child.txt"), "content");

        FileSystem.DeleteDirectory(path);

        Directory.Exists(path).Should().BeFalse();
    }

    [Fact]
    public void DeleteDirectory_WhenMissing_ShouldDoNothing()
    {
        var path = Path.Combine(_tempDir, "nonexistentDir");

        var act = () => FileSystem.DeleteDirectory(path);

        act.Should().NotThrow();
    }

    #endregion

    #region MoveFile

    [Fact]
    public void MoveFile_ShouldMoveToNewLocation()
    {
        var source = Path.Combine(_tempDir, "source.txt");
        var dest = Path.Combine(_tempDir, "dest.txt");
        File.WriteAllText(source, "move me");

        FileSystem.MoveFile(source, dest);

        File.Exists(source).Should().BeFalse();
        File.Exists(dest).Should().BeTrue();
        File.ReadAllText(dest).Should().Be("move me");
    }

    [Fact]
    public void MoveFile_WhenSourceMissing_ShouldDoNothing()
    {
        var source = Path.Combine(_tempDir, "nonexistent.txt");
        var dest = Path.Combine(_tempDir, "dest.txt");

        var act = () => FileSystem.MoveFile(source, dest);

        act.Should().NotThrow();
        File.Exists(dest).Should().BeFalse();
    }

    #endregion

    #region MoveDirectory

    [Fact]
    public void MoveDirectory_ShouldMoveToNewLocation()
    {
        var source = Path.Combine(_tempDir, "sourceDir");
        var dest = Path.Combine(_tempDir, "destDir");
        Directory.CreateDirectory(source);
        File.WriteAllText(Path.Combine(source, "file.txt"), "content");

        FileSystem.MoveDirectory(source, dest);

        Directory.Exists(source).Should().BeFalse();
        Directory.Exists(dest).Should().BeTrue();
        File.Exists(Path.Combine(dest, "file.txt")).Should().BeTrue();
    }

    [Fact]
    public void MoveDirectory_WhenSourceMissing_ShouldDoNothing()
    {
        var source = Path.Combine(_tempDir, "nonexistentDir");
        var dest = Path.Combine(_tempDir, "destDir");

        var act = () => FileSystem.MoveDirectory(source, dest);

        act.Should().NotThrow();
        Directory.Exists(dest).Should().BeFalse();
    }

    #endregion
}
