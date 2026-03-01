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

**Services/** (11 files, ~212 tests):
- `AudioServiceTests.cs`, `ConfigurationServiceTests.cs`, `MetadataServiceTests.cs`
- `LibraryScanServiceTests.cs`, `PlaybackTrackingServiceTests.cs`, `StatisticsServiceTests.cs`
- `PlaylistServiceTests.cs` (37 tests)
- `PlaybackControllerTests.cs` (30 tests — split ToggleRepeatMode into 3, mocked IActivePlaylistService), `LibraryBrowserTests.cs` (12 tests)
- `PlaylistManagerTests.cs` (11 tests — mocked IActivePlaylistService), `LibraryManagerTests.cs` (10 tests)

**Repositories/** (7 files, ~70 tests):
- `RepositoryTests.cs`, `TrackRepositoryTests.cs`, `AlbumRepositoryTests.cs`
- `ArtistRepositoryTests.cs`, `PlaybackHistoryRepositoryTests.cs`, `UnitOfWorkTests.cs`
- `PlaylistRepositoryTests.cs` (14 tests)

**Database/** (3 files, ~48 tests):
- `FsmpDbContextTests.cs`, `EntityConfigurationTests.cs` (20 tests incl. Playlist/PlaylistTrack config), `MigrationTests.cs`

**UI/** (7 files, ~214 tests):
- `MenuSystemTests.cs`, `BrowseUITests.cs`, `PlayerUITests.cs` (48 tests)
- `MetadataEditorTests.cs`, `StatisticsViewerTests.cs`
- `PrintTests.cs` (incl. 13 NewDisplay tests), `AppStartupTests.cs`

> Removed: `PlaybackUITests.cs` and `LibraryManagerTests.cs` (dead code — classes superseded by PlayerUI)

**Audio/** (2 files, 51 tests):
- `LibVlcAudioPlayerTests.cs` (49 tests) -- Constructor, properties, LoadAsync, PlayAsync, PauseAsync, StopAsync, SeekAsync, event handlers, state machine, dispose (removed redundant multi-method dispose test)
- `LibVlcAudioPlayerFactoryTests.cs` (2 tests) -- Factory with adapter injection

**Core/** (1 file, 37 tests):
- `ActivePlaylistServiceTests.cs` -- RepeatMode, shuffle, queue navigation

**Integration/** (1 file, 12 tests):
- `EndToEndTests.cs` -- Full-stack E2E workflows with real SQLite

**ErrorHandling/** (1 file, 18 tests):
- `ErrorHandlingTests.cs` -- Corrupt files, missing paths, recovery scenarios

## Current Status

**Status**: Complete (v1) + Playlist feature + Coverage improvement + FSMO tests + Player bug fixes + Orchestration refactor + Test isolation audit | **Tests**: 864 passing | **Overall Coverage**: 93.99%

| Project | Coverage |
|---------|----------|
| FsmpConsole | 80.37% |
| FsmpDataAcsses | 98.46% |
| FsmpLibrary | 95.43% |
| FSMP.Core | 99.53% |
| FSMO | 96.39% |

---

## Future Work

### FSMO Tests

- [x] Create `FSMP.Tests/FSMO/` directory
- [x] `FsmoReferenceTests.cs` -- placeholder test verifying FSMO project reference (1 test)
- [x] `FileSystemTests.cs` -- 12 tests covering all FileSystem helper methods (create/delete/move for files and directories)
- [x] `AudioFileScannerTests.cs` -- 21 tests covering ScanDirectory and IsSupportedFormat (mp3/wav/wma, case-insensitive, recursion, validation)
- [x] `AudioMetadataTests.cs` -- 7 tests covering POCO property defaults and setters
- [x] `MetadataReaderTests.cs` -- 8 tests covering TagLibSharp metadata extraction (MP3, WMA, WAV no-tags, corrupt file, validation)
- [x] `PathBuilderTests.cs` -- 13 tests covering path building, fallbacks, sanitization, validation
- [x] `FileOrganizerTests.cs` -- 21 tests covering copy/move modes, duplicate handling (skip/overwrite/rename), directory creation/cleanup, validation
- [x] `DirectoryManagerTests.cs` -- 8 tests covering ReorganiseDirectory and GetAllDistinctAudioFiles
- [x] `EdgeCaseTests.cs` -- 9 tests covering long path truncation, read-only files, corrupt files, special characters, empty metadata

### Coverage Improvement

- [x] Improve FsmpLibrary coverage from 65.74% toward 80%+ (achieved 86.26%)
- [x] Add LibVlcAudioPlayer unit tests via IMediaPlayerAdapter mock (52 tests)
- [ ] Add LibVlcMediaPlayerAdapter integration tests (requires LibVLC runtime, optional)

### Console UI Restructure Tests

- [x] Update `PlayerUITests.cs` — Add tests for B/L/D/X hotkeys, sub-screen launches, exit behavior
- [x] Update pause/resume tests from Space to K key
- [x] Add skip-to-track tests (4 tests: valid jump, zero, out-of-range, empty queue)
- [x] MenuSystemTests.cs — Already tests thin-wrapper behavior (launches PlayerUI, exits with X)
- [x] PrintTests.cs — Updated with [L] Playlists and [D] Directories hotkey assertions
- [x] Verify all existing tests still pass after refactor

### Orchestration Service Refactor Tests

- [x] `PlaybackControllerTests.cs` — 28 tests (constructor null guards, play/stop/next/prev/restart, repeat/shuffle, jump, queue items, auto-advance)
- [x] `LibraryBrowserTests.cs` — 12 tests (all methods + error paths)
- [x] `PlaylistManagerTests.cs` — 11 tests (CRUD + load into queue)
- [x] `LibraryManagerTests.cs` — 10 tests (config, add/remove path, scan)
- [x] Rewrote `PlayerUITests.cs` — Mocks orchestration interfaces instead of raw services
- [x] Rewrote `BrowseUITests.cs` — Mocks ILibraryBrowser + IPlaybackController
- [x] Rewrote `MenuSystemTests.cs` — Mocks 4 orchestration interfaces

### Cross-Platform Migration Tests (when migration begins)

- [x] Create `FSMP.Tests/Core/` directory for FSMP.Core tests
- [ ] Create `FSMP.Tests/Platform.Windows/` for Windows-specific tests
- [ ] Create `FSMP.Tests/Platform.Android/` for Android-specific tests
- [ ] Expand EndToEndTests.cs for cross-platform scenarios

---

## Progress Summary

**Status**: Complete (v1) + Playlist feature + Audio coverage + FSMO tests + Orchestration refactor tests (61 new tests)
**Next Action**: Cross-platform migration tests when ready