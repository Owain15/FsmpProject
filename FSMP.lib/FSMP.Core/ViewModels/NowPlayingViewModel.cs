using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FSMP.Core.Interfaces;
using FSMP.Core.Interfaces.EventArgs;
using FSMP.Core.Models;

namespace FSMP.Core.ViewModels;

public class NowPlayingViewModel : INotifyPropertyChanged
{
    private readonly IPlaybackController _playbackController;
    private readonly IAudioService _audioService;
    private readonly ITagService _tagService;
    private readonly IConfigurationService _configService;
    private readonly Action<Action> _dispatchToUI;
    private readonly Func<Func<Task>, Task> _dispatchToUIAsync;

    private string _trackTitle = "No track loaded";
    private string _trackArtist = string.Empty;
    private string _trackAlbum = string.Empty;
    private PlaybackState _playbackState = PlaybackState.Stopped;
    private TimeSpan _position;
    private TimeSpan _duration;
    private float _volume;
    private string _repeatModeText = "🔁 Off";
    private bool _isShuffled;
    private bool _subscribed;
    private bool _isSeeking;
    private int? _currentTrackId;
    private string _tagFilter = string.Empty;
    private bool _showCreateNew;
    private bool _isTagListMode;
    private bool _allowUnsaveFromTagList;
    private bool _hasPendingChanges;
    private List<Tags> _trackTagsBacking = new();
    private List<Tags> _allTagsBacking = new();

    public NowPlayingViewModel(
        IPlaybackController playbackController,
        IAudioService audioService,
        ITagService tagService,
        IConfigurationService configService,
        Action<Action> dispatchToUI,
        Func<Func<Task>, Task> dispatchToUIAsync)
    {
        _playbackController = playbackController ?? throw new ArgumentNullException(nameof(playbackController));
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _dispatchToUI = dispatchToUI ?? throw new ArgumentNullException(nameof(dispatchToUI));
        _dispatchToUIAsync = dispatchToUIAsync ?? throw new ArgumentNullException(nameof(dispatchToUIAsync));

        QueueItems = new ObservableCollection<QueueItem>();
        TrackTags = new ObservableCollection<Tags>();
        AllTags = new ObservableCollection<Tags>();
        TagListItems = new ObservableCollection<TagListItem>();

        PlayPauseCommand = new AsyncRelayCommand(OnPlayPause);
        NextCommand = new AsyncRelayCommand(OnNext);
        PreviousCommand = new AsyncRelayCommand(OnPrevious);
        StopCommand = new AsyncRelayCommand(OnStop);
        ToggleRepeatCommand = new RelayCommand(OnToggleRepeat);
        ToggleShuffleCommand = new RelayCommand(OnToggleShuffle);
        JumpToCommand = new AsyncRelayCommand<QueueItem>(OnJumpTo);
        AddTagToTrackCommand = new AsyncRelayCommand<Tags>(OnAddTagToTrack);
        RemoveTagFromTrackCommand = new AsyncRelayCommand<Tags>(OnRemoveTagFromTrack);
        CreateAndAddTagCommand = new AsyncRelayCommand(OnCreateAndAddTag);
        ToggleTagViewCommand = new RelayCommand(OnToggleTagView);
        TogglePendingCommand = new RelayCommand<TagListItem>(OnTogglePending);
        QuickAddTagCommand = new AsyncRelayCommand<TagListItem>(OnQuickAddTag);
        QuickRemoveTagCommand = new AsyncRelayCommand<TagListItem>(OnQuickRemoveTag);
        ApplyPendingTagsCommand = new AsyncRelayCommand(OnApplyPendingTags);
    }

    public string TrackTitle
    {
        get => _trackTitle;
        private set => SetProperty(ref _trackTitle, value);
    }

    public string TrackArtist
    {
        get => _trackArtist;
        private set => SetProperty(ref _trackArtist, value);
    }

    public string TrackAlbum
    {
        get => _trackAlbum;
        private set => SetProperty(ref _trackAlbum, value);
    }

    public PlaybackState PlaybackState
    {
        get => _playbackState;
        private set => SetProperty(ref _playbackState, value);
    }

    public TimeSpan Position
    {
        get => _position;
        private set
        {
            if (SetProperty(ref _position, value))
            {
                OnPropertyChanged(nameof(PositionText));
                OnPropertyChanged(nameof(Progress));
            }
        }
    }

