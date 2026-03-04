using FSMP.Core.Models;
using FSMP.MAUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FSMP.MAUI.Pages;

public partial class NowPlayingPage : ContentPage
{
    private readonly NowPlayingViewModel _viewModel;
    private readonly IServiceScope _scope;

    public NowPlayingPage()
    {
        _scope = App.Services.CreateScope();
        _viewModel = _scope.ServiceProvider.GetRequiredService<NowPlayingViewModel>();
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
            App.Log($"NowPlayingPage.OnAppearing error: {ex}");
        }
    }

    private void OnQueueSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is QueueItem item)
        {
            _viewModel.JumpToCommand.Execute(item);
            QueueCollectionView.SelectedItem = null;
        }
    }
}
