# FSMP.Core

Cross-platform abstractions and platform-agnostic services for FSMP.

**Status:** Complete

## Responsibilities

**This project is responsible for:**
- Cross-platform interfaces (IAudioPlayer, IAudioPlayerFactory, IAudioService, etc.)
- Orchestration interfaces and implementations (IPlaybackController, ILibraryBrowser, IPlaylistManager, ILibraryManager)
- Service interfaces (IPlaylistService, ILibraryScanService, IConfigurationService, ITagService)
- Shared models (Track, Album, Artist, Playlist, QueueState, Configuration, etc.)
- ActivePlaylistService (in-memory queue with shuffle/repeat)
- MAUI ViewModels (NowPlaying, LibraryBrowse, Settings, Playlists)
- Result<T> pattern for error handling

**This project is NOT responsible for:**
- Platform-specific implementations (see FSMP.Platform.Windows / FSMP.Platform.Android)
- UI or user interaction (see FsmpConsole / FSMP.MAUI)
- Database access (see FsmpDataAcsses)

## How It Fits In

Business Logic Layer — provides the abstractions that platform projects implement. Consumed by both the console app and the MAUI app.
