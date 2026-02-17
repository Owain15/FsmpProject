# FSMP Solution - Progress Dashboard

## Project Status Overview

| Project | Description | Status | Coverage | Todo |
|---------|-------------|--------|----------|------|
| FsmpLibrary | Core business logic & models | Complete (v1) | 86.26% | [todo](FSMP.lib/FsmpLibrary/todo.md) |
| FSMP.Core | Platform-agnostic player logic | In progress | 100% | [todo](FSMP.lib/FSMP.Core/todo.md) |
| FsmpDataAcsses | EF Core data access layer | Complete (v1) | 98.52% | [todo](FSMP.db/entity/FsmpDataAcsses/todo.md) |
| FsmpConsole | Console UI application | Complete (v1) | 95.60% | [todo](FSMP.UI/FSMP.UI.Console/FsmpConsole/todo.md) |
| FSMO | File System Music Organizer | Not started | -- | [todo](FSMP.lib/FSMO/todo.md) |
| FSMP.Tests | Test suite | Complete (v1) | -- | [todo](FSMP.Tests/todo.md) |

**Overall coverage**: 96.43% | **Tests**: 759 all passing | **Build**: Passing

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

**Status**: 2/10 slices complete | **Next**: Slice 3 -- Audio File Discovery

Scan source directories for audio files and reorganize them into `Artist/Album/Track` structure. Supports copy and move operations with duplicate handling.

See [FSMO todo](FSMP.lib/FSMO/todo.md) for detailed task breakdown.

---

## Planned: Cross-Platform Migration (Windows + Android)

**Status**: Planning complete, not started | **Phases**: 0/8 complete
**Plan Document**: [.claude/plans/elegant-honking-locket.md](.claude/plans/elegant-honking-locket.md)

| # | Phase | Status | Projects Affected |
|---|-------|--------|-------------------|
| 1 | Setup Projects | Not started | NEW: FSMP.Core, Platform.Windows, Platform.Android, FSMP.MAUI |
| 2 | Platform Abstraction | Not started | FSMP.Core, Platform.Windows, Platform.Android |
| 3 | Migrate Business Logic | Not started | FSMP.Core, FsmpLibrary (refactor) |
| 4 | Configure ExoPlayer FFmpeg | Not started | Platform.Android |
| 5 | Build MAUI UI | Not started | FSMP.MAUI |
| 6 | Android-Specific Features | Not started | FSMP.MAUI, Platform.Android |
| 7 | Testing & Coverage | Not started | FSMP.Tests |
| 8 | Documentation & Migration | Not started | All |

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