    public TimeSpan Duration
    {
        get => _duration;
        private set
        {
            if (SetProperty(ref _duration, value))
            {
                OnPropertyChanged(nameof(DurationText));
                OnPropertyChanged(nameof(Progress));
            }
        }
    }

    private double _progress;
    public double Progress
    {
        get => Duration.TotalSeconds > 0
            ? Position.TotalSeconds / Duration.TotalSeconds
            : 0;
        set
        {
            // Only act on user-initiated changes (from slider drag)
            if (_isSeeking && Duration.TotalSeconds > 0)
            {
                var target = TimeSpan.FromSeconds(value * Duration.TotalSeconds);
                _ = _audioService.Player.SeekAsync(target);
            }
        }
    }

    public bool IsSeeking
    {
        get => _isSeeking;
        set => SetProperty(ref _isSeeking, value);
    }

    public string PositionText => FormatTime(Position);
    public string DurationText => FormatTime(Duration);

    public float Volume
    {
        get => _volume;
        set
        {
            if (SetProperty(ref _volume, value))
                _audioService.Volume = value;
        }
    }

    public string RepeatModeText
    {
        get => _repeatModeText;
        private set => SetProperty(ref _repeatModeText, value);
    }

    public bool IsShuffled
    {
        get => _isShuffled;
        private set => SetProperty(ref _isShuffled, value);
    }

    public ObservableCollection<QueueItem> QueueItems { get; }
    public ObservableCollection<Tags> TrackTags { get; }
    public ObservableCollection<Tags> AllTags { get; }
    public ObservableCollection<TagListItem> TagListItems { get; }

    public bool IsTagListMode
    {
        get => _isTagListMode;
        private set => SetProperty(ref _isTagListMode, value);
    }

    public bool AllowUnsaveFromTagList
    {
        get => _allowUnsaveFromTagList;
        private set => SetProperty(ref _allowUnsaveFromTagList, value);
    }

    public bool HasPendingChanges
    {
        get => _hasPendingChanges;
        private set => SetProperty(ref _hasPendingChanges, value);
    }

    public string TagFilter
    {
        get => _tagFilter;
        set
        {
            if (SetProperty(ref _tagFilter, value))
                ApplyTagFilter();
        }
    }

    public bool ShowCreateNew
    {
        get => _showCreateNew;
        private set => SetProperty(ref _showCreateNew, value);
    }

    public ICommand PlayPauseCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand PreviousCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ToggleRepeatCommand { get; }
    public ICommand ToggleShuffleCommand { get; }
    public ICommand JumpToCommand { get; }
    public ICommand AddTagToTrackCommand { get; }
    public ICommand RemoveTagFromTrackCommand { get; }
    public ICommand CreateAndAddTagCommand { get; }
    public ICommand ToggleTagViewCommand { get; }
    public ICommand TogglePendingCommand { get; }
    public ICommand QuickAddTagCommand { get; }
    public ICommand QuickRemoveTagCommand { get; }
    public ICommand ApplyPendingTagsCommand { get; }

    public async Task LoadAsync()
    {
        if (!_subscribed)
        {
            _subscribed = true;
            SubscribeToEvents();
        }

        var trackResult = await _playbackController.GetCurrentTrackAsync();
        if (trackResult.IsSuccess && trackResult.Value is not null)
        {
            var track = trackResult.Value;
            _currentTrackId = track.TrackId;
            TrackTitle = track.Title ?? "Unknown Title";
            TrackArtist = track.Artist?.Name ?? "Unknown Artist";
            TrackAlbum = track.Album?.Title ?? "Unknown Album";
        }
        else
        {
            _currentTrackId = null;
            TrackTitle = "No track loaded";
            TrackArtist = string.Empty;
            TrackAlbum = string.Empty;
        }

        PlaybackState = _audioService.Player.State;
        Position = _audioService.Player.Position;
        Duration = _audioService.Player.Duration;
        Volume = _audioService.Volume;
        UpdateRepeatModeText();
        IsShuffled = _playbackController.IsShuffled;

        try
        {
            var config = await _configService.LoadConfigurationAsync();
            AllowUnsaveFromTagList = config.AllowUnsaveFromTagList;
        }
        catch { /* non-fatal */ }

        await RefreshQueueAsync();
        await RefreshTagsAsync();
    }

