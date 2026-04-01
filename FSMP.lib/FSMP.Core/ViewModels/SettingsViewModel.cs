using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;

namespace FSMP.Core.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly ILibraryManager _libraryManager;
    private readonly IConfigurationService _configService;
    private readonly ILibraryStatsService _libraryStatsService;
    private readonly Action<Action> _dispatchToUI;

    private bool _autoScanOnStartup;
    private int _defaultVolume = 75;
    private bool _isBusy;
    private string _statusMessage = string.Empty;
    private string _selectedTheme = "Light";
    private bool _allowUnsaveFromTagList;
    private bool _resumeSession = true;
    private bool _autoPlayOnStartup;
    private string _textSize = "Medium";
    private string _doubleClickAction = "PlayNow";
    private string _defaultSortOrder = "Artist";
    private string _defaultOrganizeMode = "Copy";
    private string _defaultDuplicateStrategy = "Skip";
    private string _unknownArtistName = "Unknown Artist";
    private string _unknownAlbumName = "Unknown Album";
    private Configuration? _config;
    private bool _isLoading;
    private int _totalTrackCount;
    private int _totalAlbumCount;
    private int _totalArtistCount;
    private bool _isDirectoriesDataExpanded;
    private bool _areAllDirectoriesExpanded;
    private bool _directoriesDataLoadedOnce;

    public SettingsViewModel(ILibraryManager libraryManager, IConfigurationService configService, ILibraryStatsService libraryStatsService, Action<Action> dispatchToUI)
    {
        _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _libraryStatsService = libraryStatsService ?? throw new ArgumentNullException(nameof(libraryStatsService));
        _dispatchToUI = dispatchToUI ?? throw new ArgumentNullException(nameof(dispatchToUI));

        LibraryPaths = new ObservableCollection<DirectoryItem>();
        DirectoryStatsItems = new ObservableCollection<DirectoryStatsItem>();
        AddPathCommand = new AsyncRelayCommand<string>(OnAddPath);
        RemovePathCommand = new AsyncRelayCommand<string>(OnRemovePath);
        ScanCommand = new AsyncRelayCommand(OnScan);
        ScanSelectedCommand = new AsyncRelayCommand(OnScanSelected);
        ToggleScanPathCommand = new RelayCommand<string>(OnToggleScanPath);
        ToggleAllScanPathsCommand = new RelayCommand(OnToggleAllScanPaths);
        ResetToDefaultsCommand = new AsyncRelayCommand(OnResetToDefaults);
        EditPathCommand = new RelayCommand<DirectoryItem>(OnEditPath);
        ConfirmEditCommand = new AsyncRelayCommand<DirectoryItem>(OnConfirmEdit);
        CancelEditCommand = new RelayCommand<DirectoryItem>(OnCancelEdit);
        ToggleDirectoriesDataCommand = new AsyncRelayCommand(OnToggleDirectoriesData);
        ToggleAllDirectoriesCommand = new AsyncRelayCommand(OnToggleAllDirectories);
        ToggleDirectoryCommand = new AsyncRelayCommand<DirectoryStatsItem>(OnToggleDirectory);
    }

    public ObservableCollection<DirectoryItem> LibraryPaths { get; }

    public bool AutoScanOnStartup
    {
        get => _autoScanOnStartup;
        set { if (SetProperty(ref _autoScanOnStartup, value)) AutoSaveAsync(); }
    }

    public int DefaultVolume
    {
        get => _defaultVolume;
        set { if (SetProperty(ref _defaultVolume, value)) AutoSaveAsync(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
                OnPropertyChanged(nameof(CanScanAll));
                OnPropertyChanged(nameof(CanScanSelected));
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set { if (SetProperty(ref _statusMessage, value)) OnPropertyChanged(nameof(HasStatusMessage)); }
    }

    public IReadOnlyList<string> AvailableThemes { get; } = new[] { "Light", "Dark", "Light Blue" };

    public string SelectedTheme
    {
        get => _selectedTheme;
        set { if (SetProperty(ref _selectedTheme, value)) AutoSaveAsync(); }
    }

    public bool AllowUnsaveFromTagList
    {
        get => _allowUnsaveFromTagList;
        set { if (SetProperty(ref _allowUnsaveFromTagList, value)) AutoSaveAsync(); }
    }

    public bool ResumeSession
    {
        get => _resumeSession;
        set { if (SetProperty(ref _resumeSession, value)) AutoSaveAsync(); }
    }

    public bool AutoPlayOnStartup
    {
        get => _autoPlayOnStartup;
        set { if (SetProperty(ref _autoPlayOnStartup, value)) AutoSaveAsync(); }
    }

    public string TextSize
    {
        get => _textSize;
        set { if (SetProperty(ref _textSize, value)) AutoSaveAsync(); }
    }

    public IReadOnlyList<string> AvailableTextSizes { get; } = new[] { "Small", "Medium", "Large", "Extra Large" };

    public string DoubleClickAction
    {
        get => _doubleClickAction;
        set { if (SetProperty(ref _doubleClickAction, value)) AutoSaveAsync(); }
    }

    public IReadOnlyList<string> AvailableDoubleClickActions { get; } = new[] { "PlayNow", "AddToQueue", "PlayNext" };

    public string DefaultSortOrder
    {
        get => _defaultSortOrder;
        set { if (SetProperty(ref _defaultSortOrder, value)) AutoSaveAsync(); }
    }

    public IReadOnlyList<string> AvailableSortOrders { get; } = new[] { "Artist", "Album", "Title", "DateAdded" };

    public string DefaultOrganizeMode
    {
        get => _defaultOrganizeMode;
        set { if (SetProperty(ref _defaultOrganizeMode, value)) AutoSaveAsync(); }
    }

    public string DefaultDuplicateStrategy
    {
        get => _defaultDuplicateStrategy;
        set { if (SetProperty(ref _defaultDuplicateStrategy, value)) AutoSaveAsync(); }
    }

    public string UnknownArtistName
    {
        get => _unknownArtistName;
        set { if (SetProperty(ref _unknownArtistName, value)) AutoSaveAsync(); }
    }

    public string UnknownAlbumName
    {
        get => _unknownAlbumName;
        set { if (SetProperty(ref _unknownAlbumName, value)) AutoSaveAsync(); }
    }

    public ObservableCollection<string> SelectedScanPaths { get; } = new();

    public bool IsNotBusy => !IsBusy;
    public bool HasLibraryPaths => LibraryPaths.Count > 0;
    public bool HasSelectedScanPaths => SelectedScanPaths.Count > 0;
    public bool CanScanAll => HasLibraryPaths && IsNotBusy;
    public bool CanScanSelected => HasSelectedScanPaths && IsNotBusy;
    public bool AllScanPathsSelected => LibraryPaths.Count > 0 && LibraryPaths.All(p => p.IsSelectedForScan);
    public string ToggleAllButtonText => AllScanPathsSelected ? "Deselect All" : "Select All";
    public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);

    public int TotalTrackCount
    {
        get => _totalTrackCount;
        private set => SetProperty(ref _totalTrackCount, value);
    }

    public int TotalAlbumCount
    {
        get => _totalAlbumCount;
        private set => SetProperty(ref _totalAlbumCount, value);
    }

    public int TotalArtistCount
    {
        get => _totalArtistCount;
        private set => SetProperty(ref _totalArtistCount, value);
    }

    public string AppVersion { get; } = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "1.0.0";

    public bool IsDirectoriesDataExpanded
    {
        get => _isDirectoriesDataExpanded;
        private set => SetProperty(ref _isDirectoriesDataExpanded, value);
    }

    public bool AreAllDirectoriesExpanded
    {
        get => _areAllDirectoriesExpanded;
        private set => SetProperty(ref _areAllDirectoriesExpanded, value);
    }

    public ObservableCollection<DirectoryStatsItem> DirectoryStatsItems { get; }

    public ICommand ToggleDirectoriesDataCommand { get; }
    public ICommand ToggleAllDirectoriesCommand { get; }
    public ICommand ToggleDirectoryCommand { get; }

    public ICommand AddPathCommand { get; }
    public ICommand RemovePathCommand { get; }
    public ICommand ScanCommand { get; }
    public ICommand ScanSelectedCommand { get; }
    public ICommand ToggleScanPathCommand { get; }
    public ICommand ToggleAllScanPathsCommand { get; }
    public ICommand ResetToDefaultsCommand { get; }
    public ICommand EditPathCommand { get; }
    public ICommand ConfirmEditCommand { get; }
    public ICommand CancelEditCommand { get; }

    public async Task LoadAsync()
    {
        var result = await _libraryManager.LoadConfigurationAsync();
        if (result.IsSuccess && result.Value is not null)
        {
            _config = result.Value;
            _isLoading = true;
            _dispatchToUI(() =>
            {
                LibraryPaths.Clear();
                foreach (var path in _config.LibraryPaths)
                    LibraryPaths.Add(new DirectoryItem(path));
                OnPropertyChanged(nameof(HasLibraryPaths));
                OnPropertyChanged(nameof(CanScanAll));
                AutoScanOnStartup = _config.AutoScanOnStartup;
                DefaultVolume = _config.DefaultVolume;
                SelectedTheme = _config.Theme;
                AllowUnsaveFromTagList = _config.AllowUnsaveFromTagList;
                ResumeSession = _config.ResumeSession;
                AutoPlayOnStartup = _config.AutoPlayOnStartup;
                TextSize = _config.TextSize;
                DoubleClickAction = _config.DoubleClickAction;
                DefaultSortOrder = _config.DefaultSortOrder;
                DefaultOrganizeMode = _config.DefaultOrganizeMode;
                DefaultDuplicateStrategy = _config.DefaultDuplicateStrategy;
                UnknownArtistName = _config.UnknownArtistName;
                UnknownAlbumName = _config.UnknownAlbumName;
            });
            _isLoading = false;
        }

        try
        {
            var totalStats = await _libraryStatsService.GetTotalStatsAsync();
            TotalTrackCount = totalStats.TrackCount;
            TotalAlbumCount = totalStats.AlbumCount;
            TotalArtistCount = totalStats.ArtistCount;
        }
        catch
        {
            // Stats are non-critical; leave at zero
        }
    }

    private async Task OnAddPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        var result = await _libraryManager.AddLibraryPathAsync(path);
        if (result.IsSuccess)
        {
            await LoadAsync();
            OnPropertyChanged(nameof(HasLibraryPaths));
            OnPropertyChanged(nameof(CanScanAll));
        }
    }

    private async Task OnRemovePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        var result = await _libraryManager.RemoveLibraryPathAsync(path);
        if (result.IsSuccess)
        {
            await LoadAsync();
            OnPropertyChanged(nameof(HasLibraryPaths));
            OnPropertyChanged(nameof(CanScanAll));
            // Remove from selected if it was selected
            if (SelectedScanPaths.Contains(path))
            {
                SelectedScanPaths.Remove(path);
                OnPropertyChanged(nameof(HasSelectedScanPaths));
                OnPropertyChanged(nameof(CanScanSelected));
            }
        }
    }

    private async Task OnScan()
    {
        IsBusy = true;
        StatusMessage = "Scanning...";

        var result = await _libraryManager.ScanAllLibrariesAsync();
        if (result.IsSuccess && result.Value is not null)
        {
            var scan = result.Value;
            ApplyDirectoryResults(scan);
            StatusMessage = $"Scan complete: {scan.TracksAdded} added, {scan.TracksUpdated} updated, {scan.TracksRemoved} removed";
        }
        else
        {
            StatusMessage = $"Scan failed: {result.ErrorMessage}";
        }

        IsBusy = false;
    }

    private void OnToggleScanPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        var item = LibraryPaths.FirstOrDefault(p => p.Path == path);

        if (SelectedScanPaths.Contains(path))
        {
            SelectedScanPaths.Remove(path);
            if (item is not null) item.IsSelectedForScan = false;
        }
        else
        {
            SelectedScanPaths.Add(path);
            if (item is not null) item.IsSelectedForScan = true;
        }

        OnPropertyChanged(nameof(HasSelectedScanPaths));
        OnPropertyChanged(nameof(CanScanSelected));
        OnPropertyChanged(nameof(AllScanPathsSelected));
        OnPropertyChanged(nameof(ToggleAllButtonText));
    }

    private void OnToggleAllScanPaths()
    {
        var selectAll = !AllScanPathsSelected;

        SelectedScanPaths.Clear();
        foreach (var item in LibraryPaths)
        {
            item.IsSelectedForScan = selectAll;
            if (selectAll)
                SelectedScanPaths.Add(item.Path);
        }

        OnPropertyChanged(nameof(HasSelectedScanPaths));
        OnPropertyChanged(nameof(CanScanSelected));
        OnPropertyChanged(nameof(AllScanPathsSelected));
        OnPropertyChanged(nameof(ToggleAllButtonText));
    }

    private async Task OnScanSelected()
    {
        if (SelectedScanPaths.Count == 0)
        {
            StatusMessage = "No paths selected for scanning.";
            return;
        }

        IsBusy = true;
        StatusMessage = $"Scanning {SelectedScanPaths.Count} selected path(s)...";

        var result = await _libraryManager.ScanSelectedLibrariesAsync(SelectedScanPaths.ToList());
        if (result.IsSuccess && result.Value is not null)
        {
            var scan = result.Value;
            ApplyDirectoryResults(scan);
            StatusMessage = $"Scan complete: {scan.TracksAdded} added, {scan.TracksUpdated} updated, {scan.TracksRemoved} removed";
        }
        else
        {
            StatusMessage = $"Scan failed: {result.ErrorMessage}";
        }

        IsBusy = false;
    }

    private void ApplyDirectoryResults(ScanResult scan)
    {
        foreach (var dirResult in scan.DirectoryResults)
        {
            var item = LibraryPaths.FirstOrDefault(p => p.Path == dirResult.Path);
            if (item is null) continue;

            item.HasScanResult = true;
            item.ScanFailed = !dirResult.Success;

            if (dirResult.Success)
            {
                item.ScanSummary = $"{dirResult.TracksAdded} added, {dirResult.TracksUpdated} updated, {dirResult.TracksRemoved} removed";
                // Untick successful directories
                item.IsSelectedForScan = false;
                SelectedScanPaths.Remove(item.Path);
            }
            else
            {
                item.ScanSummary = dirResult.ErrorMessage ?? "Unknown error";
            }
        }

        OnPropertyChanged(nameof(HasSelectedScanPaths));
        OnPropertyChanged(nameof(CanScanSelected));
        OnPropertyChanged(nameof(AllScanPathsSelected));
        OnPropertyChanged(nameof(ToggleAllButtonText));
    }

    private void OnEditPath(DirectoryItem? item)
    {
        if (item is null) return;
        item.EditText = item.Path;
        item.IsEditing = true;
    }

    private async Task OnConfirmEdit(DirectoryItem? item)
    {
        if (item is null || string.IsNullOrWhiteSpace(item.EditText)) return;
        var oldPath = item.Path;
        var newPath = item.EditText.Trim();
        if (oldPath == newPath) { item.IsEditing = false; return; }

        var removeResult = await _libraryManager.RemoveLibraryPathAsync(oldPath);
        if (removeResult.IsSuccess)
        {
            var addResult = await _libraryManager.AddLibraryPathAsync(newPath);
            if (addResult.IsSuccess)
            {
                await LoadAsync();
                OnPropertyChanged(nameof(HasLibraryPaths));
                OnPropertyChanged(nameof(CanScanAll));
                return;
            }
            // Rollback: re-add old path if add failed
            await _libraryManager.AddLibraryPathAsync(oldPath);
        }
        item.IsEditing = false;
    }

    private void OnCancelEdit(DirectoryItem? item)
    {
        if (item is null) return;
        item.IsEditing = false;
    }

    private async Task OnToggleDirectoriesData()
    {
        IsDirectoriesDataExpanded = !IsDirectoriesDataExpanded;
        if (IsDirectoriesDataExpanded && !_directoriesDataLoadedOnce)
        {
            _directoriesDataLoadedOnce = true;
            _dispatchToUI(() =>
            {
                DirectoryStatsItems.Clear();
                foreach (var dir in LibraryPaths)
                    DirectoryStatsItems.Add(new DirectoryStatsItem(dir.Path));
            });
        }
    }

    private async Task OnToggleAllDirectories()
    {
        var expandAll = !AreAllDirectoriesExpanded;
        AreAllDirectoriesExpanded = expandAll;

        foreach (var item in DirectoryStatsItems)
        {
            if (expandAll && !item.IsExpanded)
            {
                item.IsExpanded = true;
                if (!item.HasLoaded)
                    await LoadDirectoryStatsAsync(item);
            }
            else if (!expandAll && item.IsExpanded)
            {
                item.IsExpanded = false;
            }
        }
    }

    private async Task OnToggleDirectory(DirectoryStatsItem? item)
    {
        if (item is null) return;
        item.IsExpanded = !item.IsExpanded;
        if (item.IsExpanded && !item.HasLoaded)
            await LoadDirectoryStatsAsync(item);
        AreAllDirectoriesExpanded = DirectoryStatsItems.Count > 0 && DirectoryStatsItems.All(d => d.IsExpanded);
    }

    private async Task LoadDirectoryStatsAsync(DirectoryStatsItem item)
    {
        item.IsLoading = true;
        try
        {
            var stats = await _libraryStatsService.GetDirectoryStatsAsync(item.Path);
            item.TrackCount = stats.TrackCount;
            item.AlbumCount = stats.AlbumCount;
            item.ArtistCount = stats.ArtistCount;
            item.HasLoaded = true;
        }
        catch
        {
            item.TrackCount = 0;
            item.AlbumCount = 0;
            item.ArtistCount = 0;
        }
        finally
        {
            item.IsLoading = false;
        }
    }

    private async Task OnResetToDefaults()
    {
        var defaults = new Configuration();
        _config = defaults;
        _isLoading = true;
        _dispatchToUI(() =>
        {
            LibraryPaths.Clear();
            AutoScanOnStartup = defaults.AutoScanOnStartup;
            DefaultVolume = defaults.DefaultVolume;
            SelectedTheme = defaults.Theme;
            AllowUnsaveFromTagList = defaults.AllowUnsaveFromTagList;
            ResumeSession = defaults.ResumeSession;
            AutoPlayOnStartup = defaults.AutoPlayOnStartup;
            TextSize = defaults.TextSize;
            DoubleClickAction = defaults.DoubleClickAction;
            DefaultSortOrder = defaults.DefaultSortOrder;
            DefaultOrganizeMode = defaults.DefaultOrganizeMode;
            DefaultDuplicateStrategy = defaults.DefaultDuplicateStrategy;
            UnknownArtistName = defaults.UnknownArtistName;
            UnknownAlbumName = defaults.UnknownAlbumName;
        });
        _isLoading = false;
        await _configService.SaveConfigurationAsync(defaults);
        StatusMessage = "Settings reset to defaults.";
    }

    private async void AutoSaveAsync()
    {
        if (_isLoading) return;
        try { await OnSave(); }
        catch { StatusMessage = "Failed to save settings."; }
    }

    private async Task OnSave()
    {
        if (_config is null)
            _config = new Configuration();

        _config.AutoScanOnStartup = AutoScanOnStartup;
        _config.DefaultVolume = DefaultVolume;
        _config.Theme = SelectedTheme;
        _config.AllowUnsaveFromTagList = AllowUnsaveFromTagList;
        _config.ResumeSession = ResumeSession;
        _config.AutoPlayOnStartup = AutoPlayOnStartup;
        _config.TextSize = TextSize;
        _config.DoubleClickAction = DoubleClickAction;
        _config.DefaultSortOrder = DefaultSortOrder;
        _config.DefaultOrganizeMode = DefaultOrganizeMode;
        _config.DefaultDuplicateStrategy = DefaultDuplicateStrategy;
        _config.UnknownArtistName = UnknownArtistName;
        _config.UnknownAlbumName = UnknownAlbumName;
        await _configService.SaveConfigurationAsync(_config);
        StatusMessage = "Settings saved.";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private sealed class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        public AsyncRelayCommand(Func<Task> execute) => _execute = execute;
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public async void Execute(object? parameter) => await _execute();
    }

    private sealed class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T?, Task> _execute;
        public AsyncRelayCommand(Func<T?, Task> execute) => _execute = execute;
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public async void Execute(object? parameter) => await _execute(parameter is T t ? t : default);
    }

    private sealed class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
    }

    private sealed class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        public RelayCommand(Action<T?> execute) => _execute = execute;
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute(parameter is T t ? t : default);
    }

    public class DirectoryStatsItem : INotifyPropertyChanged
    {
        private string _path;
        private bool _isExpanded;
        private bool _isLoading;
        private int _trackCount;
        private int _albumCount;
        private int _artistCount;
        private bool _hasLoaded;

        public DirectoryStatsItem(string path) => _path = path;

        public string Path
        {
            get => _path;
            set { if (_path != value) { _path = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Path))); } }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set { if (_isExpanded != value) { _isExpanded = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded))); } }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { if (_isLoading != value) { _isLoading = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading))); } }
        }

        public int TrackCount
        {
            get => _trackCount;
            set { if (_trackCount != value) { _trackCount = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrackCount))); } }
        }

        public int AlbumCount
        {
            get => _albumCount;
            set { if (_albumCount != value) { _albumCount = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlbumCount))); } }
        }

        public int ArtistCount
        {
            get => _artistCount;
            set { if (_artistCount != value) { _artistCount = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ArtistCount))); } }
        }

        public bool HasLoaded
        {
            get => _hasLoaded;
            set { if (_hasLoaded != value) { _hasLoaded = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasLoaded))); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class DirectoryItem : INotifyPropertyChanged
    {
        private string _path;
        private string _editText = string.Empty;
        private bool _isEditing;
        private bool _isSelectedForScan;
        private bool _hasScanResult;
        private bool _scanFailed;
        private string _scanSummary = string.Empty;

        public DirectoryItem(string path) => _path = path;

        public string Path
        {
            get => _path;
            set { if (_path != value) { _path = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Path))); } }
        }

        public string EditText
        {
            get => _editText;
            set { if (_editText != value) { _editText = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditText))); } }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set { if (_isEditing != value) { _isEditing = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEditing))); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNotEditing))); } }
        }

        public bool IsNotEditing => !_isEditing;

        public bool IsSelectedForScan
        {
            get => _isSelectedForScan;
            set { if (_isSelectedForScan != value) { _isSelectedForScan = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelectedForScan))); } }
        }

        public bool HasScanResult
        {
            get => _hasScanResult;
            set { if (_hasScanResult != value) { _hasScanResult = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasScanResult))); } }
        }

        public bool ScanFailed
        {
            get => _scanFailed;
            set { if (_scanFailed != value) { _scanFailed = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScanFailed))); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScanSucceeded))); } }
        }

        public bool ScanSucceeded => _hasScanResult && !_scanFailed;

        public string ScanSummary
        {
            get => _scanSummary;
            set { if (_scanSummary != value) { _scanSummary = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScanSummary))); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
