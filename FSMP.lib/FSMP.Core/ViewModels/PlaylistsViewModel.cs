using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;

namespace FSMP.Core.ViewModels;

public class PlaylistsViewModel : INotifyPropertyChanged
{
    private readonly IPlaylistManager _playlistManager;

    private string _statusMessage = string.Empty;

    public PlaylistsViewModel(IPlaylistManager playlistManager)
    {
        _playlistManager = playlistManager ?? throw new ArgumentNullException(nameof(playlistManager));

        Playlists = new ObservableCollection<Playlist>();
        CreatePlaylistCommand = new AsyncRelayCommand<string>(OnCreatePlaylist);
        DeletePlaylistCommand = new AsyncRelayCommand<Playlist>(OnDeletePlaylist);
        LoadIntoQueueCommand = new AsyncRelayCommand<Playlist>(OnLoadIntoQueue);
    }

    public ObservableCollection<Playlist> Playlists { get; }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public ICommand CreatePlaylistCommand { get; }
    public ICommand DeletePlaylistCommand { get; }
    public ICommand LoadIntoQueueCommand { get; }

    public async Task LoadAsync()
    {
        var result = await _playlistManager.GetAllPlaylistsAsync();
        Playlists.Clear();
        if (result.IsSuccess && result.Value is not null)
        {
            foreach (var playlist in result.Value)
                Playlists.Add(playlist);
        }
    }

    private async Task OnCreatePlaylist(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        var result = await _playlistManager.CreatePlaylistAsync(name);
        if (result.IsSuccess)
        {
            StatusMessage = $"Created playlist '{name}'.";
            await LoadAsync();
        }
        else
        {
            StatusMessage = $"Failed to create playlist: {result.ErrorMessage}";
        }
    }

    private async Task OnDeletePlaylist(Playlist? playlist)
    {
        if (playlist is null) return;

        var result = await _playlistManager.DeletePlaylistAsync(playlist.PlaylistId);
        if (result.IsSuccess)
        {
            StatusMessage = $"Deleted playlist '{playlist.Name}'.";
            await LoadAsync();
        }
    }

    private async Task OnLoadIntoQueue(Playlist? playlist)
    {
        if (playlist is null) return;

        var result = await _playlistManager.LoadPlaylistIntoQueueAsync(playlist.PlaylistId);
        StatusMessage = result.IsSuccess
            ? $"Loaded '{playlist.Name}' into queue."
            : $"Failed to load playlist: {result.ErrorMessage}";
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