    private void SubscribeToEvents()
    {
        _audioService.Player.StateChanged += OnStateChanged;
        _audioService.Player.PositionChanged += OnPositionChanged;
        _audioService.TrackChanged += OnTrackChanged;
        _playbackController.SubscribeToTrackEnd(() =>
            _dispatchToUIAsync(async () => await _playbackController.AutoAdvanceAsync()));
    }

    private void OnStateChanged(object? sender, PlaybackStateChangedEventArgs e)
    {
        _dispatchToUI(() => PlaybackState = e.NewState);
    }

    private void OnPositionChanged(object? sender, PositionChangedEventArgs e)
    {
        if (_isSeeking) return;
        _dispatchToUI(() =>
        {
            Position = e.Position;
            Duration = e.Duration;
        });
    }

    private async void OnTrackChanged(object? sender, TrackChangedEventArgs e)
    {
        await _dispatchToUIAsync(async () =>
        {
            if (e.NewTrack is not null)
            {
                _currentTrackId = e.NewTrack.TrackId;
                TrackTitle = e.NewTrack.Title ?? "Unknown Title";
                TrackArtist = e.NewTrack.Artist?.Name ?? "Unknown Artist";
                TrackAlbum = e.NewTrack.Album?.Title ?? "Unknown Album";
            }
            else
            {
                _currentTrackId = null;
                TrackTitle = "No track loaded";
                TrackArtist = string.Empty;
                TrackAlbum = string.Empty;
            }

            await RefreshQueueAsync();
            await RefreshTagsAsync();
        });
    }

    private async Task RefreshQueueAsync()
    {
        var queueResult = await _playbackController.GetQueueItemsAsync(truncate: false);
        QueueItems.Clear();
        if (queueResult.IsSuccess)
        {
            foreach (var item in queueResult.Value!)
                QueueItems.Add(item);
        }
    }

    private async Task OnJumpTo(QueueItem? item)
    {
        if (item is not null)
            await _playbackController.JumpToAsync(item.Index);
    }

    private async Task OnPlayPause() => await _playbackController.TogglePauseAsync();
    private async Task OnNext() => await _playbackController.NextTrackAsync();
    private async Task OnPrevious() => await _playbackController.PreviousTrackAsync();
    private async Task OnStop() => await _playbackController.StopAsync();

    private void OnToggleRepeat()
    {
        _playbackController.ToggleRepeatMode();
        UpdateRepeatModeText();
    }

    private void OnToggleShuffle()
    {
        _playbackController.ToggleShuffle();
        IsShuffled = _playbackController.IsShuffled;
    }

    private void UpdateRepeatModeText()
    {
        RepeatModeText = _playbackController.RepeatMode switch
        {
            RepeatMode.None => "🔁 Off",
            RepeatMode.One => "🔂 One",
            RepeatMode.All => "🔁 All",
            _ => "🔁 Off"
        };
    }

    public async Task RefreshTagsAsync()
    {
        if (_currentTrackId is null)
        {
            _trackTagsBacking.Clear();
            _allTagsBacking.Clear();
            ApplyTagFilter();
            return;
        }

        var trackTagsResult = await _tagService.GetTagsForTrackAsync(_currentTrackId.Value);
        _trackTagsBacking = trackTagsResult.IsSuccess
            ? new List<Tags>(trackTagsResult.Value!)
            : new List<Tags>();

        var allTagsResult = await _tagService.GetAllTagsAsync();
        if (allTagsResult.IsSuccess)
        {
            var assignedIds = new HashSet<int>(_trackTagsBacking.Select(t => t.TagId));
            _allTagsBacking = allTagsResult.Value!
                .Where(t => !assignedIds.Contains(t.TagId))
                .ToList();
        }
        else
        {
            _allTagsBacking = new List<Tags>();
        }

        ApplyTagFilter();
    }

