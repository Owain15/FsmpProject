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

## Current Status

**Status**: Complete | **Coverage**: 99.53% | **Tests**: 37+ passing