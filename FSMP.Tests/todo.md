# FSMP.Tests - Test Suite

Comprehensive test project covering all FSMP solution projects using xUnit, Moq, FluentAssertions, and Coverlet.

## Completed Work Summary

### Test Infrastructure

- xUnit 2.9.3, Moq 4.20.72, FluentAssertions 8.8.0, Coverlet
- Microsoft.EntityFrameworkCore.InMemory 9.0.0 for database testing
- `coverlet.runsettings` with 80% coverage threshold
- Helper scripts: `build.cmd`, `test.cmd`, `test-with-coverage.cmd`

### Test Helpers (`TestHelpers/`)

- `MockAudioPlayer.cs` -- IAudioPlayer mock implementation for unit testing
- `MockAudioPlayerFactory.cs` -- IAudioPlayerFactory mock implementation
- `MockMediaPlayerAdapter.cs` -- IMediaPlayerAdapter mock for LibVlcAudioPlayer unit testing

### Test Files by Directory

**Models/** (8 files, ~102 tests):
- `TrackTests.cs`, `AlbumTests.cs`, `ArtistTests.cs`
- `PlaybackHistoryTests.cs`, `LibraryPathTests.cs`, `ConfigurationTests.cs`
- `PlaylistTests.cs`, `PlaylistTrackTests.cs`

**Services/** (7 files, ~151 tests):
- `AudioServiceTests.cs`, `ConfigurationServiceTests.cs`, `MetadataServiceTests.cs`
- `LibraryScanServiceTests.cs`, `PlaybackTrackingServiceTests.cs`, `StatisticsServiceTests.cs`
- `PlaylistServiceTests.cs` (37 tests)

**Repositories/** (7 files, ~70 tests):
- `RepositoryTests.cs`, `TrackRepositoryTests.cs`, `AlbumRepositoryTests.cs`
- `ArtistRepositoryTests.cs`, `PlaybackHistoryRepositoryTests.cs`, `UnitOfWorkTests.cs`
- `PlaylistRepositoryTests.cs` (14 tests)

**Database/** (3 files, ~48 tests):
- `FsmpDbContextTests.cs`, `EntityConfigurationTests.cs` (20 tests incl. Playlist/PlaylistTrack config), `MigrationTests.cs`

**UI/** (9 files, ~241 tests):
- `MenuSystemTests.cs`, `BrowseUITests.cs`, `PlaybackUITests.cs`, `PlayerUITests.cs` (40 tests)
- `MetadataEditorTests.cs`, `LibraryManagerTests.cs`, `StatisticsViewerTests.cs`
- `PrintTests.cs` (incl. 13 NewDisplay tests), `AppStartupTests.cs`

**Audio/** (2 files, 52 tests):
- `LibVlcAudioPlayerTests.cs` (50 tests) -- Constructor, properties, LoadAsync, PlayAsync, PauseAsync, StopAsync, SeekAsync, event handlers, state machine, dispose
- `LibVlcAudioPlayerFactoryTests.cs` (2 tests) -- Factory with adapter injection

**Core/** (1 file, 37 tests):
- `ActivePlaylistServiceTests.cs` -- RepeatMode, shuffle, queue navigation

**Integration/** (1 file, 12 tests):
- `EndToEndTests.cs` -- Full-stack E2E workflows with real SQLite

**ErrorHandling/** (1 file, 18 tests):
- `ErrorHandlingTests.cs` -- Corrupt files, missing paths, recovery scenarios

## Current Status

**Status**: Complete (v1) + Playlist feature + Coverage improvement | **Tests**: 746 passing | **Overall Coverage**: 96.43%

| Project | Coverage |
|---------|----------|
| FsmpConsole | 95.60% |
| FsmpDataAcsses | 98.52% |
| FsmpLibrary | 86.26% |
| FSMP.Core | 100% |

---

## Future Work

### FSMO Tests (when FSMO implementation begins)

- [ ] Create `FSMP.Tests/FSMO/` directory
- [ ] Tests will be tracked in the [FSMO todo](../FSMP.lib/FSMO/todo.md) alongside their implementation slices

### Coverage Improvement

- [x] Improve FsmpLibrary coverage from 65.74% toward 80%+ (achieved 86.26%)
- [x] Add LibVlcAudioPlayer unit tests via IMediaPlayerAdapter mock (52 tests)
- [ ] Add LibVlcMediaPlayerAdapter integration tests (requires LibVLC runtime, optional)

### Cross-Platform Migration Tests (when migration begins)

- [x] Create `FSMP.Tests/Core/` directory for FSMP.Core tests
- [ ] Create `FSMP.Tests/Platform.Windows/` for Windows-specific tests
- [ ] Create `FSMP.Tests/Platform.Android/` for Android-specific tests
- [ ] Expand EndToEndTests.cs for cross-platform scenarios

---

## Progress Summary

**Status**: Complete (v1) + Playlist feature + Audio coverage improvement (52 new tests)
**Next Action**: FSMO tests (when FSMO implementation begins)
