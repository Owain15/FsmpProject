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
    private readonly Action<Action> _dispatchToUI;
    private readonly Func<Func<Task>, Task> _dispatchToUIAsync;

    private string _trackTitle = "No track loaded";
    private string _trackArtist = string.Empty;
    private string _trackAlbum = string.Empty;
    private PlaybackState _playbackState = PlaybackState.Stopped;
    private TimeSpan _position;
    private TimeSpan _duration;
    private float _volume;
    private string _repeatModeText = "Repeat: Off";
    private bool _isShuffled;

    public NowPlayingViewModel(
        IPlaybackController playbackController,
        IAudioService audioService,
        Action<Action> dispatchToUI,
        Func<Func<Task>, Task> dispatchToUIAsync)
    {
        _playbackController = playbackController ?? throw new ArgumentNullException(nameof(playbackController));
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _dispatchToUI = dispatchToUI ?? throw new ArgumentNullException(nameof(dispatchToUI));
        _dispatchToUIAsync = dispatchToUIAsync ?? throw new ArgumentNullException(nameof(dispatchToUIAsync));

        _volume = _audioService.Volume;
        QueueItems = new ObservableCollection<QueueItem>();

        PlayPauseCommand = new AsyncRelayCommand(OnPlayPause);
        NextCommand = new AsyncRelayCommand(OnNext);
        PreviousCommand = new AsyncRelayCommand(OnPrevious);
        StopCommand = new AsyncRelayCommand(OnStop);
        ToggleRepeatCommand = new RelayCommand(OnToggleRepeat);
        ToggleShuffleCommand = new RelayCommand(OnToggleShuffle);

        SubscribeToEvents();
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

    public double Progress => Duration.TotalSeconds > 0
        ? Position.TotalSeconds / Duration.TotalSeconds
        : 0;

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

    public ICommand PlayPauseCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand PreviousCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ToggleRepeatCommand { get; }
    public ICommand ToggleShuffleCommand { get; }

    public async Task LoadAsync()
    {
        var trackResult = await _playbackController.GetCurrentTrackAsync();
        if (trackResult.IsSuccess && trackResult.Value is not null)
        {
            var track = trackResult.Value;
            TrackTitle = track.Title ?? "Unknown Title";
            TrackArtist = track.Artist?.Name ?? "Unknown Artist";
            TrackAlbum = track.Album?.Title ?? "Unknown Album";
        }
        else
        {
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

        await RefreshQueueAsync();
    }

    private void SubscribeToEvents()
    {
        _audioService.Player.StateChanged += OnStateChanged;
        _audioService.Player.PositionChanged += OnPositionChanged;
        _audioService.TrackChanged += OnTrackChanged;
    }

    private void OnStateChanged(object? sender, PlaybackStateChangedEventArgs e)
    {
        _dispatchToUI(() => PlaybackState = e.NewState);
    }

    private void OnPositionChanged(object? sender, PositionChangedEventArgs e)
    {
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
                TrackTitle = e.NewTrack.Title ?? "Unknown Title";
                TrackArtist = e.NewTrack.Artist?.Name ?? "Unknown Artist";
                TrackAlbum = e.NewTrack.Album?.Title ?? "Unknown Album";
            }
            else
            {
                TrackTitle = "No track loaded";
                TrackArtist = string.Empty;
                TrackAlbum = string.Empty;
            }

            await RefreshQueueAsync();
        });
    }

    private async Task RefreshQueueAsync()
    {
        var queueResult = await _playbackController.GetQueueItemsAsync();
        QueueItems.Clear();
        if (queueResult.IsSuccess)
        {
            foreach (var item in queueResult.Value!)
                QueueItems.Add(item);
        }
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
            RepeatMode.None => "Repeat: Off",
            RepeatMode.One => "Repeat: One",
            RepeatMode.All => "Repeat: All",
            _ => "Repeat: Off"
        };
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

    private sealed class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
    }
}
