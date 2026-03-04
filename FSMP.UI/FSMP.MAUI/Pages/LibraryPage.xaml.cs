using FSMP.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FSMP.MAUI.Pages;

public partial class LibraryPage : ContentPage
{
    private readonly LibraryBrowseViewModel _viewModel;
    private readonly IServiceScope _scope;

    public LibraryPage()
    {
        try
        {
            App.Log("LibraryPage constructor starting");
            _scope = App.Services.CreateScope();
            _viewModel = _scope.ServiceProvider.GetRequiredService<LibraryBrowseViewModel>();
            InitializeComponent();
            BindingContext = _viewModel;
            Unloaded += (_, _) => _scope.Dispose();
            App.Log("LibraryPage constructor done");
        }
        catch (Exception ex)
        {
            App.Log($"CRASH in LibraryPage constructor: {ex}");
            throw;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            App.Log("LibraryPage.OnAppearing calling LoadAsync");
            await _viewModel.LoadAsync();
            App.Log($"LibraryPage.OnAppearing done, Items.Count={_viewModel.Items.Count}");
        }
        catch (Exception ex)
        {
            App.Log($"LibraryPage.OnAppearing error: {ex}");
        }
    }
}
