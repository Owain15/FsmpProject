# FSMP Solution - Progress Dashboard

## Project Status Overview

| Project | Description | Status | Coverage | Todo |
|---------|-------------|--------|----------|------|
| FsmpLibrary | Core business logic & models | Complete (v1) | 95.43% | [todo](FSMP.lib/FsmpLibrary/todo.md) |
| FSMP.Core | Platform-agnostic player logic | In progress | 99.53% | [todo](FSMP.lib/FSMP.Core/todo.md) |
| FsmpDataAcsses | EF Core data access layer | Complete (v1) | 98.46% | [todo](FSMP.db/entity/FsmpDataAcsses/todo.md) |
| FsmpConsole | Console UI application | Complete (v1) | 80.37% | [todo](FSMP.UI/FSMP.UI.Console/FsmpConsole/todo.md) |
| FSMO | File System Music Organizer | Complete (10/10) | 96.39% | [todo](FSMP.lib/FSMO/todo.md) |
| FSMP.Tests | Test suite | Complete (v1) | -- | [todo](FSMP.Tests/todo.md) |
| FSMP.MAUI | Cross-platform MAUI UI | In progress | -- | [todo](FSMP.UI/FSMP.MAUI/todo.md) |

**Overall coverage**: 93.99% | **Tests**: 863 passing | **Build**: Passing

---

## Completed Milestones

### v1.0 -- Console Music Player (26 slices)

- [x] Testing infrastructure (xUnit, Moq, FluentAssertions, Coverlet)
- [x] Entity models (Track, Album, Artist, Genre, FileExtension, PlaybackHistory, LibraryPath, Configuration)
- [x] DI infrastructure and LibVLCSharp audio migration (replaced WMPLib COM)
- [x] Database layer (EF Core + SQLite, repository pattern, Unit of Work, migrations)
- [x] Services (Configuration, Metadata, Library Scan, Playback Tracking, Statistics)
- [x] Console UI (Menu, Browse, Playback, Metadata Editor, Library Manager, Statistics Viewer)
- [x] Program.cs integration with AppStartup
- [x] End-to-end testing and error handling (513 tests, 92.49% coverage)

Full slice-by-slice history: [todo-v1-archive.md](todo-v1-archive.md)

---

## Active Work

### Playlist + Music Player Feature

**Status**: Complete | **Batches**: 17/17 complete
**Plan**: [.claude/plans/smooth-wishing-cerf.md](.claude/plans/smooth-wishing-cerf.md)

| # | Component | Status | Project |
|---|-----------|--------|---------|
| 1 | FSMP.Core project setup (csproj, slnx, refs) | **Complete** | FSMP.Core |
| 2 | RepeatMode enum + ActivePlaylistService | **Complete** | FSMP.Core |
| 3 | ActivePlaylistService tests | **Complete** | FSMP.Tests |
| 4 | Playlist + PlaylistTrack models | **Complete** | FsmpLibrary |
| 5 | Playlist model tests | **Complete** | FSMP.Tests |
| 6 | FsmpDbContext (DbSets + config) | **Complete** | FsmpDataAcsses |
| 7 | EF Core migration (AddPlaylists) | **Complete** | FsmpDataAcsses |
| 8 | Entity configuration tests | **Complete** | FSMP.Tests |
| 9 | PlaylistRepository + UnitOfWork | **Complete** | FsmpDataAcsses |
| 10 | PlaylistRepository tests | **Complete** | FSMP.Tests |
| 11 | PlaylistService | **Complete** | FsmpDataAcsses |
| 12 | PlaylistService tests | **Complete** | FSMP.Tests |
| 13 | PlayerUI + Print.NewDisplay() update | **Complete** | FsmpConsole |
| 14 | MenuSystem + AppStartup + BrowseUI updates | **Complete** | FsmpConsole |
| 15 | PlayerUI tests + Print.NewDisplay tests | **Complete** | FSMP.Tests |
| 16 | MenuSystem/BrowseUI test updates | **Complete** | FSMP.Tests |
| 17 | Update all todo.md files | **Complete** | — |

**Key Features**:
- Saved playlists (DB-persisted with ordered tracks)
- Active playlist (in-memory queue of track IDs)
- Music player view with current track display
- Playback controls: play/pause, next, prev, restart, stop
- Repeat modes: None, One, All
- Shuffle on/off

---

### FsmpLibrary Coverage Improvement

**Status**: Complete | **Batches**: 11/11 complete
**Plan**: [.claude/plans/nifty-launching-sunset.md](.claude/plans/nifty-launching-sunset.md)

Refactored `LibVlcAudioPlayer` to extract `IMediaPlayerAdapter` interface, enabling unit testing of the audio player business logic without requiring LibVLC runtime. Added 52 new tests covering constructor, properties, LoadAsync, PlayAsync, PauseAsync, StopAsync, SeekAsync, event handlers, state machine, and disposal.

**Result**: FsmpLibrary coverage **65.74% -> 86.26%** (target was 80%+)

---

### FSMO -- File System Music Organizer

**Status**: Complete (10/10 slices)

Scan source directories for audio files and reorganize them into `Artist/Album/Track` structure. Supports copy and move operations with duplicate handling.

See [FSMO todo](FSMP.lib/FSMO/todo.md) for detailed task breakdown.

---

## Console UI Restructure: Player as Main Screen

**Status**: Complete | **Plan**: [.claude/plans/lexical-growing-acorn.md](.claude/plans/lexical-growing-acorn.md)

Replace the 8-option main menu with the Player screen as the primary UI. Navigation to Browse, Playlists, and Directories via hotkeys from the player.

