# FSMP Solution - Progress Dashboard

## Project Status Overview

| Project | Description | Status | Coverage | Todo |
|---------|-------------|--------|----------|------|
| FsmpLibrary | Core business logic & models | Complete (v1) | 65.74% | [todo](FSMP.lib/FsmpLibrary/todo.md) |
| FSMP.Core | Platform-agnostic player logic | Not started | -- | [todo](FSMP.lib/FSMP.Core/todo.md) |
| FsmpDataAcsses | EF Core data access layer | Complete (v1) | 98.18% | [todo](FSMP.db/entity/FsmpDataAcsses/todo.md) |
| FsmpConsole | Console UI application | Complete (v1) | 94.20% | [todo](FSMP.UI/FSMP.UI.Console/FsmpConsole/todo.md) |
| FSMO | File System Music Organizer | Not started | -- | [todo](FSMP.lib/FSMO/todo.md) |
| FSMP.Tests | Test suite | Complete (v1) | -- | [todo](FSMP.Tests/todo.md) |

**Overall coverage**: 92.49% | **Tests**: 530 passing | **Build**: Passing

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

**Status**: In progress | **Batches**: 4/17 complete
**Plan**: [.claude/plans/smooth-wishing-cerf.md](.claude/plans/smooth-wishing-cerf.md)

| # | Component | Status | Project |
|---|-----------|--------|---------|
| 1 | FSMP.Core project setup (csproj, slnx, refs) | Not started | FSMP.Core |
| 2 | RepeatMode enum + ActivePlaylistService | Not started | FSMP.Core |
| 3 | ActivePlaylistService tests | Not started | FSMP.Tests |
| 4 | Playlist + PlaylistTrack models | **Complete** | FsmpLibrary |
| 5 | Playlist model tests | **Complete** | FSMP.Tests |
| 6 | FsmpDbContext (DbSets + config) | **Complete** | FsmpDataAcsses |
| 7 | EF Core migration (AddPlaylists) | **Complete** | FsmpDataAcsses |
| 8 | Entity configuration tests | Not started | FSMP.Tests |
| 9 | PlaylistRepository + UnitOfWork | Not started | FsmpDataAcsses |
| 10 | PlaylistRepository tests | Not started | FSMP.Tests |
| 11 | PlaylistService | Not started | FsmpDataAcsses |
| 12 | PlaylistService tests | Not started | FSMP.Tests |
| 13 | PlayerUI + Print.NewDisplay() update | Not started | FsmpConsole |
| 14 | MenuSystem + AppStartup + BrowseUI updates | Not started | FsmpConsole |
| 15 | PlayerUI tests | Not started | FSMP.Tests |
| 16 | MenuSystem/AppStartup/BrowseUI test updates | Not started | FSMP.Tests |
| 17 | Update all todo.md files | Not started | — |

**Key Features**:
- Saved playlists (DB-persisted with ordered tracks)
- Active playlist (in-memory queue of track IDs)
- Music player view with current track display
- Playback controls: play/pause, next, prev, restart, stop
- Repeat modes: None, One, All
- Shuffle on/off

---

### FSMO -- File System Music Organizer

**Status**: 0/10 slices complete | **Next**: Slice 1 -- Testing Infrastructure

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
