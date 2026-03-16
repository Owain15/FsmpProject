using FSMP.Core.Models;
using FSMP.MAUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FSMP.MAUI.Pages;

public partial class NowPlayingPage : ContentPage
{
    private NowPlayingViewModel _viewModel = null!;
    private IServiceScope? _scope;
    private bool _isQueueVisible;
    private bool _isWideLayout;
    private const double WideBreakpoint = 800;

    public NowPlayingPage()
    {
        InitializeComponent();
        CreateScopeAndViewModel();
        SizeChanged += OnPageSizeChanged;
    }

    private void CreateScopeAndViewModel()
    {
        _viewModel?.UnsubscribeFromEvents();
        _scope?.Dispose();
        _scope = App.Services.CreateScope();
        _viewModel = _scope.ServiceProvider.GetRequiredService<NowPlayingViewModel>();
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
            CreateScopeAndViewModel();
            await _viewModel.LoadAsync();
        }
        catch (Exception ex)
        {
            App.Log($"NowPlayingPage.OnAppearing error: {ex}");
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

    private void OnSeekStarted(object? sender, EventArgs e)
    {
        _viewModel.IsSeeking = true;
    }

    private void OnSeekCompleted(object? sender, EventArgs e)
    {
        _viewModel.Progress = ProgressSlider.Value;
        _viewModel.IsSeeking = false;
    }

    private void OnQueueSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is QueueItem item)
        {
            _viewModel.JumpToCommand.Execute(item);
            QueueCollectionView.SelectedItem = null;
        }
    }

    private void OnToggleQueue(object? sender, EventArgs e)
    {
        _isQueueVisible = !_isQueueVisible;
        ApplyQueueLayout();
    }

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        var wasWide = _isWideLayout;
        _isWideLayout = Width >= WideBreakpoint;

        if (wasWide != _isWideLayout)
            ApplyQueueLayout();
    }

    private void ApplyQueueLayout()
    {
        QueueSidebar.IsVisible = _isQueueVisible;

        if (!_isQueueVisible)
        {
            // Reset overlay mode
            Grid.SetColumn(QueueSidebar, 1);
            Grid.SetColumnSpan(QueueSidebar, 1);
            QueueSidebar.ZIndex = 0;
            return;
        }

        if (_isWideLayout)
        {
            // Side-by-side: sidebar in column 1
            Grid.SetColumn(QueueSidebar, 1);
            Grid.SetColumnSpan(QueueSidebar, 1);
            QueueSidebar.ZIndex = 0;
        }
        else
        {
            // Overlay: sidebar spans both columns, on top
            Grid.SetColumn(QueueSidebar, 0);
            Grid.SetColumnSpan(QueueSidebar, 2);
            QueueSidebar.ZIndex = 50;
            QueueSidebar.WidthRequest = -1; // fill available space
        }
    }
}
