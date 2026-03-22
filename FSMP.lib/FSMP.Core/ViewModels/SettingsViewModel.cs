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

    public SettingsViewModel(ILibraryManager libraryManager, IConfigurationService configService, Action<Action> dispatchToUI)
    {
        _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _dispatchToUI = dispatchToUI ?? throw new ArgumentNullException(nameof(dispatchToUI));

        LibraryPaths = new ObservableCollection<string>();
        AddPathCommand = new AsyncRelayCommand<string>(OnAddPath);
        RemovePathCommand = new AsyncRelayCommand<string>(OnRemovePath);
        ScanCommand = new AsyncRelayCommand(OnScan);
        ScanSelectedCommand = new AsyncRelayCommand(OnScanSelected);
        SaveCommand = new AsyncRelayCommand(OnSave);
        ResetToDefaultsCommand = new AsyncRelayCommand(OnResetToDefaults);
    }

    public ObservableCollection<string> LibraryPaths { get; }

    public bool AutoScanOnStartup
    {
        get => _autoScanOnStartup;
        set => SetProperty(ref _autoScanOnStartup, value);
    }

    public int DefaultVolume
    {
        get => _defaultVolume;
        set => SetProperty(ref _defaultVolume, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public IReadOnlyList<string> AvailableThemes { get; } = new[] { "Light", "Dark", "Light Blue", "Custom" };

    public string SelectedTheme
    {
        get => _selectedTheme;
        set => SetProperty(ref _selectedTheme, value);
    }

    public bool AllowUnsaveFromTagList
    {
        get => _allowUnsaveFromTagList;
        set => SetProperty(ref _allowUnsaveFromTagList, value);
    }

    public bool ResumeSession
    {
        get => _resumeSession;
        set => SetProperty(ref _resumeSession, value);
    }

    public bool AutoPlayOnStartup
    {
        get => _autoPlayOnStartup;
        set => SetProperty(ref _autoPlayOnStartup, value);
    }

    public string TextSize
    {
        get => _textSize;
        set => SetProperty(ref _textSize, value);
    }

    public IReadOnlyList<string> AvailableTextSizes { get; } = new[] { "Small", "Medium", "Large", "Extra Large" };

    public string DoubleClickAction
    {
        get => _doubleClickAction;
        set => SetProperty(ref _doubleClickAction, value);
    }

    public IReadOnlyList<string> AvailableDoubleClickActions { get; } = new[] { "PlayNow", "AddToQueue", "PlayNext" };

    public string DefaultSortOrder
    {
        get => _defaultSortOrder;
        set => SetProperty(ref _defaultSortOrder, value);
    }

    public IReadOnlyList<string> AvailableSortOrders { get; } = new[] { "Artist", "Album", "Title", "DateAdded" };

    public string DefaultOrganizeMode
    {
        get => _defaultOrganizeMode;
        set => SetProperty(ref _defaultOrganizeMode, value);
    }

    public string DefaultDuplicateStrategy
    {
        get => _defaultDuplicateStrategy;
        set => SetProperty(ref _defaultDuplicateStrategy, value);
    }

    public string UnknownArtistName
    {
        get => _unknownArtistName;
        set => SetProperty(ref _unknownArtistName, value);
    }

    public string UnknownAlbumName
    {
        get => _unknownAlbumName;
        set => SetProperty(ref _unknownAlbumName, value);
    }

    public ObservableCollection<string> SelectedScanPaths { get; } = new();

    public string AppVersion { get; } = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "1.0.0";

    public ICommand AddPathCommand { get; }
    public ICommand RemovePathCommand { get; }
    public ICommand ScanCommand { get; }
    public ICommand ScanSelectedCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ResetToDefaultsCommand { get; }

    public async Task LoadAsync()
    {
        var result = await _libraryManager.LoadConfigurationAsync();
        if (result.IsSuccess && result.Value is not null)
        {
            _config = result.Value;
            _dispatchToUI(() =>
            {
                LibraryPaths.Clear();
                foreach (var path in _config.LibraryPaths)
                    LibraryPaths.Add(path);
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
        }
    }

    private async Task OnAddPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        var result = await _libraryManager.AddLibraryPathAsync(path);
        if (result.IsSuccess)
            await LoadAsync();
    }

    private async Task OnRemovePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        var result = await _libraryManager.RemoveLibraryPathAsync(path);
        if (result.IsSuccess)
            await LoadAsync();
    }

    private async Task OnScan()
    {
        IsBusy = true;
        StatusMessage = "Scanning...";

        var result = await _libraryManager.ScanAllLibrariesAsync();
        if (result.IsSuccess && result.Value is not null)
        {
            var scan = result.Value;
            StatusMessage = $"Scan complete: {scan.TracksAdded} added, {scan.TracksUpdated} updated, {scan.TracksRemoved} removed";
        }
        else
        {
            StatusMessage = $"Scan failed: {result.ErrorMessage}";
        }

        IsBusy = false;
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
            StatusMessage = $"Scan complete: {scan.TracksAdded} added, {scan.TracksUpdated} updated, {scan.TracksRemoved} removed";
        }
        else
        {
            StatusMessage = $"Scan failed: {result.ErrorMessage}";
        }

        IsBusy = false;
    }

    private async Task OnResetToDefaults()
    {
        var defaults = new Configuration();
        _config = defaults;
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
        await _configService.SaveConfigurationAsync(defaults);
        StatusMessage = "Settings reset to defaults.";
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
}
