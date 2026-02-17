# FsmpLibrary - Core Business Logic & Models

Core library containing entity models, audio playback interfaces, and business services for FSMP.

## Completed Work Summary

### Entity Models (`Models/`)

All entity models for the data layer, used by both FsmpLibrary and FsmpDataAcsses:

- `Track.cs` -- Audio track with metadata (title, artist, album, duration, play count, rating, favorites, genres, file extension FK)
- `Album.cs` -- Album with artist relationship, year, album art, genres
- `Artist.cs` -- Artist with biography, sort name, albums/tracks/genres collections
- `Genre.cs` -- Lookup entity with many-to-many relationships to Track, Album, Artist
- `FileExtension.cs` -- Lookup entity (wav, wma, mp3) with FK from Track
- `PlaybackHistory.cs` -- Play event record with duration, completed/skipped flags
- `LibraryPath.cs` -- Configured library directory with scan tracking
- `Configuration.cs` -- App configuration POCO (library paths, DB path, volume, auto-scan)

### Audio Interfaces (`Interfaces/`)

Platform-agnostic audio playback abstraction:

- `IAudioPlayer.cs` -- Load/Play/Pause/Stop/Seek with state and position events
- `IAudioPlayerFactory.cs` -- Factory for creating audio player instances
- `PlaybackState.cs` -- Enum: Stopped, Playing, Paused, Loading, Error
- `EventArgs/` -- PlaybackStateChanged, PlaybackCompleted, PlaybackError, PositionChanged

### Audio Implementation (`Audio/`)

LibVLCSharp-based audio playback (replaced WMPLib COM interop), refactored with adapter pattern:

- `IMediaPlayerAdapter.cs` -- Thin adapter interface wrapping platform-specific media player operations
- `LibVlcMediaPlayerAdapter.cs` -- IMediaPlayerAdapter implementation using LibVLCSharp (thin pass-through)
- `LibVlcAudioPlayer.cs` -- IAudioPlayer implementation with business logic (state machine, validation, events), uses IMediaPlayerAdapter
- `LibVlcAudioPlayerFactory.cs` -- IAudioPlayerFactory implementation with optional adapter factory injection

### Services (`Services/`)

- `IAudioService.cs` / `AudioService.cs` -- High-level playback orchestration (play track/file, volume, pause/resume/stop/seek)
- `ConfigurationService.cs` -- JSON config file management (load/save/add path/remove path, corrupt file recovery)
- `IMetadataService.cs` / `MetadataService.cs` -- TagLibSharp metadata reading (title, artist, album, duration, album art, audio properties)
- `TrackMetadata.cs` -- Metadata POCO
- `AudioProperties.cs` -- Audio properties POCO (bit rate, sample rate, channels)
- `ScanResult.cs` -- Library scan result POCO (tracks added/updated/removed, errors)

## Current Status

**Status**: Complete (v1) | **Coverage**: 86.26% | **Tests**: see FSMP.Tests

Coverage improved from 65.74% to 86.26% via adapter pattern refactor (IMediaPlayerAdapter) enabling unit testing of LibVlcAudioPlayer without LibVLC runtime.

---

## Upcoming Work

### Playlist Models (Playlist + Music Player Feature)

- [x] Create `Models/Playlist.cs` — PlaylistId, Name, Description, CreatedAt, UpdatedAt, PlaylistTracks collection
- [x] Create `Models/PlaylistTrack.cs` — PlaylistTrackId, PlaylistId (FK), TrackId (FK), Position, AddedAt
- [x] Add `PlaylistTracks` nav collection to `Track.cs`

---

## Future Work

### Coverage Improvement

- [x] Improve coverage from 65.74% toward 80%+ (achieved 86.26%)
- [x] Extract IMediaPlayerAdapter to decouple LibVLC from business logic
- [x] Add 52 unit tests for LibVlcAudioPlayer and LibVlcAudioPlayerFactory
- [ ] Add integration tests for LibVlcMediaPlayerAdapter (requires LibVLC runtime, optional)

### Cross-Platform Migration (when Phase 3 begins)

- [ ] Move models from `FsmpLibrary/Models/` to `FSMP.Core/Models/`
- [ ] Move audio interfaces from `Interfaces/` to `FSMP.Core/Interfaces/`
- [ ] Refactor AudioService to use platform-agnostic IAudioPlayer
- [ ] Remove LibVLC dependency (move to FSMP.Platform.Windows)

---

## Progress Summary

**Status**: Complete (v1), coverage improvement done (86.26%)
**Next Action**: Cross-platform migration Phase 3 or FSMO
