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
    private readonly Mock<ILibraryStatsService> _libraryStatsServiceMock;
    private readonly SettingsViewModel _vm;

    public SettingsViewModelTests()
    {
        _libraryManagerMock = new Mock<ILibraryManager>();
        _configServiceMock = new Mock<IConfigurationService>();
        _libraryStatsServiceMock = new Mock<ILibraryStatsService>();
        _vm = new SettingsViewModel(_libraryManagerMock.Object, _configServiceMock.Object, _libraryStatsServiceMock.Object, action => action());
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

        _vm.LibraryPaths.Select(d => d.Path).Should().BeEquivalentTo(new[] { @"C:\Music", @"D:\Music" });
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
        _vm.LibraryPaths.Select(d => d.Path).Should().Contain(@"D:\New");
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
        _vm.LibraryPaths.Select(d => d.Path).Should().NotContain(@"C:\Music");
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
    public async Task AutoSave_PersistsOnPropertyChange()
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

        _configServiceMock.Invocations.Clear();

        _vm.AutoScanOnStartup = false;
        await Task.Delay(50);

        _configServiceMock.Verify(c => c.SaveConfigurationAsync(
            It.Is<Configuration>(cfg => cfg.AutoScanOnStartup == false)),
            Times.Once);
    }

    [Fact]
    public void Constructor_ThrowsOnNullLibraryManager()
    {
        var act = () => new SettingsViewModel(null!, _configServiceMock.Object, _libraryStatsServiceMock.Object, action => action());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullConfigService()
    {
        var act = () => new SettingsViewModel(_libraryManagerMock.Object, null!, _libraryStatsServiceMock.Object, action => action());
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

    [Fact]
    public async Task LoadAsync_PopulatesNewProperties()
    {
        var config = new Configuration
        {
            LibraryPaths = new List<string>(),
            ResumeSession = false,
            AutoPlayOnStartup = true,
            TextSize = "Large",
            DoubleClickAction = "AddToQueue",
            DefaultSortOrder = "Title",
            DefaultOrganizeMode = "Move",
            DefaultDuplicateStrategy = "Overwrite",
            UnknownArtistName = "No Artist",
            UnknownAlbumName = "No Album"
        };
        _libraryManagerMock.Setup(m => m.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(config));

        await _vm.LoadAsync();

        _vm.ResumeSession.Should().BeFalse();
        _vm.AutoPlayOnStartup.Should().BeTrue();
        _vm.TextSize.Should().Be("Large");
        _vm.DoubleClickAction.Should().Be("AddToQueue");
        _vm.DefaultSortOrder.Should().Be("Title");
        _vm.DefaultOrganizeMode.Should().Be("Move");
        _vm.DefaultDuplicateStrategy.Should().Be("Overwrite");
        _vm.UnknownArtistName.Should().Be("No Artist");
        _vm.UnknownAlbumName.Should().Be("No Album");
    }

    [Fact]
    public async Task AutoSave_PersistsNewProperties()
    {
        var config = new Configuration();
        _libraryManagerMock.Setup(m => m.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(config));
        await _vm.LoadAsync();

        _configServiceMock.Invocations.Clear();

        _vm.DefaultSortOrder = "Album";
        await Task.Delay(50);

        _configServiceMock.Verify(c => c.SaveConfigurationAsync(
            It.Is<Configuration>(cfg => cfg.DefaultSortOrder == "Album")),
            Times.Once);
    }

    [Fact]
    public async Task AutoSave_DoesNotFireDuringLoad()
    {
        var config = new Configuration
        {
            LibraryPaths = new List<string>(),
            AutoScanOnStartup = false,
            DefaultVolume = 50
        };
        _libraryManagerMock.Setup(m => m.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(config));

        await _vm.LoadAsync();
        await Task.Delay(50);

        // SaveConfigurationAsync should NOT have been called by auto-save during load
        _configServiceMock.Verify(c => c.SaveConfigurationAsync(It.IsAny<Configuration>()), Times.Never);
    }

    [Fact]
    public async Task ResetToDefaults_ResetsNewProperties()
    {
        var config = new Configuration
        {
            ResumeSession = false,
            AutoPlayOnStartup = true,
            TextSize = "Large",
            DoubleClickAction = "AddToQueue",
            DefaultSortOrder = "Title"
        };
        _libraryManagerMock.Setup(m => m.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(config));
        await _vm.LoadAsync();

        _vm.ResetToDefaultsCommand.Execute(null);
        await Task.Delay(50);

        _vm.ResumeSession.Should().BeTrue();
        _vm.AutoPlayOnStartup.Should().BeFalse();
        _vm.TextSize.Should().Be("Medium");
        _vm.DoubleClickAction.Should().Be("PlayNow");
        _vm.DefaultSortOrder.Should().Be("Artist");
    }

    [Fact]
    public void ToggleAllScanPaths_SelectsAll_ThenDeselectsAll()
    {
        // Setup: load paths
        var config = new Configuration
        {
            LibraryPaths = new List<string> { @"C:\Music", @"D:\Music" }
        };
        _libraryManagerMock.Setup(m => m.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(config));
        _vm.LoadAsync().GetAwaiter().GetResult();

        // Select all
        _vm.ToggleAllScanPathsCommand.Execute(null);

        _vm.SelectedScanPaths.Should().BeEquivalentTo(new[] { @"C:\Music", @"D:\Music" });
        _vm.AllScanPathsSelected.Should().BeTrue();
        _vm.ToggleAllButtonText.Should().Be("Deselect All");
        _vm.LibraryPaths.All(p => p.IsSelectedForScan).Should().BeTrue();

        // Deselect all
        _vm.ToggleAllScanPathsCommand.Execute(null);

        _vm.SelectedScanPaths.Should().BeEmpty();
        _vm.AllScanPathsSelected.Should().BeFalse();
        _vm.ToggleAllButtonText.Should().Be("Select All");
        _vm.LibraryPaths.All(p => !p.IsSelectedForScan).Should().BeTrue();
    }

    [Fact]
    public void ToggleScanPath_SetsIsSelectedForScan()
    {
        var config = new Configuration
        {
            LibraryPaths = new List<string> { @"C:\Music" }
        };
        _libraryManagerMock.Setup(m => m.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(config));
        _vm.LoadAsync().GetAwaiter().GetResult();

        _vm.ToggleScanPathCommand.Execute(@"C:\Music");
        _vm.LibraryPaths[0].IsSelectedForScan.Should().BeTrue();

        _vm.ToggleScanPathCommand.Execute(@"C:\Music");
        _vm.LibraryPaths[0].IsSelectedForScan.Should().BeFalse();
    }

    [Fact]
    public void HasStatusMessage_ReflectsStatusMessage()
    {
        _vm.HasStatusMessage.Should().BeFalse();

        // Trigger a status message via scan with no paths
        _vm.ScanSelectedCommand.Execute(null);
        // Wait briefly for async
        System.Threading.Thread.Sleep(50);

        _vm.HasStatusMessage.Should().BeTrue();
    }

    [Fact]
    public async Task ScanSelectedCommand_ScansSelectedPaths()
    {
        var scanResult = new ScanResult { TracksAdded = 5, TracksUpdated = 0, TracksRemoved = 0 };
        _libraryManagerMock.Setup(m => m.ScanSelectedLibrariesAsync(It.IsAny<IReadOnlyList<string>>()))
            .ReturnsAsync(Result.Success(scanResult));

        _vm.SelectedScanPaths.Add(@"C:\Music");
        _vm.ScanSelectedCommand.Execute(null);
        await Task.Delay(50);

        _libraryManagerMock.Verify(m => m.ScanSelectedLibrariesAsync(
            It.Is<IReadOnlyList<string>>(p => p.Count == 1 && p[0] == @"C:\Music")), Times.Once);
        _vm.StatusMessage.Should().Contain("5 added");
    }

    [Fact]
    public async Task ScanSelectedCommand_ShowsMessage_WhenNoPathsSelected()
    {
        _vm.ScanSelectedCommand.Execute(null);
        await Task.Delay(50);

        _vm.StatusMessage.Should().Contain("No paths selected");
        _libraryManagerMock.Verify(m => m.ScanSelectedLibrariesAsync(It.IsAny<IReadOnlyList<string>>()), Times.Never);
    }

    [Fact]
    public void Constructor_ThrowsOnNullLibraryStatsService()
    {
        var act = () => new SettingsViewModel(_libraryManagerMock.Object, _configServiceMock.Object, null!, action => action());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ToggleDirectoriesData_ExpandsAndPopulatesItems()
    {
        var config = new Configuration { LibraryPaths = new List<string> { @"C:\Music", @"D:\Music" } };
        _libraryManagerMock.Setup(m => m.LoadConfigurationAsync()).ReturnsAsync(Result.Success(config));
        await _vm.LoadAsync();

        _vm.ToggleDirectoriesDataCommand.Execute(null);
        await Task.Delay(50);

        _vm.IsDirectoriesDataExpanded.Should().BeTrue();
        _vm.DirectoryStatsItems.Should().HaveCount(2);
        _vm.DirectoryStatsItems[0].Path.Should().Be(@"C:\Music");
        _vm.DirectoryStatsItems[1].Path.Should().Be(@"D:\Music");
    }

    [Fact]
    public async Task ToggleDirectoriesData_CollapsesOnSecondToggle()
    {
        var config = new Configuration { LibraryPaths = new List<string> { @"C:\Music" } };
        _libraryManagerMock.Setup(m => m.LoadConfigurationAsync()).ReturnsAsync(Result.Success(config));
        await _vm.LoadAsync();

        _vm.ToggleDirectoriesDataCommand.Execute(null);
        await Task.Delay(50);
        _vm.ToggleDirectoriesDataCommand.Execute(null);
        await Task.Delay(50);

        _vm.IsDirectoriesDataExpanded.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleDirectory_ExpandsAndLoadsStats()
    {
        var config = new Configuration { LibraryPaths = new List<string> { @"C:\Music" } };
        _libraryManagerMock.Setup(m => m.LoadConfigurationAsync()).ReturnsAsync(Result.Success(config));
        _libraryStatsServiceMock.Setup(s => s.GetDirectoryStatsAsync(@"C:\Music"))
            .ReturnsAsync(new DirectoryStats(10, 3, 2));
        await _vm.LoadAsync();

        _vm.ToggleDirectoriesDataCommand.Execute(null);
        await Task.Delay(50);

        _vm.ToggleDirectoryCommand.Execute(_vm.DirectoryStatsItems[0]);
        await Task.Delay(50);

        _vm.DirectoryStatsItems[0].IsExpanded.Should().BeTrue();
        _vm.DirectoryStatsItems[0].TrackCount.Should().Be(10);
        _vm.DirectoryStatsItems[0].AlbumCount.Should().Be(3);
        _vm.DirectoryStatsItems[0].ArtistCount.Should().Be(2);
        _vm.DirectoryStatsItems[0].HasLoaded.Should().BeTrue();
    }

    [Fact]
    public async Task ToggleAllDirectories_ExpandsAndCollapsesAll()
    {
        var config = new Configuration { LibraryPaths = new List<string> { @"C:\Music", @"D:\Music" } };
        _libraryManagerMock.Setup(m => m.LoadConfigurationAsync()).ReturnsAsync(Result.Success(config));
        _libraryStatsServiceMock.Setup(s => s.GetDirectoryStatsAsync(It.IsAny<string>()))
            .ReturnsAsync(new DirectoryStats(5, 2, 1));
        await _vm.LoadAsync();

        _vm.ToggleDirectoriesDataCommand.Execute(null);
        await Task.Delay(50);

        // Expand all
        _vm.ToggleAllDirectoriesCommand.Execute(null);
        await Task.Delay(50);

        _vm.AreAllDirectoriesExpanded.Should().BeTrue();
        _vm.DirectoryStatsItems.All(d => d.IsExpanded).Should().BeTrue();

        // Collapse all
        _vm.ToggleAllDirectoriesCommand.Execute(null);
        await Task.Delay(50);

        _vm.AreAllDirectoriesExpanded.Should().BeFalse();
        _vm.DirectoryStatsItems.All(d => !d.IsExpanded).Should().BeTrue();
    }

    [Fact]
    public void DirectoryStatsItem_ToggleExpanded_RaisesPropertyChanged()
    {
        var item = new SettingsViewModel.DirectoryStatsItem(@"C:\Music");
        var raised = false;
        item.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(SettingsViewModel.DirectoryStatsItem.IsExpanded)) raised = true; };
        item.IsExpanded = true;
        raised.Should().BeTrue();
    }

    [Theory]
    [InlineData(nameof(SettingsViewModel.ResumeSession))]
    [InlineData(nameof(SettingsViewModel.AutoPlayOnStartup))]
    [InlineData(nameof(SettingsViewModel.TextSize))]
    [InlineData(nameof(SettingsViewModel.DoubleClickAction))]
    [InlineData(nameof(SettingsViewModel.DefaultSortOrder))]
    public void PropertyChanged_RaisedForNewProperties(string propertyName)
    {
        var raised = false;
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == propertyName)
                raised = true;
        };

        switch (propertyName)
        {
            case nameof(SettingsViewModel.ResumeSession): _vm.ResumeSession = false; break;
            case nameof(SettingsViewModel.AutoPlayOnStartup): _vm.AutoPlayOnStartup = true; break;
            case nameof(SettingsViewModel.TextSize): _vm.TextSize = "Large"; break;
            case nameof(SettingsViewModel.DoubleClickAction): _vm.DoubleClickAction = "AddToQueue"; break;
            case nameof(SettingsViewModel.DefaultSortOrder): _vm.DefaultSortOrder = "Title"; break;
        }

        raised.Should().BeTrue();
    }
}
