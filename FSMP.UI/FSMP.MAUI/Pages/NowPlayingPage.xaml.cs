using FSMP.Core.Models;
using FSMP.Core;
using FSMP.MAUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FSMP.MAUI.Pages;

public partial class NowPlayingPage : ContentPage
{
    private NowPlayingViewModel _viewModel = null!;
    private IServiceScope? _scope;
    private bool _isQueueVisible;
    private bool _isTagsVisible;
    private bool _isWideLayout;
    private bool _isPhoneLayout;
    private const double WideBreakpoint = 800;
    private const double AlbumArtMinHeight = 80;

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

    private void OnVolumeToggle(object? sender, EventArgs e)
    {
        VolumeSlider.IsVisible = !VolumeSlider.IsVisible;
    }

    private void OnToggleQueue(object? sender, EventArgs e)
    {
        _isQueueVisible = !_isQueueVisible;
        ApplyQueueLayout();
    }

    private void OnToggleTags(object? sender, EventArgs e)
    {
        _isTagsVisible = !_isTagsVisible;
        ApplyTagsLayout();
    }

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        var wasWide = _isWideLayout;
        var wasPhone = _isPhoneLayout;
        _isWideLayout = Width >= WideBreakpoint;
        _isPhoneLayout = ResponsiveHelper.IsPhone(Width);

        if (wasWide != _isWideLayout)
        {
            ApplyQueueLayout();
            ApplyTagsLayout();
        }

        if (wasPhone != _isPhoneLayout)
            ApplyPhoneLayout();

        // Adjust sidebar widths proportionally when in wide layout
        if (_isWideLayout && Width > 0)
        {
            var maxSidebarWidth = Width * 0.35;
            if (_isQueueVisible)
                QueueSidebar.WidthRequest = Math.Min(300, maxSidebarWidth);
            if (_isTagsVisible)
                TagsSidebar.WidthRequest = Math.Min(280, maxSidebarWidth);
        }

        // Hide album art area when too small
        if (AlbumArtArea.Height >= 0)
        {
            AlbumArtPlaceholder.IsVisible = AlbumArtArea.Height >= AlbumArtMinHeight;
        }
    }

    private void ApplyPhoneLayout()
    {
        // On phone: hide sidebar toggle buttons (no room for sidebars), compact album art, hide volume slider
        QueueToggleButton.IsVisible = !_isPhoneLayout;
        TagsToggleButton.IsVisible = !_isPhoneLayout;
        AlbumArtPlaceholder.WidthRequest = _isPhoneLayout ? ResponsiveHelper.AlbumArtPhone : ResponsiveHelper.AlbumArtDesktop;
        AlbumArtPlaceholder.HeightRequest = _isPhoneLayout ? ResponsiveHelper.AlbumArtPhone : ResponsiveHelper.AlbumArtDesktop;

        // Close sidebars if switching to phone
        if (_isPhoneLayout)
        {
            _isQueueVisible = false;
            _isTagsVisible = false;
            ApplyQueueLayout();
            ApplyTagsLayout();
        }
    }

    private void ApplyQueueLayout()
    {
        QueueSidebar.IsVisible = _isQueueVisible;

        if (!_isQueueVisible)
        {
            Grid.SetColumn(QueueSidebar, 1);
            Grid.SetColumnSpan(QueueSidebar, 1);
            QueueSidebar.ZIndex = 0;
            return;
        }

        if (_isWideLayout)
        {
            Grid.SetColumn(QueueSidebar, 1);
            Grid.SetColumnSpan(QueueSidebar, 1);
            QueueSidebar.ZIndex = 0;
            var maxW = Width > 0 ? Width * 0.35 : 300;
            QueueSidebar.WidthRequest = Math.Min(300, maxW);
        }
        else
        {
            Grid.SetColumn(QueueSidebar, 0);
            Grid.SetColumnSpan(QueueSidebar, 3);
            QueueSidebar.ZIndex = 50;
            QueueSidebar.WidthRequest = -1;
        }
    }

    private void OnHamburgerClicked(object? sender, EventArgs e)
    {
        NavOverlay.CurrentRoute = "NowPlaying";
        NavOverlay.Toggle();
    }

    private void ApplyTagsLayout()
    {
        TagsSidebar.IsVisible = _isTagsVisible;

        if (!_isTagsVisible)
        {
            Grid.SetColumn(TagsSidebar, 2);
            Grid.SetColumnSpan(TagsSidebar, 1);
            TagsSidebar.ZIndex = 0;
            return;
        }

        if (_isWideLayout)
        {
            Grid.SetColumn(TagsSidebar, 2);
            Grid.SetColumnSpan(TagsSidebar, 1);
            TagsSidebar.ZIndex = 0;
            var maxW = Width > 0 ? Width * 0.35 : 280;
            TagsSidebar.WidthRequest = Math.Min(280, maxW);
        }
        else
        {
            Grid.SetColumn(TagsSidebar, 0);
            Grid.SetColumnSpan(TagsSidebar, 3);
            TagsSidebar.ZIndex = 50;
            TagsSidebar.WidthRequest = -1;
        }
    }
}