    private void ApplyTagFilter()
    {
        TrackTags.Clear();
        AllTags.Clear();

        var filter = _tagFilter?.Trim() ?? string.Empty;

        foreach (var tag in _trackTagsBacking)
        {
            if (filter.Length == 0 || tag.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                TrackTags.Add(tag);
        }

        foreach (var tag in _allTagsBacking)
        {
            if (filter.Length == 0 || tag.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                AllTags.Add(tag);
        }

        ShowCreateNew = filter.Length > 0 &&
            !_trackTagsBacking.Any(t => t.Name.Equals(filter, StringComparison.OrdinalIgnoreCase)) &&
            !_allTagsBacking.Any(t => t.Name.Equals(filter, StringComparison.OrdinalIgnoreCase));

        if (_isTagListMode)
            RebuildTagListItems();
    }

    private async Task OnAddTagToTrack(Tags? tag)
    {
        if (tag is null || _currentTrackId is null) return;
        await _tagService.AddTagToTrackAsync(_currentTrackId.Value, tag.TagId);
        await RefreshTagsAsync();
    }

    private async Task OnRemoveTagFromTrack(Tags? tag)
    {
        if (tag is null || _currentTrackId is null) return;
        await _tagService.RemoveTagFromTrackAsync(_currentTrackId.Value, tag.TagId);
        await RefreshTagsAsync();
    }

    private async Task OnCreateAndAddTag()
    {
        if (string.IsNullOrWhiteSpace(TagFilter) || _currentTrackId is null) return;
        var createResult = await _tagService.CreateTagAsync(TagFilter.Trim());
        if (createResult.IsSuccess)
        {
            await _tagService.AddTagToTrackAsync(_currentTrackId.Value, createResult.Value!.TagId);
            TagFilter = string.Empty;
            await RefreshTagsAsync();
        }
    }

    private void OnToggleTagView()
    {
        IsTagListMode = !IsTagListMode;
        if (IsTagListMode)
            RebuildTagListItems();
    }

    private void OnTogglePending(TagListItem? item)
    {
        if (item is null) return;
        if (item.IsSaved && !AllowUnsaveFromTagList) return;
        item.IsPendingChange = !item.IsPendingChange;
    }

    private async Task OnQuickAddTag(TagListItem? item)
    {
        if (item is null || _currentTrackId is null || item.IsSaved) return;
        await _tagService.AddTagToTrackAsync(_currentTrackId.Value, item.Tag.TagId);
        await RefreshTagsAsync();
    }

    private async Task OnQuickRemoveTag(TagListItem? item)
    {
        if (item is null || _currentTrackId is null || !item.IsSaved) return;
        await _tagService.RemoveTagFromTrackAsync(_currentTrackId.Value, item.Tag.TagId);
        await RefreshTagsAsync();
    }

    private async Task OnApplyPendingTags()
    {
        if (_currentTrackId is null) return;

        foreach (var item in TagListItems.Where(i => i.IsPendingChange).ToList())
        {
            if (item.IsSaved)
                await _tagService.RemoveTagFromTrackAsync(_currentTrackId.Value, item.Tag.TagId);
            else
                await _tagService.AddTagToTrackAsync(_currentTrackId.Value, item.Tag.TagId);
        }

        await RefreshTagsAsync();
    }

    private void RebuildTagListItems()
    {
        foreach (var old in TagListItems)
            old.PropertyChanged -= OnTagListItemPropertyChanged;

        TagListItems.Clear();
        var filter = _tagFilter?.Trim() ?? string.Empty;

        foreach (var tag in _trackTagsBacking)
        {
            if (filter.Length == 0 || tag.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                TagListItems.Add(new TagListItem { Tag = tag, IsSaved = true });
        }

        foreach (var tag in _allTagsBacking)
        {
            if (filter.Length == 0 || tag.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                TagListItems.Add(new TagListItem { Tag = tag, IsSaved = false });
        }

        foreach (var item in TagListItems)
            item.PropertyChanged += OnTagListItemPropertyChanged;

        UpdateHasPendingChanges();
    }

    private void OnTagListItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TagListItem.IsPendingChange))
            UpdateHasPendingChanges();
    }

    private void UpdateHasPendingChanges()
    {
        HasPendingChanges = TagListItems.Any(i => i.IsPendingChange);
    }

    public void UnsubscribeFromEvents()
    {
        if (!_subscribed) return;
        _subscribed = false;
        _audioService.Player.StateChanged -= OnStateChanged;
        _audioService.Player.PositionChanged -= OnPositionChanged;
        _audioService.TrackChanged -= OnTrackChanged;
    }

    private static string FormatTime(TimeSpan time) =>
        time.Hours > 0
            ? time.ToString(@"h\:mm\:ss")
            : time.ToString(@"m\:ss");

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
}
