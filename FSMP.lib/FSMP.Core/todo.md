# FSMP.Core - Platform-Agnostic Player Logic

Cross-platform business logic for playlist management, active playback queue, and repeat/shuffle modes.

## Completed Work

### Project Setup

- [x] Create FSMP.Core.csproj (net10.0, ARM64, references FsmpLibrary)
- [x] Add to solution file (FsmpConsole.slnx)
- [x] Add project reference from FSMP.Tests

## Current Work

### Playlist + Music Player Feature

- [ ] Create `RepeatMode.cs` enum (None, One, All)
- [ ] Create `ActivePlaylistService.cs` (in-memory playlist queue with shuffle, repeat, next/prev)
- [ ] ActivePlaylistService tests

## Current Status

**Status**: In progress | **Coverage**: -- | **Tests**: see FSMP.Tests

---

## Progress Summary

**Status**: Project setup complete, implementing playlist logic
**Next Action**: RepeatMode enum + ActivePlaylistService