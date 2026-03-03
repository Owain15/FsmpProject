using FSMP.Core.ViewModels;

namespace FSMP.MAUI.Pages;

public partial class PlaylistsPage : ContentPage
{
    private readonly PlaylistsViewModel _viewModel;

    public PlaylistsPage(PlaylistsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
