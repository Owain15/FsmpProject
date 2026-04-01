# FSMP Solution - Progress Dashboard

## Project Status Overview

| Project | Description | Status | Coverage | Todo |
|---------|-------------|--------|----------|------|
| FSMP.Core | Cross-platform business logic | Complete | 85.2% | [todo](FSMP.lib/FSMP.Core/todo.md) |
| FSMP.Platform.Windows | Windows audio (LibVLC) | Complete | 80.1% | -- |
| FsmpDataAcsses | EF Core data access layer | Complete | 97.8% | [todo](FSMP.db/entity/FsmpDataAcsses/todo.md) |
| FsmpConsole | Console UI application | Complete | 87.3% | [todo](FSMP.UI/FSMP.UI.Console/FsmpConsole/todo.md) |
| FSMO | File System Music Organizer | Complete | 95.1% | [todo](FSMP.lib/FSMO/todo.md) |
| FSMP.Tests | Test suite | Complete | -- | [todo](FSMP.Tests/todo.md) |
| FSMP.MAUI | MAUI UI (Windows) | Complete | -- | -- |

**Overall coverage**: 89.4% line / 75.3% branch | **Tests**: 1166/1166 passing | **Build**: Passing (Windows + Android)
**Last verified**: 2026-03-30

### Test Verification Status

| Architecture | Last Verified | Result |
|---|---|---|
| x64 (native) | 2026-03-20 | 1113/1114 passing — `ConfigurationServiceTests.GetDefaultConfiguration_ShouldReturnValidDefaults` failing |
| ARM64 (native) | Unknown | Not yet verified natively (runs under x64 emulation on dev machine) |
| Android | Not started | Awaiting device/emulator |

---

## Active Work

### MAUI Icon Buttons via Reusable Component

**Status**: Complete
**Goal**: Replace plain text buttons with icon+label buttons using a reusable IconButton component

| # | Task | Status |
|---|------|--------|
| 1 | Create IconButton component (Components/IconButton.xaml + .cs) | Done |
| 2 | Update NowPlayingPage with transport icon buttons | Done |
| 3 | Update LibraryPage with icon buttons | Done |
| 4 | Update PlaylistsPage with icon buttons | Done |
| 5 | Update PlaylistDetailPage with icon buttons | Done |
| 6 | Update SettingsPage with icon buttons | Done |
| 7 | Add emoji prefixes to repeat/shuffle mode text in ViewModel | Done |

---

### MAUI Now Playing — Responsive Queue Sidebar

**Status**: Complete
**Goal**: Move queue to a collapsible sidebar with responsive wide/narrow layout

| # | Task | Status |
|---|------|--------|
| 1 | Two-column Grid layout with sidebar panel in NowPlayingPage.xaml | Done |
| 2 | Toggle button + SizeChanged responsive logic in code-behind | Done |
| 3 | Pass `truncate: false` to show full queue in MAUI | Done |
| 4 | Theme support via DynamicResource | Done |

---

### UI Philosophy — Startup Feedback & Loading States

**Status**: Complete
**Goal**: Apply UI philosophy (Fast, Uncoupled, Informative, Graceful) to Console and MAUI apps

| # | Task | Status |
|---|------|--------|
| 1 | Add UI Philosophy section to CLAUDE.md | Done |
| 2 | Console startup feedback (scan progress, config, session, audio) | Done |
| 3 | MAUI deferred init with status broadcast | Done |
| 4 | MAUI loading overlay on all 4 pages | Done |

---

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

### MAUI Now Playing — Seek Slider

**Status**: Complete
**Goal**: Replace read-only ProgressBar with draggable Slider for track seeking

| # | Task | Status |
|---|------|--------|
| 1 | Replace ProgressBar with two-way bound Slider in NowPlayingPage.xaml | Done |
| 2 | Add IsSeeking flag to suppress position updates during drag | Done |
| 3 | Wire DragStarted/DragCompleted to SeekAsync via Progress setter | Done |

---

### MAUI Theme System

**Status**: Complete
**Goal**: Add theme selection (Light, Dark, Light Blue) to Settings with persistence

| # | Task | Status |
|---|------|--------|
| 1 | Add `Theme` property to `Configuration` model | Done |
| 2 | Create theme ResourceDictionaries (Light, Dark, Light Blue) | Done |
| 3 | Create `ThemeManager` helper for runtime theme switching | Done |
| 4 | Add theme Picker to Settings page with live preview | Done |
| 5 | Apply saved theme on app startup | Done |
| 6 | Persist theme selection via config.json | Done |

> **Note**: Custom Theme feature (CustomThemePage, CustomThemeViewModel) was removed as unsatisfactory. Rebuild planned for a future release.