| # | Task | Status | Project |
|---|------|--------|---------|
| 1 | Expand PlayerUI with B/L/D/X hotkeys and sub-screen launchers | **Complete** | FsmpConsole |
| 2 | Update Print.NewDisplay to show navigation hotkeys | **Complete** | FsmpConsole |
| 3 | Simplify/remove MenuSystem (PlayerUI is now the entry point) | **Complete** | FsmpConsole |
| 4 | Update AppStartup wiring for expanded PlayerUI | **Complete** | FsmpConsole |
| 5 | Update PlayerUI tests for new hotkeys and sub-screens | **Complete** | FSMP.Tests |
| 6 | Update MenuSystem tests / remove if MenuSystem removed | **Complete** | FSMP.Tests |
| 7 | Update Print.NewDisplay tests | **Complete** | FSMP.Tests |
| 8 | Verify build, all tests pass, 80%+ coverage maintained | **Complete** | All |

**Removed from main flow**: Statistics, Settings, Scan Libraries (scan accessible via Directories)

---

## Player Menu Bug Fixes & Enhancements

**Status**: Complete

| # | Fix | Status | File |
|---|-----|--------|------|
| 1 | RestartTrackAsync resumes playback after seek | **Complete** | PlayerUI.cs |
| 2 | Directory path validation before adding | **Complete** | PlayerUI.cs |
| 3 | Show scan error details instead of just count | **Complete** | PlayerUI.cs |
| 4 | Directories menu loops (like Playlists) | **Complete** | PlayerUI.cs |
| 5 | Error handling on playlist create/delete | **Complete** | PlayerUI.cs |
| 6 | Shuffle/Repeat status messages | **Complete** | PlayerUI.cs |
| 7 | Auto-play on queue load (Browse/Playlists) | **Complete** | PlayerUI.cs |
| 8 | Pause key changed from Space to [K] (Space unreachable after Trim) | **Complete** | PlayerUI.cs, Print.cs |
| 9 | Resume after Stop re-loads track (LibVLC requirement) | **Complete** | PlayerUI.cs |
| 10 | Queue display: sliding window with current track centered | **Complete** | PlayerUI.cs |
| 11 | [V] View full queue | **Complete** | PlayerUI.cs |
| 12 | [#] Skip to track by number | **Complete** | PlayerUI.cs, Print.cs |
| 13 | Skip-to-track tests (4 new tests) | **Complete** | PlayerUITests.cs |

---

## Orchestration Service Refactor

**Status**: Complete | **Tests**: 863 passing

Replaced the god-object PlayerUI (6+ raw service deps) with a clean orchestration layer:
- `Result<T>` pattern for structured success/failure in FSMP.Core
- 4 data access interfaces: `ITrackRepository`, `IArtistRepository`, `IAlbumRepository`, `IPlaylistService`, `ILibraryScanService`, `IConfigurationService`
- 4 orchestration interfaces: `IPlaybackController`, `ILibraryBrowser`, `IPlaylistManager`, `ILibraryManager`
- 4 implementations in FsmpLibrary wrapping calls in try/catch returning `Result<T>`
- PlayerUI constructor: 4 orchestration deps (was 6 raw services + UnitOfWork)
- BrowseUI constructor: 2 orchestration deps (was UnitOfWork + IAudioService + ActivePlaylistService)
- No direct `UnitOfWork` usage in any UI file
- All UI error handling uses `result.IsSuccess` / `result.ErrorMessage`
- ScanResult moved to FSMP.Core.Models, QueueItem DTO added

---

## Cross-Platform Migration (Windows + Android)

**Status**: In progress | **Batch 1 Complete**
**Plan Document**: [.claude/plans/soft-percolating-codd.md](.claude/plans/soft-percolating-codd.md)
**Decision**: Use LibVLCSharp on both Windows and Android

| # | Phase | Status | Projects Affected |
|---|-------|--------|-------------------|
| 1 | Setup Projects | **Partial** | FSMP.Core ✓, FSMP.MAUI ✓, Platform.Windows ✓ (exists), Platform.Android pending |
| 2 | Platform Abstraction | **Partial** | Interfaces in FSMP.Core ✓, duplicate cleanup ✓ |
| 3 | Migrate Business Logic | Not started | FSMP.Core, FsmpLibrary (refactor) |
| 4 | Configure LibVLCSharp Android | Not started | Platform.Android |
| 5 | Build MAUI UI | Not started | FSMP.MAUI |
| 6 | Android-Specific Features | Not started | FSMP.MAUI, Platform.Android |
| 7 | Testing & Coverage | Not started | FSMP.Tests |
| 8 | Documentation & Migration | Not started | All |

**Batch 1 Complete** (Interface Cleanup):
- [x] Deleted duplicate interfaces from FsmpLibrary/Interfaces/
- [x] FsmpLibrary now uses FSMP.Core.Interfaces
- [x] Build passes (MAUI requires SDK installation)

**Key Requirements:**
- Support Windows 10/11 and Android 11+
- WMA playback WITHOUT altering originals (ExoPlayer FFmpeg on Android)
- Update console app to use new architecture (not deprecate)
- Maintain 80%+ test coverage throughout

When implementation begins, per-project todo.md files will be created for each new project.

---

## Remaining Manual Verification

- [ ] Fresh install creates config.json and fsmp.db
- [ ] Add multiple library paths via UI
- [ ] Scan all libraries successfully
- [ ] Browse database-driven navigation works
- [ ] WAV playback works
- [ ] WMA playback works
- [ ] MP3 playback works
- [ ] Metadata editing saves to database
- [ ] Statistics display correctly
- [ ] Application restart preserves all data
- [ ] Custom metadata overrides display correctly

---

## Build & Test Quick Reference

```batch
build.cmd                    # Build solution (MSBuild for COM interop)
test.cmd                     # Run all tests
test-with-coverage.cmd       # Run tests with coverage report
```
