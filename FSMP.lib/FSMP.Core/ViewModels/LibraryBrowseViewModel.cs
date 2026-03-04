using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;

namespace FSMP.Core.ViewModels;

public enum BrowseLevel
{
    Artists,
    Albums,
    Tracks
}

public class LibraryBrowseViewModel : INotifyPropertyChanged
{
    private readonly ILibraryBrowser _libraryBrowser;
    private readonly IPlaybackController _playbackController;
    private readonly Action<Action> _dispatchToUI;

    private BrowseLevel _browseLevel = BrowseLevel.Artists;
    private string _pageTitle = "Artists";
    private int? _currentArtistId;
    private int? _currentAlbumId;

    public LibraryBrowseViewModel(ILibraryBrowser libraryBrowser, IPlaybackController playbackController, Action<Action> dispatchToUI)
    {
        _libraryBrowser = libraryBrowser ?? throw new ArgumentNullException(nameof(libraryBrowser));
        _playbackController = playbackController ?? throw new ArgumentNullException(nameof(playbackController));
        _dispatchToUI = dispatchToUI ?? throw new ArgumentNullException(nameof(dispatchToUI));

        Items = new ObservableCollection<object>();
        SelectItemCommand = new AsyncRelayCommand<object>(OnSelectItem);
        GoBackCommand = new AsyncRelayCommand(OnGoBack);
        PlayNowCommand = new AsyncRelayCommand<Track>(OnPlayNow);
        AddToQueueCommand = new RelayCommand<Track>(OnAddToQueue);
    }

    public ObservableCollection<object> Items { get; }

    public BrowseLevel BrowseLevel
    {
        get => _browseLevel;
        private set => SetProperty(ref _browseLevel, value);
    }

    public string PageTitle
    {
        get => _pageTitle;
        private set => SetProperty(ref _pageTitle, value);
    }

    public bool CanGoBack => BrowseLevel != BrowseLevel.Artists;

    public ICommand SelectItemCommand { get; }
    public ICommand GoBackCommand { get; }
    public ICommand PlayNowCommand { get; }
    public ICommand AddToQueueCommand { get; }

    public async Task LoadAsync()
    {
        BrowseLevel = BrowseLevel.Artists;
        PageTitle = "Artists";
        _currentArtistId = null;
        _currentAlbumId = null;

        var result = await _libraryBrowser.GetAllArtistsAsync();
        _dispatchToUI(() =>
        {
            Items.Clear();
            if (result.IsSuccess)
            {
                foreach (var artist in result.Value!)
                    Items.Add(artist);
            }
            OnPropertyChanged(nameof(CanGoBack));
        });
    }

    private async Task OnSelectItem(object? item)
    {
        switch (item)
        {
            case Artist artist:
                await LoadAlbumsForArtist(artist);
                break;
            case Album album:
                await LoadTracksForAlbum(album);
                break;
            case Track track:
                await OnPlayNow(track);
                break;
        }
    }

    private async Task LoadAlbumsForArtist(Artist artist)
    {
        _currentArtistId = artist.ArtistId;
        BrowseLevel = BrowseLevel.Albums;
        PageTitle = artist.Name;

        var result = await _libraryBrowser.GetAlbumsByArtistAsync(artist.ArtistId);
        _dispatchToUI(() =>
        {
            Items.Clear();
            if (result.IsSuccess)
            {
                foreach (var album in result.Value!)
                    Items.Add(album);
            }
            OnPropertyChanged(nameof(CanGoBack));
        });
    }

    private async Task LoadTracksForAlbum(Album album)
    {
        _currentAlbumId = album.AlbumId;
        BrowseLevel = BrowseLevel.Tracks;
        PageTitle = album.Title;

        var result = await _libraryBrowser.GetAlbumWithTracksAsync(album.AlbumId);
        _dispatchToUI(() =>
        {
            Items.Clear();
            if (result.IsSuccess && result.Value is not null)
            {
                foreach (var track in result.Value.Tracks)
                    Items.Add(track);
            }
            OnPropertyChanged(nameof(CanGoBack));
        });
    }

    private async Task OnPlayNow(Track? track)
    {
        if (track is null) return;

        // Get all track IDs from the current album
        if (_currentAlbumId.HasValue)
        {
            var albumResult = await _libraryBrowser.GetAlbumWithTracksAsync(_currentAlbumId.Value);
            if (albumResult.IsSuccess && albumResult.Value is not null)
            {
                var trackIds = albumResult.Value.Tracks.Select(t => t.TrackId).ToList();
                var trackIndex = trackIds.IndexOf(track.TrackId);
                _playbackController.SetQueue(trackIds);
                if (trackIndex >= 0)
                    await _playbackController.JumpToAsync(trackIndex);
            }
        }
        else
        {
            _playbackController.SetQueue(new[] { track.TrackId });
            await _playbackController.JumpToAsync(0);
        }
    }

    private void OnAddToQueue(Track? track)
    {
        if (track is not null)
            _playbackController.AppendToQueue(new List<int> { track.TrackId });
    }

    private async Task OnGoBack()
    {
        switch (BrowseLevel)
        {
            case BrowseLevel.Tracks when _currentArtistId.HasValue:
                var artistResult = await _libraryBrowser.GetArtistByIdAsync(_currentArtistId.Value);
                if (artistResult.IsSuccess && artistResult.Value is not null)
                    await LoadAlbumsForArtist(artistResult.Value);
                break;
            case BrowseLevel.Albums:
                await LoadAsync();
                break;
        }
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

    private sealed class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        public RelayCommand(Action<T?> execute) => _execute = execute;
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute(parameter is T t ? t : default);
    }
}
