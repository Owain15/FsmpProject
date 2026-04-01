using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FSMP.Core.Interfaces;
using FSMO;

namespace FSMP.Core.ViewModels;

public class FileOrganizerViewModel : INotifyPropertyChanged
{
    private readonly IFileOrganizerService _organizer;
    private readonly IConfigurationService _configService;
    private readonly Action<Action> _dispatchToUI;

    private string _sourcePath = string.Empty;
    private string _destinationPath = string.Empty;
    private string _organizeMode = "Copy";
    private string _duplicateStrategy = "Skip";
    private string _statusMessage = string.Empty;
    private bool _isBusy;
    private OrganizePreview? _lastPreview;

    public FileOrganizerViewModel(IFileOrganizerService organizer, IConfigurationService configService, Action<Action> dispatchToUI)
    {
        _organizer = organizer ?? throw new ArgumentNullException(nameof(organizer));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _dispatchToUI = dispatchToUI ?? throw new ArgumentNullException(nameof(dispatchToUI));

        PreviewMappings = new ObservableCollection<FileMapping>();
        PreviewCommand = new AsyncRelayCommand(OnPreview);
        OrganizeCommand = new AsyncRelayCommand(OnOrganize);
    }

    public string SourcePath
    {
        get => _sourcePath;
        set => SetProperty(ref _sourcePath, value);
    }

    public string DestinationPath
    {
        get => _destinationPath;
        set => SetProperty(ref _destinationPath, value);
    }

    public string SelectedOrganizeMode
    {
        get => _organizeMode;
        set => SetProperty(ref _organizeMode, value);
    }

    public IReadOnlyList<string> AvailableOrganizeModes { get; } = new[] { "Copy", "Move" };

    public string SelectedDuplicateStrategy
    {
        get => _duplicateStrategy;
        set => SetProperty(ref _duplicateStrategy, value);
    }

    public IReadOnlyList<string> AvailableDuplicateStrategies { get; } = new[] { "Skip", "Overwrite", "Rename" };

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public ObservableCollection<FileMapping> PreviewMappings { get; }

    public ICommand PreviewCommand { get; }
    public ICommand OrganizeCommand { get; }

    public async Task LoadDefaultsAsync()
    {
        var config = await _configService.LoadConfigurationAsync();
        if (config is not null)
        {
            SelectedOrganizeMode = config.DefaultOrganizeMode;
            SelectedDuplicateStrategy = config.DefaultDuplicateStrategy;
        }
    }

    private OrganizeMode ParseMode() =>
        SelectedOrganizeMode == "Move" ? OrganizeMode.Move : OrganizeMode.Copy;

    private DuplicateStrategy ParseStrategy() => SelectedDuplicateStrategy switch
    {
        "Overwrite" => DuplicateStrategy.Overwrite,
        "Rename" => DuplicateStrategy.Rename,
        _ => DuplicateStrategy.Skip
    };

    private async Task OnPreview()
    {
        if (string.IsNullOrWhiteSpace(SourcePath) || string.IsNullOrWhiteSpace(DestinationPath))
        {
            StatusMessage = "Source and destination paths are required.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Generating preview...";

        try
        {
            var preview = await Task.Run(() =>
                _organizer.Preview(SourcePath, DestinationPath, ParseMode(), ParseStrategy()));

            _lastPreview = preview;
            _dispatchToUI(() =>
            {
                PreviewMappings.Clear();
                foreach (var mapping in preview.FileMappings)
                    PreviewMappings.Add(mapping);
            });

            StatusMessage = $"Preview: {preview.TotalFiles} files — {preview.WouldCopyOrMove} to process, {preview.WouldSkip} to skip";
            if (preview.Errors.Count > 0)
                StatusMessage += $", {preview.Errors.Count} errors";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Preview failed: {ex.Message}";
        }

        IsBusy = false;
    }

    private async Task OnOrganize()
    {
        if (string.IsNullOrWhiteSpace(SourcePath) || string.IsNullOrWhiteSpace(DestinationPath))
        {
            StatusMessage = "Source and destination paths are required.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Organizing files...";

        try
        {
            var result = await Task.Run(() =>
                _organizer.Organize(SourcePath, DestinationPath, ParseMode(), ParseStrategy()));

            _dispatchToUI(() => PreviewMappings.Clear());

            var parts = new List<string>();
            if (result.FilesCopied > 0) parts.Add($"{result.FilesCopied} copied");
            if (result.FilesMoved > 0) parts.Add($"{result.FilesMoved} moved");
            if (result.FilesSkipped > 0) parts.Add($"{result.FilesSkipped} skipped");
            if (result.Errors.Count > 0) parts.Add($"{result.Errors.Count} errors");

            StatusMessage = $"Done: {string.Join(", ", parts)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Organize failed: {ex.Message}";
        }

        IsBusy = false;
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
}
