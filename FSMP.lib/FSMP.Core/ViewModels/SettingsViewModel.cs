using System.Collections.ObjectModel;
using System.ComponentModel;
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
        SaveCommand = new AsyncRelayCommand(OnSave);
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

    public ICommand AddPathCommand { get; }
    public ICommand RemovePathCommand { get; }
    public ICommand ScanCommand { get; }
    public ICommand SaveCommand { get; }

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

    private async Task OnSave()
    {
        if (_config is null)
            _config = new Configuration();

        _config.AutoScanOnStartup = AutoScanOnStartup;
        _config.DefaultVolume = DefaultVolume;
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
