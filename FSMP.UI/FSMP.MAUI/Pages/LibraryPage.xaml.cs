using FSMP.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FSMP.MAUI.Pages;

public partial class LibraryPage : ContentPage
{
    private LibraryBrowseViewModel _viewModel = null!;
    private IServiceScope? _scope;

    public LibraryPage()
    {
        try
        {
            App.Log("LibraryPage constructor starting");
            InitializeComponent();
            CreateScopeAndViewModel();
            App.Log("LibraryPage constructor done");
        }
        catch (Exception ex)
        {
            App.Log($"CRASH in LibraryPage constructor: {ex}");
            throw;
        }
    }

    private void CreateScopeAndViewModel()
    {
        _scope?.Dispose();
        _scope = App.Services.CreateScope();
        _viewModel = _scope.ServiceProvider.GetRequiredService<LibraryBrowseViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (App.IsInitialized)
        {
            LoadingOverlay.IsVisible = false;
            await LoadDataAsync();
            return;
        }
        StatusLabel.Text = App.InitStatusMessage;
        App.InitializationStatusChanged += OnStatusChanged;
        App.InitializationComplete += OnInitComplete;
    }

    private async Task LoadDataAsync()
    {
        try
        {
            App.Log("LibraryPage.LoadDataAsync calling LoadAsync");
            CreateScopeAndViewModel();
            await _viewModel.LoadAsync();
            App.Log($"LibraryPage.LoadDataAsync done, Items.Count={_viewModel.Items.Count}");
        }
        catch (Exception ex)
        {
            App.Log($"LibraryPage.OnAppearing error: {ex}");
        }
    }

    private void OnStatusChanged()
        => MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = App.InitStatusMessage);

    private async void OnInitComplete()
    {
        App.InitializationStatusChanged -= OnStatusChanged;
        App.InitializationComplete -= OnInitComplete;
        MainThread.BeginInvokeOnMainThread(() => LoadingOverlay.IsVisible = false);
        await LoadDataAsync();
    }

    protected override void OnDisappearing()
    {
        App.InitializationStatusChanged -= OnStatusChanged;
        App.InitializationComplete -= OnInitComplete;
        base.OnDisappearing();
    }
}
