using FSMP.MAUI.ViewModels;

namespace FSMP.MAUI.Pages;

public partial class NowPlayingPage : ContentPage
{
    private readonly NowPlayingViewModel _viewModel;

    public NowPlayingPage(NowPlayingViewModel viewModel)
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
