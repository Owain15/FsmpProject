using FSMP.Core.Interfaces;

namespace FSMP.MAUI.ViewModels;

public class NowPlayingViewModel : Core.ViewModels.NowPlayingViewModel
{
    public NowPlayingViewModel(IPlaybackController playbackController, IAudioService audioService, ITagService tagService, IConfigurationService configService)
        : base(playbackController, audioService, tagService, configService,
            MainThread.BeginInvokeOnMainThread,
            MainThread.InvokeOnMainThreadAsync)
    {
    }
}
