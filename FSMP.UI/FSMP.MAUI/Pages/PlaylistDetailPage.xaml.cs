using FSMP.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FSMP.MAUI.Pages;

[QueryProperty(nameof(PlaylistId), "playlistId")]
public partial class PlaylistDetailPage : ContentPage
{
    private readonly PlaylistDetailViewModel _viewModel;
    private readonly IServiceScope _scope;

    public string PlaylistId { get; set; } = string.Empty;

    public PlaylistDetailPage()
    {
        _scope = App.Services.CreateScope();
        _viewModel = _scope.ServiceProvider.GetRequiredService<PlaylistDetailViewModel>();
        InitializeComponent();
        BindingContext = _viewModel;
        Unloaded += (_, _) => _scope.Dispose();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (int.TryParse(PlaylistId, out var id))
        {
            await _viewModel.LoadAsync(id);
        }
    }
}
