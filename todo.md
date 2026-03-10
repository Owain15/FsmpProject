# FSMP Solution - Progress Dashboard

## Project Status Overview

| Project | Description | Status | Coverage | Todo |
|---------|-------------|--------|----------|------|
| FSMP.Core | Cross-platform business logic | In progress | 88.4% | [todo](FSMP.lib/FSMP.Core/todo.md) |
| FsmpDataAcsses | EF Core data access layer | Complete (v1) | 98.4% | [todo](FSMP.db/entity/FsmpDataAcsses/todo.md) |
| FsmpConsole | Console UI application | Complete (v1) | 88.7% | [todo](FSMP.UI/FSMP.UI.Console/FsmpConsole/todo.md) |
| FSMO | File System Music Organizer | Complete (10/10) | 96.3% | [todo](FSMP.lib/FSMO/todo.md) |
| FSMP.Tests | Test suite | Complete (v1) | -- | [todo](FSMP.Tests/todo.md) |
| FSMP.MAUI | MAUI UI (Windows) | Complete (v1) | -- | -- |

**Overall coverage**: ~94% | **Tests**: 1042 passing | **Build**: Passing

---

## Active Work

### Tag Management & Filtering

**Status**: Complete
**Goal**: Full tag CRUD + library filtering in Console and MAUI apps

| # | Task | Status |
|---|------|--------|
| 1 | ITagRepository + TagRepository | Done |
| 2 | GetByTagAsync on Track/Album/Artist repos | Done |
| 3 | Repository tests (TagRepository + GetByTagAsync) | Done |
| 4 | ITagService + TagService | Done |
| 5 | TagService tests | Done |
| 6 | ILibraryBrowser tag-filtering methods | Done |
| 7 | Console UI — T hotkey (tag management) + F hotkey (filter by tag in Browse) | Done |
| 8 | MAUI — Tag filter chip bar in Library page | Done |

---

### MAUI Windows — Build, Run & Verify

**Status**: Complete
**Goal**: Get the MAUI app building and running on Windows to verify audio + UI functionality

The MAUI app has all UI pages and ViewModels wired up but has never been built or run. The flow is: Settings (add directory → scan) → Library (browse → queue) → Now Playing (playback).

| # | Task | Status |
|---|------|--------|
| 1 | Enable MAUI build in solution (currently `Build=false` in slnx) | Done |
| 2 | Build MAUI project, fix compilation errors | Done |
| 3 | Fix runtime issues (LibVLC ARM64, DB paths, EF migrations) | Done |
| 4 | Fix MAUI deadlock on session restore (Task.Run wrapper) | Done |
| 5 | Add session save/restore to MAUI (queue state persistence) | Done |
| 6 | Register IQueueStateRepository in MAUI DI | Done |
| 7 | Verify end-to-end: add directory → scan → browse → queue → play | Done |
| 8 | Update build.cmd with MAUI build support | Done (solution build includes MAUI) |
| 9 | Add ViewModel tests to maintain 80%+ coverage | Done |

---

## Deferred Work

### Android Support

**Status**: Deferred — waiting until MAUI works on Windows

| Phase | Description | Status |
|-------|-------------|--------|
| Platform.Android project setup | Create project, configure LibVLCSharp Android | Not started |
| ExoPlayer FFmpeg for WMA | Real-time WMA decoding on Android | Not started |
| Android-specific features | Permissions, background playback, lock screen | Not started |
| Android testing | Device/emulator verification | Not started |

---

## Completed Milestones

### Cross-Platform Architecture Migration

**Status**: Complete (Batches 1-3)

Restructured the codebase for cross-platform support:
- Created FSMP.Core with cross-platform interfaces, models, services, and ViewModels
- Created FSMP.Platform.Windows with LibVLC audio player implementation
- Created FSMP.MAUI with 4-tab UI (Now Playing, Library, Playlists, Settings)
- Removed FsmpLibrary (replaced by FSMP.Core + FSMP.Platform.Windows)
- 960 tests passing, 94.3% coverage

### v1.0 — Console Music Player (26 slices)

- [x] Testing infrastructure (xUnit, Moq, FluentAssertions, Coverlet)
- [x] Entity models (Track, Album, Artist, Genre, FileExtension, PlaybackHistory, LibraryPath, Configuration)
- [x] DI infrastructure and LibVLCSharp audio migration (replaced WMPLib COM)
- [x] Database layer (EF Core + SQLite, repository pattern, Unit of Work, migrations)
- [x] Services (Configuration, Metadata, Library Scan, Playback Tracking, Statistics)
- [x] Console UI (Menu, Browse, Playback, Metadata Editor, Library Manager, Statistics Viewer)
- [x] Program.cs integration with AppStartup
- [x] End-to-end testing and error handling

Full slice-by-slice history: [todo-v1-archive.md](todo-v1-archive.md)

### Playlist + Music Player Feature

- Saved playlists (DB-persisted with ordered tracks)
- Active playlist (in-memory queue of track IDs)
- Music player view with playback controls (play/pause, next, prev, stop)
- Repeat modes (None, One, All) and shuffle

### FSMO — File System Music Organizer

Scan source directories for audio files and reorganize into Artist/Album/Track structure. Copy/move with duplicate handling. See [FSMO todo](FSMP.lib/FSMO/todo.md).

### Console UI Restructure

Player screen as primary UI with hotkey navigation (B=Browse, L=Playlists, D=Directories, X=Exit).

### Orchestration Service Refactor

Replaced god-object PlayerUI with clean orchestration layer: IPlaybackController, ILibraryBrowser, IPlaylistManager, ILibraryManager with Result<T> pattern.

### Queue State Persistence

Persist active queue (track order, position, shuffle, repeat) across sessions as JSON. Implemented in both Console and MAUI apps.

### Player Bug Fixes & Enhancements

13 fixes including auto-play, pause key, resume after stop, sliding queue window, skip-to-track.

### Test Isolation Audit

Extracted IActivePlaylistService, mocked dependencies, split/fixed tests.

### FsmpLibrary Coverage Improvement

LibVlcAudioPlayer refactored with IMediaPlayerAdapter. Coverage 65.74% → 86.26%.

---

## Build & Test Quick Reference

```batch
build.cmd                    # Build console solution (MSBuild for COM interop)
test.cmd                     # Run all tests
test-with-coverage.cmd       # Run tests with coverage report
```

---

## Manual Verification Checklist

- [ ] Fresh install creates config.json and fsmp.db
- [ ] Add multiple library paths via UI
- [ ] Scan all libraries successfully
- [ ] Browse database-driven navigation works
- [ ] WAV playback works
- [ ] WMA playback works
- [ ] MP3 playback works
- [ ] Metadata editing saves to database
- [ ] Application restart preserves all data
