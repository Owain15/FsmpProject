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

## Current Status

**Status**: Complete (for current feature) | **Coverage**: -- | **Tests**: 37 passing

---

## Progress Summary

**Status**: All playlist-related FSMP.Core work complete
**Next Action**: PlaylistRepository (Batch 9, FsmpDataAcsses)