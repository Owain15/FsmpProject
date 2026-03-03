using FSMP.Core.ViewModels;

namespace FSMP.MAUI.Pages;

public partial class LibraryPage : ContentPage
{
    private readonly LibraryBrowseViewModel _viewModel;

    public LibraryPage(LibraryBrowseViewModel viewModel)
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
