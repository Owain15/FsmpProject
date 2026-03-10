using FSMP.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FSMP.MAUI.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;
    private readonly IServiceScope _scope;

    public SettingsPage()
    {
        _scope = App.Services.CreateScope();
        _viewModel = _scope.ServiceProvider.GetRequiredService<SettingsViewModel>();
        InitializeComponent();
        BindingContext = _viewModel;
        Unloaded += (_, _) => _scope.Dispose();
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
            await _viewModel.LoadAsync();
        }
        catch (Exception ex)
        {
            App.Log($"SettingsPage.OnAppearing error: {ex}");
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
