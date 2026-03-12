using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;

namespace FSMP.Core.ViewModels;

public class PlaylistDetailViewModel : INotifyPropertyChanged
{
    private readonly IPlaylistManager _playlistManager;
    private readonly Action<Action> _dispatchToUI;

    private int _playlistId;
    private string _playlistName = string.Empty;
    private string _statusMessage = string.Empty;

    public PlaylistDetailViewModel(IPlaylistManager playlistManager, Action<Action> dispatchToUI)
    {
        _playlistManager = playlistManager ?? throw new ArgumentNullException(nameof(playlistManager));
        _dispatchToUI = dispatchToUI ?? throw new ArgumentNullException(nameof(dispatchToUI));

        Tracks = new ObservableCollection<PlaylistTrack>();
        RenameCommand = new AsyncRelayCommand<string>(OnRename);
        RemoveTrackCommand = new AsyncRelayCommand<PlaylistTrack>(OnRemoveTrack);
        AddTrackCommand = new AsyncRelayCommand<Track>(OnAddTrack);
    }

    public ObservableCollection<PlaylistTrack> Tracks { get; }

    public string PlaylistName
    {
        get => _playlistName;
        set => SetProperty(ref _playlistName, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public ICommand RenameCommand { get; }
    public ICommand RemoveTrackCommand { get; }
    public ICommand AddTrackCommand { get; }

    public async Task LoadAsync(int playlistId)
    {
        _playlistId = playlistId;
        var result = await _playlistManager.GetPlaylistWithTracksAsync(playlistId);
        _dispatchToUI(() =>
        {
            Tracks.Clear();
            if (result.IsSuccess && result.Value is not null)
            {
                PlaylistName = result.Value.Name;
                foreach (var pt in result.Value.PlaylistTracks.OrderBy(t => t.Position))
                    Tracks.Add(pt);
            }
            else
            {
                StatusMessage = result.ErrorMessage ?? "Playlist not found.";
            }
        });
    }

    private async Task OnRename(string? newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return;

        var result = await _playlistManager.RenamePlaylistAsync(_playlistId, newName);
        if (result.IsSuccess)
        {
            PlaylistName = newName;
            StatusMessage = $"Renamed to '{newName}'.";
        }
        else
        {
            StatusMessage = $"Rename failed: {result.ErrorMessage}";
        }
    }

    private async Task OnRemoveTrack(PlaylistTrack? pt)
    {
        if (pt is null) return;

        var result = await _playlistManager.RemoveTrackFromPlaylistAsync(_playlistId, pt.Position);
        if (result.IsSuccess)
        {
            StatusMessage = "Track removed.";
            await LoadAsync(_playlistId);
        }
        else
        {
            StatusMessage = $"Remove failed: {result.ErrorMessage}";
        }
    }

    private async Task OnAddTrack(Track? track)
    {
        if (track is null) return;

        var result = await _playlistManager.AddTrackToPlaylistAsync(_playlistId, track.TrackId);
        if (result.IsSuccess)
        {
            StatusMessage = $"Added '{track.Title}'.";
            await LoadAsync(_playlistId);
        }
        else
        {
            StatusMessage = $"Add failed: {result.ErrorMessage}";
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

    private sealed class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T?, Task> _execute;
        public AsyncRelayCommand(Func<T?, Task> execute) => _execute = execute;
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public async void Execute(object? parameter) => await _execute(parameter is T t ? t : default);
    }
}
