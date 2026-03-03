using FSMP.Core.Interfaces;

namespace FSMP.MAUI.ViewModels;

public class NowPlayingViewModel : Core.ViewModels.NowPlayingViewModel
{
    public NowPlayingViewModel(IPlaybackController playbackController, IAudioService audioService)
        : base(playbackController, audioService,
            MainThread.BeginInvokeOnMainThread,
            MainThread.InvokeOnMainThreadAsync)
    {
    }
}
