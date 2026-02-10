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

LibVLCSharp-based audio playback (replaced WMPLib COM interop):

- `LibVlcAudioPlayer.cs` -- IAudioPlayer implementation using LibVLCSharp
- `LibVlcAudioPlayerFactory.cs` -- IAudioPlayerFactory implementation

### Services (`Services/`)

- `IAudioService.cs` / `AudioService.cs` -- High-level playback orchestration (play track/file, volume, pause/resume/stop/seek)
- `ConfigurationService.cs` -- JSON config file management (load/save/add path/remove path, corrupt file recovery)
- `IMetadataService.cs` / `MetadataService.cs` -- TagLibSharp metadata reading (title, artist, album, duration, album art, audio properties)
- `TrackMetadata.cs` -- Metadata POCO
- `AudioProperties.cs` -- Audio properties POCO (bit rate, sample rate, channels)
- `ScanResult.cs` -- Library scan result POCO (tracks added/updated/removed, errors)

## Current Status

**Status**: Complete (v1) | **Coverage**: 65.74% | **Tests**: see FSMP.Tests

Coverage is lower due to untested `LibVlcAudioPlayer.cs` and `LibVlcAudioPlayerFactory.cs` (require real LibVLC runtime).

---

## Future Work

### Coverage Improvement

- [ ] Improve coverage from 65.74% toward 80%+
- [ ] Add integration tests for LibVlcAudioPlayer (requires LibVLC runtime)
- [ ] Consider mocking LibVLC for unit-testable coverage

### Cross-Platform Migration (when Phase 3 begins)

- [ ] Move models from `FsmpLibrary/Models/` to `FSMP.Core/Models/`
- [ ] Move audio interfaces from `Interfaces/` to `FSMP.Core/Interfaces/`
- [ ] Refactor AudioService to use platform-agnostic IAudioPlayer
- [ ] Remove LibVLC dependency (move to FSMP.Platform.Windows)

---

## Progress Summary

**Status**: Complete (v1), pending coverage improvement
**Next Action**: Coverage improvement or cross-platform migration Phase 3
