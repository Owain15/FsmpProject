# FSMP.Core - Platform-Agnostic Player Logic

Cross-platform business logic for playlist management, active playback queue, and repeat/shuffle modes.

## Completed Work

### Project Setup

- [x] Create FSMP.Core.csproj (net10.0, ARM64, references FsmpLibrary)
- [x] Add to solution file (FsmpConsole.slnx)
- [x] Add project reference from FSMP.Tests

### Playlist + Music Player Feature

- [x] Create `RepeatMode.cs` enum (None, One, All)
- [x] Create `ActivePlaylistService.cs` (in-memory playlist queue with shuffle, repeat, next/prev)
- [x] ActivePlaylistService tests (37 tests in FSMP.Tests/Core/)

### Orchestration Service Refactor

- [x] `Result.cs` and `Result<T>` with IsSuccess, Value, ErrorMessage
- [x] `QueueItem.cs` display DTO
- [x] `ScanResult.cs` moved from FsmpLibrary
- [x] Data access interfaces: `ITrackRepository`, `IArtistRepository`, `IAlbumRepository`
- [x] Service interfaces: `IPlaylistService`, `ILibraryScanService`, `IConfigurationService`
- [x] Orchestration interfaces: `IPlaybackController`, `ILibraryBrowser`, `IPlaylistManager`, `ILibraryManager`

### Queue State Persistence

- [x] Create `QueueState.cs` model (OriginalOrder, PlayOrder, CurrentIndex, RepeatMode, IsShuffled)
- [x] Create `IQueueStateRepository.cs` interface (LoadAsync, SaveAsync)
- [x] Add `GetState()` / `RestoreState(QueueState)` to `IActivePlaylistService`
- [x] Implement GetState/RestoreState in `ActivePlaylistService`
- [x] Tests: 8 round-trip state tests in FSMP.Tests/Core/ActivePlaylistServiceStateTests.cs

## Current Status

### Cross-Platform Migration Phase 2

- [x] Add `InitializationError` property to `IAudioPlayerFactory` interface

## Current Status

### MAUI ViewModels

- [x] `NowPlayingViewModel.cs` — Playback controls, queue display, auto-advance
- [x] `LibraryBrowseViewModel.cs` — Artist/Album/Track drill-down with play/queue
- [x] `SettingsViewModel.cs` — Config management, library paths, scan, save
- [x] `PlaylistsViewModel.cs` — Playlist CRUD, load into queue

**Status**: Complete | **Coverage**: 99.53% | **Tests**: 45+ passing

### MAUI Session Restore Integration

- [x] `ActivePlaylistService.GetState()` / `RestoreState()` used by MAUI App.xaml.cs for session persistence
- [x] `IQueueStateRepository` registered in MAUI DI (MauiProgram.cs)
- [x] Deadlock fix: async calls wrapped in `Task.Run()` to avoid MAUI SynchronizationContext capture