---

### Settings Enhancement

**Status**: In progress
**Goal**: Enrich MAUI Settings sections with useful settings

#### Completed Settings Pages

| # | Feature | Status |
|---|---------|--------|
| 1 | Configuration model — new properties | Done |
| 2 | SettingsViewModel — new properties & scan selected | Done |
| 3 | Playback Settings — resume session + auto-play | Done |
| 4 | Appearance Settings — text size picker | Done |
| 5 | Behavior Settings — double-click + sort order + reset (polished layout) | Done |
| 6 | About Settings — library stats (readonly) | Done |
| 7 | About Settings — Directories Data collapsible section (per-directory track/album/artist counts) | Done |

> **Future**: Add duplicate count per directory (by FileHash) to Directories Data section.

#### Directories Settings (formerly Library)

| # | Task | Status |
|---|------|--------|
| 1 | Rename Library → Directories + restructure layout | Done |
| 2 | Create ManageDirectoriesPage sub-page (add/remove paths) | Done |
| 3 | Scan Selected with per-directory checkboxes + CanExecute | Done |
| 4 | Update SettingsPage navigation label | Done |
| 5 | Move scan controls to ScanLibraryPage sub-page | Done |
| 6 | ManageDirectories: add at top, scrollable list, empty prompt | Done |
| 7 | Inline edit for directory paths (DirectoryItem model) | Done |

#### Appearance Settings Overhaul

| # | Task | Status |
|---|------|--------|
| 1 | Remove custom theme code (CustomThemePage, CustomThemeViewModel, "Custom" option) | Done |
| 2 | Polish AppearanceSettingsPage layout (section headers + descriptions) | Done |
| 3 | Create TextSizeManager (DynamicResource dictionary swap pattern) | Done |
| 4 | Replace hardcoded FontSize values with DynamicResource across all XAML | Done |
| 5 | Wire TextSize runtime application + startup load | Done |
| 6 | TextSizeManager tests | Done |

### Responsive Layout — Phone & Desktop Screen Sizes

**Status**: Complete
**Goal**: Target phone (portrait, <600dp) and desktop (≥600dp) with responsive layout logic

| # | Task | Status |
|---|------|--------|
| 1 | ResponsiveHelper in FSMP.Core (IsPhone, breakpoint, art sizes) | Done |
| 2 | Android portrait lock (MainActivity ScreenOrientation) | Done |
| 3 | NowPlayingPage responsive (hide sidebar toggles, compact art on phone) | Done |
| 4 | NavMenuOverlay phone-friendly (full width on phone) | Done |
| 5 | ResponsiveHelperTests (14 tests) | Done |
| 6 | ResponsiveLayoutTests (12 tests) | Done |
| 7 | Fix FSMP.Tests NU1201 errors (remove incompatible Android/MAUI references) | Done |
| 8 | CLAUDE.md Target Screen Sizes section | Done |

---

#### Future — Custom Theme Editor (Rebuild)

| # | Task | Status |
|---|------|--------|
| 1 | Redesign custom theme editor | Not started |

#### Future — FSMO Integration (High Priority)

| # | Task | Status |
|---|------|--------|
| 1 | FSMO dry-run preview | Not started |
| 2 | IFileOrganizerService + implementation | Not started |
| 3 | FileOrganizerViewModel | Not started |
| 4 | Organize Music Page | Not started |
| 5 | Compare & Sync Page | Not started |
| 6 | Reorganize Library Page | Not started |
| 7 | DI registration for FSMO services | Not started |

#### Future — Organize Defaults

| # | Task | Status |
|---|------|--------|
| 1 | Default organize mode (Copy/Move) setting | Not started |
| 2 | Default duplicate strategy (Skip/Overwrite/Rename) setting | Not started |
| 3 | Unknown artist/album name defaults | Not started |

---

## Deferred Work

### Android Support

**Status**: Android-ready — build compiles, needs device/emulator verification

| Phase | Description | Status |
|-------|-------------|--------|
| Platform.Android project setup | Create project, configure LibVLCSharp Android | Done |
| Android build verification | `build-android.cmd` compiles successfully | Done |
| Solution build fix | MAUI csproj restricts to Windows TFM during solution build | Done |
| Coverlet config update | Include all testable projects in coverage | Done |
| Cross-platform config tests | Android-style path handling verified | Done |
| MAUI DI wiring tests | ServiceRegistrationTests verifying shared container | Done |
| Theme data contract tests | ThemeManagerTests verifying theme model round-trips | Done |
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
build.cmd                    # Build console + MAUI solution (MSBuild for COM interop)
build-android.cmd            # Build MAUI for Android target
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
