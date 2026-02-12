# FsmpConsole - Console UI Application

Interactive console application for FSMP, providing menu-driven access to library browsing, playback, metadata editing, and statistics.

## Completed Work Summary

### Application Startup

- `Program.cs` -- Entry point with top-level exception handling, delegating to AppStartup
- `AppStartup.cs` -- Testable startup: loads config, initializes SQLite + EF migrations, wires DI (UnitOfWork, MetadataService, LibraryScanService, StatisticsService, AudioService), auto-scans on startup, launches MenuSystem

### Menu System

- `MenuSystem.cs` -- Main event loop with 6 options: Browse & Play, Scan Libraries, View Statistics, Manage Libraries, Settings, Exit

### UI Components

- `BrowseUI.cs` -- Hierarchical Artist -> Album -> Track browser with track selection and playback
- `PlaybackUI.cs` -- Now Playing display (title, artist, album, duration, bit rate, play count, rating) + playback controls
- `MetadataEditor.cs` -- Search tracks, display file metadata + custom overrides, edit CustomTitle/CustomArtist/CustomAlbum/Rating/IsFavorite/Comment with clear ("-") support
- `LibraryManager.cs` -- List/add/remove library paths, trigger single or all-library scans
- `StatisticsViewer.cs` -- Overview, most played, recently played, favorites, genre breakdown
- `Print.cs` -- Utilities: FormatTable (aligned columns), FormatProgressBar, FormatMetadataCard

## Current Status

**Status**: Complete (v1) | **Coverage**: 94.20% | **Tests**: see FSMP.Tests

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

## Future Work

### Cross-Platform Migration (when Phase 8 begins)

- [ ] Refactor to use FSMP.Core instead of FsmpLibrary directly
- [ ] Refactor to use FSMP.Platform.Windows for audio playback
- [ ] Update DI registration for new architecture
- [ ] Ensure backward compatibility with existing config and database

---

## Progress Summary

**Status**: Complete (v1), pending manual verification
**Next Action**: Complete manual verification checklist
