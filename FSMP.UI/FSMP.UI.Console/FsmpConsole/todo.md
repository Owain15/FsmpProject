# FsmpConsole - Console UI Application

Interactive console application for FSMP, providing menu-driven access to library browsing, playback, metadata editing, and statistics.

## Completed Work Summary

### Application Startup

- `Program.cs` -- Entry point with top-level exception handling, delegating to AppStartup
- `AppStartup.cs` -- Testable startup: loads config, initializes SQLite + EF migrations, wires DI (UnitOfWork, MetadataService, LibraryScanService, StatisticsService, AudioService), auto-scans on startup, launches MenuSystem

### Menu System

- `MenuSystem.cs` -- Main event loop with 8 options: Browse & Play, Player, Playlists, Scan Libraries, View Statistics, Manage Libraries, Settings, Exit

### UI Components

- `BrowseUI.cs` -- Hierarchical Artist -> Album -> Track browser with track selection and playback
- `MetadataEditor.cs` -- Search tracks, display file metadata + custom overrides, edit CustomTitle/CustomArtist/CustomAlbum/Rating/IsFavorite/Comment with clear ("-") support
- `StatisticsViewer.cs` -- Overview, most played, recently played, favorites, genre breakdown
- `PlayerUI.cs` -- Music player view with queue display, playback controls (next/prev/pause/resume/stop/restart), repeat mode cycling, shuffle toggle
- `Print.cs` -- Utilities: NewDisplay (player view), FormatTable, FormatProgressBar, FormatMetadataCard

## Current Status

**Status**: Complete (v1) | **Coverage**: 80.37% | **Tests**: see FSMP.Tests

> Note: Coverage dropped after removing dead code (PlaybackUI.cs, LibraryManager.cs) that was superseded by PlayerUI.cs but had been inflating coverage numbers.

---

## Manual Verification Checklist

- [ ] Fresh install creates config.json and fsmp.db
- [ ] Add multiple library paths via Library Manager
- [ ] Scan all libraries successfully
- [ ] Browse Artists -> Albums -> Tracks navigation works
- [ ] WAV playback works
- [ ] WMA playback works
- [ ] MP3 playback works
- [ ] Edit track metadata and verify it persists
- [ ] View statistics and verify correct counts
- [ ] Restart application and verify data persists
- [ ] Custom metadata overrides display correctly
- [ ] No hardcoded paths in code

---

## Active Work

### Console UI Restructure: Player as Main Screen

**Status**: Complete | **Plan**: [.claude/plans/lexical-growing-acorn.md](../../../../.claude/plans/lexical-growing-acorn.md)

- [x] Expand `PlayerUI.cs` — Add dependencies (PlaylistService, ConfigurationService, LibraryScanService), add hotkeys: `[B]` Browse, `[L]` Playlists, `[D]` Directories, `[X]` Exit. Remove `[Q]` back-to-menu.
- [x] Update `Print.cs` — Update `NewDisplay` to show B/L/D/X navigation hotkeys in controls section
- [x] Simplify or remove `MenuSystem.cs` — PlayerUI is now the main entry point
- [x] Update `AppStartup.cs` — Wire additional services to PlayerUI
- [x] Verify build passes and coverage ≥ 80%

### Player Menu Bug Fixes & Enhancements

**Status**: Complete

- [x] Fix `RestartTrackAsync` — resume playback after seek to zero
- [x] Add directory path validation (`Directory.Exists`) before adding
- [x] Show individual scan error messages instead of just count
- [x] Make Directories menu loop (like Playlists already does)
- [x] Add try-catch error handling to playlist create and delete
- [x] Shuffle/Repeat toggle status messages
- [x] Auto-play when queue loaded from Browse or Playlists
- [x] Pause key changed from Space to `[K]` (Space was unreachable after `Trim()`)
- [x] Resume after Stop re-loads track via `PlayTrackByIdAsync` (LibVLC requires re-load after stop)
- [x] Queue display: sliding window (9 visible) with current track in middle 3 lines
- [x] `[V]` View full queue command
- [x] `[#]` Skip to track by number input
- [x] 4 new skip-to-track tests added

---

## Future Work

### Cross-Platform Migration (when Phase 8 begins)

- [ ] Refactor to use FSMP.Core instead of FsmpLibrary directly
- [ ] Refactor to use FSMP.Platform.Windows for audio playback
- [ ] Update DI registration for new architecture
- [ ] Ensure backward compatibility with existing config and database

---

## Progress Summary

**Status**: Complete (v1) + Playlist/Player UI integration (Batches 13-14 complete)
**Next Action**: All playlist/player feature tests complete (Batches 15-16)
