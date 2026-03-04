using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;
using FSMP.Core.ViewModels;
using FluentAssertions;
using Moq;

namespace FSMP.Tests.ViewModels;

public class SettingsViewModelTests
{
    private readonly Mock<ILibraryManager> _libraryManagerMock;
    private readonly Mock<IConfigurationService> _configServiceMock;
    private readonly SettingsViewModel _vm;

    public SettingsViewModelTests()
    {
        _libraryManagerMock = new Mock<ILibraryManager>();
        _configServiceMock = new Mock<IConfigurationService>();
        _vm = new SettingsViewModel(_libraryManagerMock.Object, _configServiceMock.Object, action => action());
    }

    [Fact]
    public async Task LoadAsync_PopulatesPathsAndSettings()
    {
        var config = new Configuration
        {
            LibraryPaths = new List<string> { @"C:\Music", @"D:\Music" },
            AutoScanOnStartup = false,
            DefaultVolume = 50
        };
        _libraryManagerMock.Setup(m => m.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(config));

        await _vm.LoadAsync();

        _vm.LibraryPaths.Should().BeEquivalentTo(new[] { @"C:\Music", @"D:\Music" });
        _vm.AutoScanOnStartup.Should().BeFalse();
        _vm.DefaultVolume.Should().Be(50);
    }

    [Fact]
    public async Task AddPathCommand_AddsPathAndRefreshes()
    {
        var config = new Configuration
        {
            LibraryPaths = new List<string> { @"C:\Music", @"D:\New" },
            AutoScanOnStartup = true,
            DefaultVolume = 75
        };
        _libraryManagerMock.Setup(m => m.AddLibraryPathAsync(@"D:\New"))
            .ReturnsAsync(Result.Success());
        _libraryManagerMock.Setup(m => m.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(config));

        _vm.AddPathCommand.Execute(@"D:\New");
        await Task.Delay(50);

        _libraryManagerMock.Verify(m => m.AddLibraryPathAsync(@"D:\New"), Times.Once);
        _vm.LibraryPaths.Should().Contain(@"D:\New");
    }

    [Fact]
    public async Task AddPathCommand_IgnoresEmptyPath()
    {
        _vm.AddPathCommand.Execute("");
        await Task.Delay(50);

        _libraryManagerMock.Verify(m => m.AddLibraryPathAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RemovePathCommand_RemovesPathAndRefreshes()
    {
        var config = new Configuration
        {
            LibraryPaths = new List<string> { @"D:\Music" },
            AutoScanOnStartup = true,
            DefaultVolume = 75
        };
        _libraryManagerMock.Setup(m => m.RemoveLibraryPathAsync(@"C:\Music"))
            .ReturnsAsync(Result.Success());
        _libraryManagerMock.Setup(m => m.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(config));

        _vm.RemovePathCommand.Execute(@"C:\Music");
        await Task.Delay(50);

        _libraryManagerMock.Verify(m => m.RemoveLibraryPathAsync(@"C:\Music"), Times.Once);
        _vm.LibraryPaths.Should().NotContain(@"C:\Music");
    }

    [Fact]
    public async Task ScanCommand_CallsScanAndShowsResult()
    {
        var scanResult = new ScanResult
        {
            TracksAdded = 10,
            TracksUpdated = 3,
            TracksRemoved = 1
        };
        _libraryManagerMock.Setup(m => m.ScanAllLibrariesAsync())
            .ReturnsAsync(Result.Success(scanResult));

        _vm.ScanCommand.Execute(null);
        await Task.Delay(50);

        _libraryManagerMock.Verify(m => m.ScanAllLibrariesAsync(), Times.Once);
        _vm.StatusMessage.Should().Contain("10 added");
        _vm.StatusMessage.Should().Contain("3 updated");
        _vm.StatusMessage.Should().Contain("1 removed");
        _vm.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task ScanCommand_ShowsErrorOnFailure()
    {
        _libraryManagerMock.Setup(m => m.ScanAllLibrariesAsync())
            .ReturnsAsync(Result.Failure<ScanResult>("No paths configured"));

        _vm.ScanCommand.Execute(null);
        await Task.Delay(50);

        _vm.StatusMessage.Should().Contain("failed");
        _vm.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task SaveCommand_PersistsConfig()
    {
        // First load so _config is populated
        var config = new Configuration
        {
            LibraryPaths = new List<string>(),
            AutoScanOnStartup = true,
            DefaultVolume = 75
        };
        _libraryManagerMock.Setup(m => m.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(config));
        await _vm.LoadAsync();

        _vm.AutoScanOnStartup = false;
        _vm.DefaultVolume = 60;

        _vm.SaveCommand.Execute(null);
        await Task.Delay(50);

        _configServiceMock.Verify(c => c.SaveConfigurationAsync(
            It.Is<Configuration>(cfg => cfg.AutoScanOnStartup == false && cfg.DefaultVolume == 60)),
            Times.Once);
        _vm.StatusMessage.Should().Contain("saved");
    }

    [Fact]
    public void Constructor_ThrowsOnNullLibraryManager()
    {
        var act = () => new SettingsViewModel(null!, _configServiceMock.Object, action => action());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullConfigService()
    {
        var act = () => new SettingsViewModel(_libraryManagerMock.Object, null!, action => action());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PropertyChanged_RaisedForAutoScanOnStartup()
    {
        var raised = false;
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SettingsViewModel.AutoScanOnStartup))
                raised = true;
        };
        _vm.AutoScanOnStartup = true;
        raised.Should().BeTrue();
    }

    [Fact]
    public void PropertyChanged_RaisedForDefaultVolume()
    {
        var raised = false;
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SettingsViewModel.DefaultVolume))
                raised = true;
        };
        _vm.DefaultVolume = 50;
        raised.Should().BeTrue();
    }
}
