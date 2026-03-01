using FSMP.Core.Interfaces;
using FSMP.Core.Models;
using FsmpLibrary.Services;
using Moq;
using FluentAssertions;

namespace FSMP.Tests.Services;

public class LibraryManagerTests
{
    private readonly Mock<IConfigurationService> _configServiceMock;
    private readonly Mock<ILibraryScanService> _scanServiceMock;
    private readonly LibraryManager _manager;

    public LibraryManagerTests()
    {
        _configServiceMock = new Mock<IConfigurationService>();
        _scanServiceMock = new Mock<ILibraryScanService>();
        _manager = new LibraryManager(_configServiceMock.Object, _scanServiceMock.Object);
    }

    [Fact]
    public void Constructor_ThrowsOnNullConfigService()
    {
        var act = () => new LibraryManager(null!, _scanServiceMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullScanService()
    {
        var act = () => new LibraryManager(_configServiceMock.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task LoadConfigurationAsync_ReturnsConfig()
    {
        var config = new Configuration { LibraryPaths = new List<string> { "C:\\Music" } };
        _configServiceMock.Setup(s => s.LoadConfigurationAsync()).ReturnsAsync(config);

        var result = await _manager.LoadConfigurationAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value!.LibraryPaths.Should().HaveCount(1);
    }

    [Fact]
    public async Task LoadConfigurationAsync_ReturnsFailure_OnException()
    {
        _configServiceMock.Setup(s => s.LoadConfigurationAsync()).ThrowsAsync(new Exception("fail"));

        var result = await _manager.LoadConfigurationAsync();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AddLibraryPathAsync_ReturnsFailure_WhenDirectoryNotFound()
    {
        var result = await _manager.AddLibraryPathAsync("C:\\NonExistent\\Path\\12345");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task AddLibraryPathAsync_ReturnsSuccess_WhenDirectoryExists()
    {
        var tempDir = Path.GetTempPath();
        var result = await _manager.AddLibraryPathAsync(tempDir);

        result.IsSuccess.Should().BeTrue();
        _configServiceMock.Verify(s => s.AddLibraryPathAsync(tempDir), Times.Once);
    }

    [Fact]
    public async Task RemoveLibraryPathAsync_ReturnsSuccess()
    {
        var result = await _manager.RemoveLibraryPathAsync("C:\\Music");

        result.IsSuccess.Should().BeTrue();
        _configServiceMock.Verify(s => s.RemoveLibraryPathAsync("C:\\Music"), Times.Once);
    }

    [Fact]
    public async Task RemoveLibraryPathAsync_ReturnsFailure_OnException()
    {
        _configServiceMock.Setup(s => s.RemoveLibraryPathAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("fail"));

        var result = await _manager.RemoveLibraryPathAsync("C:\\Music");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ScanAllLibrariesAsync_ReturnsFailure_WhenNoPaths()
    {
        var config = new Configuration { LibraryPaths = new List<string>() };
        _configServiceMock.Setup(s => s.LoadConfigurationAsync()).ReturnsAsync(config);

        var result = await _manager.ScanAllLibrariesAsync();

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("No library paths");
    }

    [Fact]
    public async Task ScanAllLibrariesAsync_ReturnsScanResult()
    {
        var config = new Configuration { LibraryPaths = new List<string> { "C:\\Music" } };
        _configServiceMock.Setup(s => s.LoadConfigurationAsync()).ReturnsAsync(config);
        var scanResult = new ScanResult { TracksAdded = 5 };
        _scanServiceMock.Setup(s => s.ScanAllLibrariesAsync(config.LibraryPaths)).ReturnsAsync(scanResult);

        var result = await _manager.ScanAllLibrariesAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value!.TracksAdded.Should().Be(5);
    }

    [Fact]
    public async Task ScanAllLibrariesAsync_ReturnsFailure_OnException()
    {
        var config = new Configuration { LibraryPaths = new List<string> { "C:\\Music" } };
        _configServiceMock.Setup(s => s.LoadConfigurationAsync()).ReturnsAsync(config);
        _scanServiceMock.Setup(s => s.ScanAllLibrariesAsync(It.IsAny<List<string>>()))
            .ThrowsAsync(new Exception("scan fail"));

        var result = await _manager.ScanAllLibrariesAsync();

        result.IsSuccess.Should().BeFalse();
    }
}
