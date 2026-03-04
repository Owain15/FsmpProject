using FSMP.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FSMP.MAUI.Pages;

public partial class PlaylistsPage : ContentPage
{
    private readonly PlaylistsViewModel _viewModel;
    private readonly IServiceScope _scope;

    public PlaylistsPage()
    {
        _scope = App.Services.CreateScope();
        _viewModel = _scope.ServiceProvider.GetRequiredService<PlaylistsViewModel>();
        InitializeComponent();
        BindingContext = _viewModel;
        Unloaded += (_, _) => _scope.Dispose();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _viewModel.LoadAsync();
        }
        catch (Exception ex)
        {
            App.Log($"PlaylistsPage.OnAppearing error: {ex}");
        }
    }
}
