# FSMP Enhancement Implementation - Vertical Slices

**Progress Tracking**: Each slice is a complete vertical feature (model → tests → repository → service → UI)

---

## ✅ Slice 1: Testing Infrastructure (COMPLETE)

**What it delivers**: Complete xUnit test project with 80% coverage requirement

- [x] Create FSMP.Tests project with xUnit, Moq, FluentAssertions
- [x] Add Coverlet for code coverage
- [x] Configure coverlet.runsettings with 80% threshold
- [x] Create test directory structure (Models/, Services/, Repositories/, UI/)
- [x] Add Microsoft.EntityFrameworkCore.InMemory for database testing
- [x] Create build.cmd helper script (MSBuild for COM interop)
- [x] Create test.cmd helper script
- [x] Create test-with-coverage.cmd helper script
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ Pass (0 tests initially)

---

## ✅ Slice 2: Track Model Complete (COMPLETE)

**What it delivers**: Fully tested Track entity model with all properties and display logic

- [x] Install NuGet packages (EF Core, TagLibSharp, System.Text.Json, InMemory)
- [x] Create Track.cs with all properties (Title, FilePath, CustomTitle, PlayCount, Rating, etc.)
- [x] Create TrackTests.cs with comprehensive tests
  - [x] Test default initialization
  - [x] Test all property setters
  - [x] Test DisplayTitle (custom vs file metadata)
  - [x] Test DisplayArtist (custom vs file metadata)
  - [x] Test DisplayAlbum (custom vs file metadata)
  - [x] Test Rating validation
  - [x] Test navigation properties
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 17/17 passing
- [x] **Coverage**: ✅ ≥80%

---

## ✅ Slice 2a: Track — IsExplicit Flag (COMPLETE)

**What it delivers**: Explicit-content boolean on Track

- [x] Add `IsExplicit` bool property to Track.cs (after SampleRate, defaults false)
- [x] Update TrackTests.cs — default assertion + set-all assertion
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 17/17 passing

---

## ✅ Slice 2b: Genre Lookup Model (COMPLETE)

**What it delivers**: Genre as a DB lookup entity with many-to-many relationships to Album, Track, and Artist. New genres can be added by inserting a row — no code or migration change required.

- [x] Create Genre.cs — GenreId (PK), Name; back-references to Album, Track, Artist
- [x] Album.cs — removed `string? Genre`, added `ICollection<Genre> Genres`
- [x] Track.cs — removed `string? CustomGenre`, added `ICollection<Genre> Genres`
- [x] Artist.cs — added `ICollection<Genre> Genres`
- [x] TrackTests.cs — replaced CustomGenre assertions with Genres equivalents
- [x] Seed values planned: Rock, Jazz, Classic, Metal, Comedy (wire into DbContext in Slice 7)
- [x] EF Core will create implicit junction tables (AlbumGenre, TrackGenre, ArtistGenre) when DbContext is configured
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 17/17 passing

---

## ✅ Slice 2c: FileExtension Lookup Model (COMPLETE)

**What it delivers**: FileExtension as a DB lookup entity replacing the free-text `Track.FileFormat` string. Seed values (`wav`, `wma`, `mp3`) wired into DbContext in Slice 7 alongside Genre.

- [x] Create FileExtension.cs — FileExtensionId (PK), Extension; back-reference to Track
- [x] Track.cs — removed `string FileFormat`, added `int? FileExtensionId` (FK) + `FileExtension?` nav property
- [x] TrackTests.cs — updated 4 FileFormat references to use new FK + nav property
- [x] Seed values planned: wav, wma, mp3 (wire into DbContext in Slice 7)
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 17/17 passing

---

## ✅ Slice 2d: DI Infrastructure (COMPLETE)

**What it delivers**: Dependency injection container in the console app. Establishes the pattern that DbContext, UoW, and future services register into.

- [x] Create `IAudioService` interface in `FsmpLibrary/Services/`
- [x] Create `AudioService` implementation — thin wrapper delegating to static `Fsmp` class
- [x] Add `Microsoft.Extensions.DependencyInjection 9.0.0` to `FsmpConsole.csproj`
- [x] Rewrite `Program.cs` — `ServiceCollection` → resolve `IAudioService` → main loop
- [x] Create `data-access-checklist.md` at project root (standalone startup guide for Slice 7+)
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 17/17 passing

---

## ✅ Slice 2e: LibVLCSharp Audio Migration (COMPLETE)

**What it delivers**: Cross-platform audio playback via LibVLCSharp replacing WMPLib COM interop and System.Media.SoundPlayer.

- [x] Add LibVLCSharp 3.9.5 and VideoLAN.LibVLC.Windows 3.0.21 NuGet packages
- [x] Create `IAudioPlayer` interface with Load/Play/Pause/Stop/Seek + events
- [x] Create `IAudioPlayerFactory` interface for DI
- [x] Create `PlaybackState` enum and event args classes
- [x] Implement `LibVlcAudioPlayer` using LibVLCSharp
- [x] Implement `LibVlcAudioPlayerFactory`
- [x] Expand `IAudioService` with PlayTrackAsync, PlayFileAsync, Volume, etc.
- [x] Refactor `AudioService` to use IAudioPlayerFactory via constructor injection
- [x] Update Program.cs DI registration with factory pattern
- [x] Remove WMPLib COM reference from FsmpLibrary.csproj
- [x] Remove System.Windows.Extensions package
- [x] Mark legacy `Fsmp` static class as `[Obsolete]`
- [x] Create `MockAudioPlayer` and `MockAudioPlayerFactory` test helpers
- [x] Create `MockAudioPlayerTests.cs` (14 tests)
- [x] Create `AudioServiceTests.cs` (20 tests)
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 51/51 passing

**Key files created:**
- `Interfaces/IAudioPlayer.cs`, `IAudioPlayerFactory.cs`, `PlaybackState.cs`
- `Interfaces/EventArgs/*.cs` (4 event args classes)
- `Audio/LibVlcAudioPlayer.cs`, `LibVlcAudioPlayerFactory.cs`
- `FSMP.Tests/TestHelpers/MockAudioPlayer.cs`, `MockAudioPlayerFactory.cs`
- `FSMP.Tests/Audio/MockAudioPlayerTests.cs`
- `FSMP.Tests/Services/AudioServiceTests.cs`

---

## 🔄 Slice 3: Album Model Complete

**What it delivers**: Fully tested Album entity model with artist relationships

- [x] Create Album.cs with all properties (Title, Year, AlbumArt, etc.)
- [x] Genre changed from `string?` to `ICollection<Genre>` (done in Slice 2b)
- [x] Enhance Album.cs with missing fields from plan:
  - [x] Add AlbumArtistName property
  - [x] Add AlbumArtPath property
  - [x] Add CreatedAt property
  - [x] Add UpdatedAt property
- [ ] Create AlbumTests.cs with comprehensive tests
  - [ ] Test default initialization
  - [ ] Test all property setters
  - [ ] Test navigation to Artist (nullable)
  - [ ] Test navigation to Tracks collection
  - [ ] Test AlbumArt byte array storage
  - [ ] Test Year nullable behavior
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All Album tests passing
- [ ] **Coverage**: ≥80%

---

## Slice 4: Artist Model Complete

**What it delivers**: Fully tested Artist entity model with album/track relationships

- [x] Create Artist.cs with basic properties
- [x] Added `ICollection<Genre> Genres` nav property (done in Slice 2b)
- [x] Enhance Artist.cs with missing fields from plan:
  - [x] Add Biography property
  - [x] Add CreatedAt property
  - [x] Add UpdatedAt property
- [ ] Create ArtistTests.cs with comprehensive tests
  - [ ] Test default initialization
  - [ ] Test all property setters
  - [ ] Test SortName nullable behavior
  - [ ] Test navigation to Albums collection
  - [ ] Test navigation to Tracks collection
  - [ ] Test navigation to Genres collection
  - [ ] Test Biography storage
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All Artist tests passing
- [ ] **Coverage**: ≥80%

---

## Slice 5: PlaybackHistory Model Complete

**What it delivers**: Fully tested PlaybackHistory entity for tracking plays

- [x] Create PlaybackHistory.cs with basic properties
- [ ] Create PlaybackHistoryTests.cs with comprehensive tests
  - [ ] Test default initialization
  - [ ] Test all property setters
  - [ ] Test PlayedAt timestamp
  - [ ] Test PlayDuration nullable behavior
  - [ ] Test CompletedPlayback flag
  - [ ] Test WasSkipped flag
  - [ ] Test navigation to Track (non-null)
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All PlaybackHistory tests passing
- [ ] **Coverage**: ≥80%

---

## Slice 6: Configuration Models Complete

**What it delivers**: Fully tested LibraryPath and Configuration entities

- [ ] Create LibraryPath.cs in Models/
  - [ ] LibraryPathId (PK)
  - [ ] Path (string, unique)
  - [ ] IsActive (bool)
  - [ ] AddedAt (DateTime)
  - [ ] LastScannedAt (DateTime?)
  - [ ] TrackCount (int, cached)
- [ ] Create LibraryPathTests.cs
  - [ ] Test default initialization
  - [ ] Test all property setters
  - [ ] Test LastScannedAt nullable
- [ ] Create Configuration.cs in Models/
  - [ ] LibraryPaths (List<string>)
  - [ ] DatabasePath (string)
  - [ ] AutoScanOnStartup (bool)
  - [ ] DefaultVolume (int)
  - [ ] RememberLastPlayed (bool)
  - [ ] LastPlayedTrackPath (string?)
- [ ] Create ConfigurationTests.cs
  - [ ] Test default initialization
  - [ ] Test LibraryPaths collection initialization
  - [ ] Test all property setters
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All Configuration model tests passing
- [ ] **Coverage**: ≥80%

---

## Slice 7: Database Context Foundation

**What it delivers**: Working FsmpDbContext with all entity relationships configured

- [ ] Create FsmpDbContext.cs in FsmpDataAcsses/
  - [ ] Add DbSet<Track> Tracks
  - [ ] Add DbSet<Album> Albums
  - [ ] Add DbSet<Artist> Artists
  - [ ] Add DbSet<Genre> Genres
  - [ ] Add DbSet<PlaybackHistory> PlaybackHistories
  - [ ] Add DbSet<LibraryPath> LibraryPaths
  - [ ] Add constructor with DbContextOptions
  - [ ] Seed Genre lookup rows: Rock, Jazz, Classic, Metal, Comedy
  - [ ] Seed FileExtension lookup rows: wav, wma, mp3
- [ ] Create FsmpDbContextTests.cs
  - [ ] Test all DbSets are not null
  - [ ] Test in-memory database creation
  - [ ] Test can add and retrieve entities
  - [ ] Test Genre seed data present after migration
- [ ] Add OnModelCreating configuration
  - [ ] Configure Track: FilePath unique index, FileHash index, relationships
  - [ ] Configure Album: relationship to Artist, relationship to Tracks, many-to-many to Genre
  - [ ] Configure Artist: indexes on Name, relationships, many-to-many to Genre
  - [ ] Configure Track: many-to-many to Genre (implicit junction table TrackGenre)
  - [ ] Configure PlaybackHistory: cascade delete, relationship to Track
  - [ ] Configure LibraryPath: unique Path constraint
- [ ] Create EntityConfigurationTests.cs
  - [ ] Test Track.FilePath unique constraint
  - [ ] Test Album.Artist relationship (nullable)
  - [ ] Test Track.Album relationship (nullable)
  - [ ] Test PlaybackHistory.Track cascade delete
  - [ ] Test LibraryPath.Path unique constraint
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All DbContext tests passing
- [ ] **Coverage**: ≥80%

---

## Slice 8: Repository Pattern Base

**What it delivers**: Generic repository pattern foundation for all entities

- [ ] Create IRepository<T>.cs interface in Repositories/
  - [ ] Task<T?> GetByIdAsync(int id)
  - [ ] Task<IEnumerable<T>> GetAllAsync()
  - [ ] Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
  - [ ] Task AddAsync(T entity)
  - [ ] Task AddRangeAsync(IEnumerable<T> entities)
  - [ ] void Update(T entity)
  - [ ] void Remove(T entity)
  - [ ] Task<int> CountAsync()
- [ ] Create Repository<T>.cs base implementation in Repositories/
  - [ ] Implement all IRepository<T> methods using DbContext
- [ ] Create RepositoryTests.cs
  - [ ] Test GetByIdAsync with in-memory database
  - [ ] Test GetAllAsync returns all entities
  - [ ] Test FindAsync with predicate filters correctly
  - [ ] Test AddAsync adds entity
  - [ ] Test AddRangeAsync adds multiple entities
  - [ ] Test Update modifies entity
  - [ ] Test Remove deletes entity
  - [ ] Test CountAsync returns correct count
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All Repository tests passing
- [ ] **Coverage**: ≥80%

---

## Slice 9: Track Repository Specialized

**What it delivers**: TrackRepository with specialized queries for tracks

- [ ] Create TrackRepository.cs in Repositories/
  - [ ] Inherit from Repository<Track>
  - [ ] Task<Track?> GetByFilePathAsync(string filePath)
  - [ ] Task<IEnumerable<Track>> GetFavoritesAsync()
  - [ ] Task<IEnumerable<Track>> GetMostPlayedAsync(int count)
  - [ ] Task<IEnumerable<Track>> GetRecentlyPlayedAsync(int count)
  - [ ] Task<Track?> GetByFileHashAsync(string fileHash)
- [ ] Create TrackRepositoryTests.cs
  - [ ] Test GetByFilePathAsync finds by path
  - [ ] Test GetFavoritesAsync filters IsFavorite=true
  - [ ] Test GetMostPlayedAsync orders by PlayCount DESC
  - [ ] Test GetRecentlyPlayedAsync orders by LastPlayedAt DESC
  - [ ] Test GetByFileHashAsync finds by hash (deduplication)
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All TrackRepository tests passing
- [ ] **Coverage**: ≥80%

---

## Slice 10: Album & Artist Repositories

**What it delivers**: AlbumRepository and ArtistRepository with specialized queries

- [ ] Create AlbumRepository.cs in Repositories/
  - [ ] Inherit from Repository<Album>
  - [ ] Task<IEnumerable<Album>> GetByArtistAsync(int artistId)
  - [ ] Task<IEnumerable<Album>> GetByYearAsync(int year)
  - [ ] Task<Album?> GetWithTracksAsync(int albumId)
- [ ] Create AlbumRepositoryTests.cs
  - [ ] Test GetByArtistAsync filters by ArtistId
  - [ ] Test GetByYearAsync filters by Year
  - [ ] Test GetWithTracksAsync includes Tracks navigation
- [ ] Create ArtistRepository.cs in Repositories/
  - [ ] Inherit from Repository<Artist>
  - [ ] Task<Artist?> GetWithAlbumsAsync(int artistId)
  - [ ] Task<Artist?> GetWithTracksAsync(int artistId)
  - [ ] Task<IEnumerable<Artist>> SearchAsync(string searchTerm)
- [ ] Create ArtistRepositoryTests.cs
  - [ ] Test GetWithAlbumsAsync includes Albums navigation
  - [ ] Test GetWithTracksAsync includes Tracks navigation
  - [ ] Test SearchAsync filters by name containing searchTerm
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All Album & Artist repository tests passing
- [ ] **Coverage**: ≥80%

---

## Slice 11: PlaybackHistory Repository & Unit of Work

**What it delivers**: Complete repository pattern with Unit of Work coordinator

- [ ] Create PlaybackHistoryRepository.cs in Repositories/
  - [ ] Inherit from Repository<PlaybackHistory>
  - [ ] Task<IEnumerable<PlaybackHistory>> GetRecentPlaysAsync(int count)
  - [ ] Task<IEnumerable<PlaybackHistory>> GetByTrackAsync(int trackId)
  - [ ] Task<int> GetTotalPlayCountAsync()
  - [ ] Task<TimeSpan> GetTotalListeningTimeAsync()
- [ ] Create PlaybackHistoryRepositoryTests.cs
  - [ ] Test GetRecentPlaysAsync orders by PlayedAt DESC
  - [ ] Test GetByTrackAsync filters by TrackId
  - [ ] Test GetTotalPlayCountAsync sums all plays
  - [ ] Test GetTotalListeningTimeAsync sums PlayDuration
- [ ] Create UnitOfWork.cs in FsmpDataAcsses/
  - [ ] TrackRepository Tracks { get; }
  - [ ] AlbumRepository Albums { get; }
  - [ ] ArtistRepository Artists { get; }
  - [ ] PlaybackHistoryRepository PlaybackHistories { get; }
  - [ ] Repository<LibraryPath> LibraryPaths { get; }
  - [ ] Task<int> SaveAsync()
  - [ ] Implement IDisposable
- [ ] Create UnitOfWorkTests.cs
  - [ ] Test all repository properties initialized
  - [ ] Test SaveAsync commits changes to database
  - [ ] Test Dispose releases DbContext
  - [ ] Test transaction rollback on exception
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All repository & UnitOfWork tests passing
- [ ] **Coverage**: ≥80%

---

## Slice 12: Initial Database Migration 🎉

**What it delivers**: Working SQLite database creation via EF Core migration

**Checkpoint**: Database file appears at %AppData%\FSMP\fsmp.db (first tangible artifact!)

- [ ] Run EF Core migration command:
  ```
  dotnet ef migrations add InitialCreate --project FSMP.db/entity/FsmpDataAcsses/FsmpDataAcsses --startup-project FSMP.UI/FSMP.UI.Console/FsmpConsole/FsmpConsole
  ```
- [ ] Verify migration file created in Migrations/ folder
- [ ] Review generated migration code for correctness
- [ ] Create MigrationTests.cs in Tests/Database/
  - [ ] Test migration applies successfully to SQLite database
  - [ ] Test all 5 tables created (Artists, Albums, Tracks, PlaybackHistories, LibraryPaths)
  - [ ] Test Track.FilePath unique index exists
  - [ ] Test Track.FileHash index exists
  - [ ] Test Album-Artist relationship created
  - [ ] Test PlaybackHistory-Track cascade delete configured
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All migration tests passing
- [ ] **Coverage**: ≥80%
- [ ] **Manual Verification**: Run migration, verify .db file exists and contains tables

---

## Slice 13: Configuration Service (End-to-End) 🎉

**What it delivers**: JSON configuration file management

**Checkpoint**: Config file created at %AppData%\FSMP\config.json (second tangible artifact!)

- [ ] Create ConfigurationService.cs in Services/
  - [ ] Constructor(string configPath)
  - [ ] Task<Configuration> LoadConfigurationAsync()
  - [ ] Task SaveConfigurationAsync(Configuration config)
  - [ ] Task AddLibraryPathAsync(string path)
  - [ ] Task RemoveLibraryPathAsync(string path)
  - [ ] Configuration GetDefaultConfiguration()
  - [ ] Create %AppData%\FSMP\ directory if missing
- [ ] Create ConfigurationServiceTests.cs
  - [ ] Test GetDefaultConfiguration returns valid defaults
  - [ ] Test LoadConfigurationAsync creates default if missing
  - [ ] Test SaveConfigurationAsync writes valid JSON
  - [ ] Test LoadConfigurationAsync reads saved JSON correctly
  - [ ] Test AddLibraryPathAsync updates config
  - [ ] Test RemoveLibraryPathAsync updates config
  - [ ] Test configuration file location is %AppData%/FSMP/config.json
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All ConfigurationService tests passing
- [ ] **Coverage**: ≥80%
- [ ] **Manual Verification**: Run service, verify config.json created with default paths

---

## Slice 14: Metadata Reading Service 🎉

**What it delivers**: Read metadata from WAV/WMA/MP3 files using TagLibSharp

**Checkpoint**: Can extract title, artist, album, duration, album art from sample files

- [ ] Create TrackMetadata.cs POCO in Services/
  - [ ] Properties: Title, Artist, Album, Year, Genre, Duration, BitRate, SampleRate, AlbumArt
- [ ] Create MetadataService.cs in Services/
  - [ ] Constructor()
  - [ ] TrackMetadata ReadMetadata(string filePath)
  - [ ] byte[]? ExtractAlbumArt(string filePath)
  - [ ] TimeSpan? GetDuration(string filePath)
  - [ ] AudioProperties GetAudioProperties(string filePath) (BitRate, SampleRate)
  - [ ] Handle TagLib exceptions (corrupt files, unsupported formats)
- [ ] Create MetadataServiceTests.cs
  - [ ] Copy sample audio files to test resources (WAV, WMA, MP3)
  - [ ] Test ReadMetadata extracts title from WAV
  - [ ] Test ReadMetadata extracts artist from WMA
  - [ ] Test ReadMetadata extracts album from MP3
  - [ ] Test ExtractAlbumArt returns byte[] for file with art
  - [ ] Test GetDuration returns correct TimeSpan
  - [ ] Test GetAudioProperties returns BitRate and SampleRate
  - [ ] Test error handling with corrupt file (returns null/defaults)
  - [ ] Test error handling with missing file (throws)
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All MetadataService tests passing
- [ ] **Coverage**: ≥80%
- [ ] **Manual Verification**: Run on sample music files, verify metadata extracted correctly

---

## Slice 15: Library Scanning (End-to-End) 🎉

**What it delivers**: Scan directories and import tracks into database

**Checkpoint**: Sample music directory scanned, tracks appear in database with metadata

- [ ] Create ScanResult.cs in Services/
  - [ ] int TracksAdded
  - [ ] int TracksUpdated
  - [ ] int TracksRemoved
  - [ ] TimeSpan Duration
  - [ ] List<string> Errors
- [ ] Create LibraryScanService.cs in Services/
  - [ ] Constructor(UnitOfWork unitOfWork, MetadataService metadataService)
  - [ ] Task<ScanResult> ScanAllLibrariesAsync(List<string> libraryPaths)
  - [ ] Task<ScanResult> ScanLibraryAsync(string libraryPath)
  - [ ] Task<Track?> ImportTrackAsync(FileInfo fileInfo)
  - [ ] string CalculateFileHash(string filePath) (SHA256)
  - [ ] bool IsSupportedFormat(string extension) (.wav, .wma, .mp3)
  - [ ] Handle duplicate detection via FileHash
  - [ ] Create or update Artist and Album entities
- [ ] Create LibraryScanServiceTests.cs
  - [ ] Create test directory structure with sample audio files
  - [ ] Test ScanLibraryAsync imports WAV files
  - [ ] Test ScanLibraryAsync imports WMA files
  - [ ] Test ScanLibraryAsync imports MP3 files
  - [ ] Test CalculateFileHash returns consistent SHA256 hash
  - [ ] Test IsSupportedFormat filters .wav, .wma, .mp3
  - [ ] Test duplicate detection skips files with same hash
  - [ ] Test error handling with corrupt files (logs error, continues)
  - [ ] Test ScanResult aggregates counts correctly
  - [ ] Test Artist/Album auto-creation from metadata
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All LibraryScanService tests passing
- [ ] **Coverage**: ≥80%
- [ ] **Manual Verification**: Scan sample music folder, query database, verify tracks exist

---

## Slice 16: MP3 Playback Support 🎉

**What it delivers**: MP3 files can be played (currently only WAV/WMA work)

**Checkpoint**: USER-VISIBLE CHANGE - MP3 files in sample music now play!

- [ ] Modify Fsmp.cs in FsmpLibrary/
  - [ ] Add PlayMp3(string mp3Path) method using WindowsMediaPlayer COM
  - [ ] Update CheckFileLocation() to handle .mp3 extension
  - [ ] Add input validation (null checks, file exists)
- [ ] Create FsmpTests.cs in Tests/Services/
  - [ ] Test PlayWav throws ArgumentNullException on null path
  - [ ] Test PlayWma throws ArgumentNullException on null path
  - [ ] Test PlayMp3 throws ArgumentNullException on null path
  - [ ] Test PlayMp3 throws FileNotFoundException on missing file
  - [ ] Test CheckFileLocation handles .wav extension
  - [ ] Test CheckFileLocation handles .wma extension
  - [ ] Test CheckFileLocation handles .mp3 extension
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All Fsmp tests passing
- [ ] **Coverage**: ≥80%
- [ ] **Manual Verification**: Place MP3 file in test directory, verify it plays

---

## Slice 17: Playback Tracking Service 🎉

**What it delivers**: Playback history tracked in database, play counts increment

**Checkpoint**: Every play creates PlaybackHistory record and updates Track statistics

- [ ] Create PlaybackService.cs in Services/
  - [ ] Constructor(UnitOfWork unitOfWork)
  - [ ] Task PlayTrackAsync(Track track)
  - [ ] void Stop()
  - [ ] void Pause()
  - [ ] void Resume()
  - [ ] Task RecordPlaybackAsync(Track track, TimeSpan playDuration, bool completed, bool skipped)
  - [ ] Route to Fsmp.PlayWav/PlayWma/PlayMp3 based on FileFormat
  - [ ] Increment Track.PlayCount on completed playback
  - [ ] Update Track.LastPlayedAt
  - [ ] Increment Track.SkipCount if skipped
- [ ] Create PlaybackServiceTests.cs
  - [ ] Mock UnitOfWork and repositories
  - [ ] Test PlayTrackAsync calls Fsmp.PlayWav for .wav format
  - [ ] Test PlayTrackAsync calls Fsmp.PlayWma for .wma format
  - [ ] Test PlayTrackAsync calls Fsmp.PlayMp3 for .mp3 format
  - [ ] Test RecordPlaybackAsync creates PlaybackHistory record
  - [ ] Test RecordPlaybackAsync increments Track.PlayCount when completed=true
  - [ ] Test RecordPlaybackAsync updates Track.LastPlayedAt
  - [ ] Test RecordPlaybackAsync increments Track.SkipCount when skipped=true
  - [ ] Test RecordPlaybackAsync doesn't increment PlayCount when completed=false
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All PlaybackService tests passing
- [ ] **Coverage**: ≥80%

---

## Slice 18: Statistics Service 🎉

**What it delivers**: Query play counts, favorites, most played, recently played

**Checkpoint**: Can retrieve statistics from database (most played, recently played, etc.)

- [ ] Create StatisticsService.cs in Services/
  - [ ] Constructor(UnitOfWork unitOfWork)
  - [ ] Task<IEnumerable<Track>> GetMostPlayedTracksAsync(int count)
  - [ ] Task<IEnumerable<Track>> GetRecentlyPlayedTracksAsync(int count)
  - [ ] Task<IEnumerable<Track>> GetFavoritesAsync()
  - [ ] Task<Dictionary<string, int>> GetGenreStatisticsAsync()
  - [ ] Task<int> GetTotalPlayCountAsync()
  - [ ] Task<TimeSpan> GetTotalListeningTimeAsync()
  - [ ] Task<int> GetTotalTrackCountAsync()
- [ ] Create StatisticsServiceTests.cs
  - [ ] Mock UnitOfWork with test data (tracks with various play counts)
  - [ ] Test GetMostPlayedTracksAsync returns top N by PlayCount DESC
  - [ ] Test GetRecentlyPlayedTracksAsync returns top N by LastPlayedAt DESC
  - [ ] Test GetFavoritesAsync returns only IsFavorite=true tracks
  - [ ] Test GetGenreStatisticsAsync aggregates counts by genre
  - [ ] Test GetTotalPlayCountAsync sums all Track.PlayCount values
  - [ ] Test GetTotalListeningTimeAsync sums PlaybackHistory.PlayDuration
  - [ ] Test GetTotalTrackCountAsync returns correct count
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All StatisticsService tests passing
- [ ] **Coverage**: ≥80%

---

## Slice 19: Menu System UI 🎉

**What it delivers**: Interactive console menu for navigating application features

**Checkpoint**: Menu displays and accepts user input to navigate features

- [ ] Create MenuSystem.cs in FsmpConsole/
  - [ ] Constructor(UnitOfWork unitOfWork, ConfigurationService configService, PlaybackService playbackService, StatisticsService statsService, LibraryScanService scanService)
  - [ ] Task RunAsync() - main event loop
  - [ ] void DisplayMainMenu()
  - [ ] Menu options: 1) Browse & Play, 2) Edit Metadata, 3) Manage Libraries, 4) Scan Libraries, 5) View Statistics, 6) Settings, 7) Exit
  - [ ] Route to appropriate UI component based on selection
- [ ] Create MenuSystemTests.cs
  - [ ] Mock all service dependencies
  - [ ] Mock console input/output (TextReader/TextWriter)
  - [ ] Test DisplayMainMenu outputs correct menu text
  - [ ] Test option 1 routes to Browse UI
  - [ ] Test option 7 exits loop
  - [ ] Test invalid input displays error and re-prompts
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All MenuSystem tests passing
- [ ] **Coverage**: ≥80%
- [ ] **Manual Verification**: Run menu, verify it displays and accepts input

---

## Slice 20: Browse & Play UI 🎉

**What it delivers**: Navigate artists → albums → tracks, select and play

**Checkpoint**: USER-VISIBLE MAJOR CHANGE - Browse database and play tracks from menu!

- [ ] Create BrowseUI.cs in FsmpConsole/
  - [ ] Constructor(UnitOfWork unitOfWork, PlaybackService playbackService)
  - [ ] Task RunAsync()
  - [ ] Task DisplayArtistsAsync()
  - [ ] Task DisplayAlbumsByArtistAsync(int artistId)
  - [ ] Task DisplayTracksByAlbumAsync(int albumId)
  - [ ] Task PlayTrackAsync(int trackId)
- [ ] Create BrowseUITests.cs
  - [ ] Mock UnitOfWork with test data
  - [ ] Test DisplayArtistsAsync lists all artists
  - [ ] Test DisplayAlbumsByArtistAsync filters by artistId
  - [ ] Test DisplayTracksByAlbumAsync filters by albumId
  - [ ] Test PlayTrackAsync calls PlaybackService.PlayTrackAsync
- [ ] Create PlaybackUI.cs in FsmpConsole/
  - [ ] Constructor()
  - [ ] void DisplayNowPlaying(Track track)
  - [ ] Display: Artist, Album, Title, Duration, BitRate, PlayCount, Rating
  - [ ] void DisplayControls() (Stop, Pause, Next, Toggle Favorite, Edit Metadata)
- [ ] Create PlaybackUITests.cs
  - [ ] Mock console output
  - [ ] Test DisplayNowPlaying formats track info correctly
  - [ ] Test DisplayControls shows control options
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All Browse & Playback UI tests passing
- [ ] **Coverage**: ≥80%
- [ ] **Manual Verification**: Browse artists → albums → tracks, play a track

---

## Slice 21: Metadata Editor UI 🎉

**What it delivers**: Search and edit track metadata from console UI

**Checkpoint**: USER-VISIBLE - Edit track title, artist, album, rating, favorites from UI

- [ ] Create MetadataEditor.cs in FsmpConsole/
  - [ ] Constructor(UnitOfWork unitOfWork)
  - [ ] Task RunAsync()
  - [ ] Task<Track?> SearchTrackAsync() (search by title or artist)
  - [ ] Task DisplayMetadataAsync(Track track)
  - [ ] Task EditMetadataAsync(Track track)
  - [ ] Fields editable: CustomTitle, CustomArtist, CustomAlbum, Rating (1-5), IsFavorite, Comment
  - [ ] Task SaveChangesAsync()
- [ ] Create MetadataEditorTests.cs
  - [ ] Mock UnitOfWork and console I/O
  - [ ] Test SearchTrackAsync returns matching tracks
  - [ ] Test DisplayMetadataAsync shows file metadata + custom overrides
  - [ ] Test EditMetadataAsync updates CustomTitle
  - [ ] Test EditMetadataAsync validates Rating 1-5
  - [ ] Test EditMetadataAsync toggles IsFavorite
  - [ ] Test SaveChangesAsync commits to database
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All MetadataEditor tests passing
- [ ] **Coverage**: ≥80%
- [ ] **Manual Verification**: Search track, edit title, verify change persists in database

---

## Slice 22: Library Manager UI 🎉

**What it delivers**: Add/remove library paths, trigger scans from UI

**Checkpoint**: USER-VISIBLE - Manage multiple library locations from console

- [ ] Create LibraryManager.cs in FsmpConsole/
  - [ ] Constructor(ConfigurationService configService, LibraryScanService scanService, UnitOfWork unitOfWork)
  - [ ] Task RunAsync()
  - [ ] Task DisplayLibraryPathsAsync()
  - [ ] Task AddLibraryPathAsync()
  - [ ] Task RemoveLibraryPathAsync()
  - [ ] Task ScanLibraryAsync(string path)
  - [ ] Task ScanAllLibrariesAsync()
  - [ ] Display: Path, Track Count, Last Scanned timestamp
- [ ] Create LibraryManagerTests.cs
  - [ ] Mock ConfigurationService and LibraryScanService
  - [ ] Test DisplayLibraryPathsAsync lists all configured paths
  - [ ] Test AddLibraryPathAsync updates config.json
  - [ ] Test RemoveLibraryPathAsync updates config.json
  - [ ] Test ScanLibraryAsync calls LibraryScanService
  - [ ] Test ScanAllLibrariesAsync scans all configured paths
  - [ ] Test displays track count from database
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All LibraryManager tests passing
- [ ] **Coverage**: ≥80%
- [ ] **Manual Verification**: Add library path, scan it, verify tracks imported

---

## Slice 23: Statistics Viewer UI 🎉

**What it delivers**: View play statistics from console UI

**Checkpoint**: USER-VISIBLE - See most played, recently played, favorites, total stats

- [ ] Create StatisticsViewer.cs in FsmpConsole/
  - [ ] Constructor(StatisticsService statsService)
  - [ ] Task RunAsync()
  - [ ] Task DisplayMostPlayedAsync()
  - [ ] Task DisplayRecentlyPlayedAsync()
  - [ ] Task DisplayFavoritesAsync()
  - [ ] Task DisplayTotalStatisticsAsync()
  - [ ] Format: Artist - Album - Title | Play Count | Last Played
- [ ] Enhance Print.cs in FsmpConsole/
  - [ ] Add FormatTable(List<object> data, List<string> headers) method
  - [ ] Add FormatProgressBar(int current, int total, int width) method
  - [ ] Add FormatMetadataCard(Track track) method
- [ ] Create StatisticsViewerTests.cs
  - [ ] Mock StatisticsService
  - [ ] Test DisplayMostPlayedAsync formats correctly
  - [ ] Test DisplayRecentlyPlayedAsync formats correctly
  - [ ] Test DisplayFavoritesAsync filters correctly
  - [ ] Test DisplayTotalStatisticsAsync shows counts
  - [ ] Test empty library displays friendly message
- [ ] Create PrintTests.cs
  - [ ] Test FormatTable creates aligned columns
  - [ ] Test FormatProgressBar renders correctly
  - [ ] Test FormatMetadataCard displays track info
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All Statistics Viewer & Print tests passing
- [ ] **Coverage**: ≥80%
- [ ] **Manual Verification**: View statistics, verify display matches database

---

## Slice 24: Program.cs Integration 🎉🎉🎉

**What it delivers**: COMPLETE END-TO-END APPLICATION with all features working!

**Checkpoint**: USER-VISIBLE MAJOR MILESTONE - Full music player experience!

- [ ] Refactor Program.cs in FsmpConsole/
  - [ ] Remove undefined `testFileRoot` variable
  - [ ] Initialize ConfigurationService
  - [ ] Load or create configuration
  - [ ] Initialize DbContext with connection string from config
  - [ ] Run EF migrations: `context.Database.Migrate()`
  - [ ] Initialize UnitOfWork with DbContext
  - [ ] Initialize all services (MetadataService, LibraryScanService, PlaybackService, StatisticsService)
  - [ ] Auto-scan libraries if AutoScanOnStartup=true
  - [ ] Initialize and launch MenuSystem.RunAsync()
  - [ ] Proper error handling and logging
- [ ] Create ProgramIntegrationTests.cs
  - [ ] Test application startup sequence
  - [ ] Test config file created at %AppData%/FSMP/config.json
  - [ ] Test database created at %AppData%/FSMP/fsmp.db
  - [ ] Test migrations applied successfully
  - [ ] Test auto-scan executes if enabled
  - [ ] Test services initialized correctly
  - [ ] Test menu system launches
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All Program integration tests passing
- [ ] **Coverage**: ≥80%
- [ ] **Manual End-to-End Test**:
  - [ ] Run application fresh (delete %AppData%/FSMP/)
  - [ ] Verify config.json created with default paths
  - [ ] Verify fsmp.db created
  - [ ] Add library path via UI
  - [ ] Scan library
  - [ ] Browse artists → albums → tracks
  - [ ] Play WAV file successfully
  - [ ] Play WMA file successfully
  - [ ] Play MP3 file successfully
  - [ ] Edit track metadata, verify it saves
  - [ ] View statistics, verify play counts
  - [ ] Restart application, verify data persists
  - [ ] Verify custom metadata displays correctly

---

## Slice 25: End-to-End Testing

**What it delivers**: Comprehensive E2E tests validating complete workflows

- [ ] Create EndToEndTests.cs in Tests/Integration/
  - [ ] Test fresh install workflow (no config, no database)
  - [ ] Test full workflow: startup → add library → scan → browse → play → edit → view stats
  - [ ] Test multi-library scenario (2+ library paths)
  - [ ] Test statistics after 10 plays
  - [ ] Test persistence across application restart
  - [ ] Test custom metadata overrides file metadata
  - [ ] Test duplicate file detection via hash
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All E2E tests passing
- [ ] **Coverage**: ≥80%

---

## Slice 26: Error Handling & Polish

**What it delivers**: Robust error handling and edge case coverage

- [ ] Create ErrorHandlingTests.cs in Tests/ErrorHandling/
  - [ ] Test corrupt config.json handling (creates default)
  - [ ] Test corrupt database handling (re-creates or repairs)
  - [ ] Test missing library paths (skips with warning)
  - [ ] Test corrupt audio file handling (logs error, continues)
  - [ ] Test TagLib exception handling (returns null metadata)
  - [ ] Test database migration failure (logs error, exits gracefully)
  - [ ] Test disk full during scan (handles gracefully)
  - [ ] Test file moved after scan (detects missing file)
- [ ] **Build**: ✅ Pass
- [ ] **Tests**: All error handling tests passing
- [ ] **Coverage**: ≥80%

---

## 🏁 Final Verification Checklist

### Build & Run
- [ ] Clean build: `build.cmd`
- [ ] All projects build without errors
- [ ] All projects build without warnings

### Testing
- [ ] Run all tests: `test.cmd`
- [ ] All tests pass (100% pass rate)
- [ ] Run coverage: `test-with-coverage.cmd`
- [ ] Verify coverage ≥ 80% for all projects (FsmpLibrary, FsmpDataAcsses)

### Functionality Verification (Manual)
- [ ] Fresh install creates config and database
- [ ] Add multiple library paths
- [ ] Scan all libraries successfully
- [ ] Browse database-driven navigation works
- [ ] WAV playback works
- [ ] WMA playback works
- [ ] MP3 playback works
- [ ] Metadata editing saves to database
- [ ] Statistics display correctly
- [ ] Application restart preserves all data
- [ ] Custom metadata overrides display correctly
- [ ] No hardcoded paths in code

### Success Criteria (All must be ✅)
- [ ] Multiple library paths configurable via JSON
- [ ] Library scanning imports tracks with metadata
- [ ] Metadata editable via console UI
- [ ] WAV, WMA, MP3 all play successfully
- [ ] Play counts increment correctly
- [ ] Statistics display most played, recently played, favorites
- [ ] Data persists across application restarts
- [ ] Original audio files remain untouched
- [ ] Code coverage ≥ 80% across all projects
- [ ] No compiler warnings

---

## Progress Summary

**Completed Slices**: 1, 2, 2a, 2b, 2c, 2d / 26
**Next Up**: Slice 3 — AlbumTests.cs (model enhancements already in code, only tests remain)

**Standalone reference**: `data-access-checklist.md` — ordered startup guide for getting FsmpDataAcsses from stub to working DbContext (covers prerequisites through migration).

Each ✅ checkbox represents a deliverable step. Work one slice at a time, updating checkboxes immediately after completion.

---
---

# 🌐 CROSS-PLATFORM MIGRATION PROJECT (Windows + Android)

**Goal**: Transform FSMP from Windows-only to cross-platform using .NET MAUI
**Plan Document**: `.claude/plans/elegant-honking-locket.md`

## Key Requirements

- ✅ Support Windows 10/11 and Android 11+
- ✅ Play WMA files WITHOUT altering originals (real-time decoding via ExoPlayer FFmpeg)
- ✅ Update console app to use new architecture (not deprecate)
- ✅ Maintain 80%+ test coverage throughout migration
- ✅ Keep existing database/config compatible

---

## Phase 1: Setup Projects ⚙️

**Deliverable**: New project structure with platform abstraction

- [ ] Create `FSMP.Core` class library (.NET 10.0)
- [ ] Create `FSMP.Platform.Windows` class library (.NET 10.0-windows)
- [ ] Create `FSMP.Platform.Android` class library (.NET 10.0-android)
- [ ] Create `FSMP.MAUI` app project (.NET 10.0)
- [ ] Install NuGet packages:
  - [ ] MAUI: `CommunityToolkit.Maui`, `CommunityToolkit.Maui.MediaElement`
  - [ ] Android: `Xamarin.AndroidX.Media3.ExoPlayer`, `Xamarin.AndroidX.Media3.ExoPlayer.Ffmpeg`
- [ ] Update `FSMP.Tests.csproj` to reference new projects
- [ ] **✅ Build**: Run `build.cmd` - all projects compile

---

## Phase 2: Platform Abstraction 🎯

**Deliverable**: IAudioPlayer interface with Windows/Android implementations

- [ ] Create `IAudioPlayer` interface in `FSMP.Core/Interfaces/`
  - [ ] Define members: LoadAsync, PlayAsync, PauseAsync, StopAsync, SeekAsync
  - [ ] Properties: Position, Duration, State, Volume
  - [ ] Events: StateChanged, PlaybackCompleted
- [ ] Create `WindowsAudioPlayer` in `FSMP.Platform.Windows/`
  - [ ] Use MediaElement for MP3/WAV
  - [ ] Fallback to WMPLib for WMA files
- [ ] Create `AndroidAudioPlayer` in `FSMP.Platform.Android/`
  - [ ] Configure ExoPlayer with FFmpeg extension for WMA support
  - [ ] Use ExoPlayer for all formats (MP3/WAV/WMA)
- [ ] Create tests: `WindowsAudioPlayerTests.cs`, `AndroidAudioPlayerTests.cs`
- [ ] **✅ Build & Test**: 80%+ coverage

---

## Phase 3: Migrate Business Logic 📦

**Deliverable**: Cross-platform core library with models and services

- [ ] Move models to `FSMP.Core/Models/`:
  - [ ] Move `Track.cs` (no schema changes)
  - [ ] Move `Album.cs`
  - [ ] Move `Artist.cs`
  - [ ] Move `PlaybackHistory.cs`
  - [ ] Update namespaces to `FSMP.Core.Models`
- [ ] Create `PlaybackService.cs` in `FSMP.Core/Services/`
  - [ ] Extract logic from `Fsmp.CheckFileLocation()`
  - [ ] Inject `IAudioPlayer` via DI
  - [ ] Remove direct Windows API calls
- [ ] Create `LibraryScanService.cs` in `FSMP.Core/Services/`
  - [ ] Extract directory scanning logic
- [ ] Update `FsmpLibrary.csproj`:
  - [ ] Remove WMPLib COM reference (move to Platform.Windows)
  - [ ] Remove System.Windows.Extensions
- [ ] Create tests: `PlaybackServiceTests.cs`, `LibraryScanServiceTests.cs`
- [ ] **✅ Build & Test**: 80%+ coverage

---

## Phase 4: Configure ExoPlayer FFmpeg 🎵

**Deliverable**: WMA playback on Android via real-time decoding

- [ ] Update `FSMP.Platform.Android.csproj`:
  - [ ] Add `Xamarin.AndroidX.Media3.ExoPlayer`
  - [ ] Add `Xamarin.AndroidX.Media3.ExoPlayer.Ffmpeg`
- [ ] Configure `AndroidAudioPlayer.cs`:
  - [ ] Initialize ExoPlayer with FFmpeg extension
  - [ ] Implement IAudioPlayer members
- [ ] **Alternative fallback**: If NuGet unavailable, use Maven dependencies
- [ ] Create tests: `WmaPlaybackTests.cs`
- [ ] **✅ Build & Test**: Verify FFmpeg decoder loads (check logs for "FfmpegAudioRenderer")
- [ ] **✅ Coverage**: 80%+

---

## Phase 5: Build MAUI UI 📱

**Deliverable**: Cross-platform MAUI app with navigation and playback controls

- [ ] Setup `MauiProgram.cs` with DI:
  - [ ] Register platform-specific `IAudioPlayer` (conditional)
  - [ ] Register services: PlaybackService, LibraryScanService, etc.
  - [ ] Configure DbContext with SQLite
- [ ] Create `Shell.xaml` navigation:
  - [ ] Tabs: Library, Now Playing, Settings
- [ ] Build `LibraryPage.xaml`:
  - [ ] Artist/Album/Track browser
  - [ ] Search functionality
  - [ ] Tap to play
- [ ] Build `NowPlayingPage.xaml`:
  - [ ] Track info display (title, artist, album art)
  - [ ] Playback controls (play/pause, previous, next)
  - [ ] Seek bar with progress
  - [ ] Volume control
- [ ] Build `SettingsPage.xaml`:
  - [ ] Library path management
  - [ ] Manual scan trigger
  - [ ] Auto-scan toggle
- [ ] Create ViewModels: `LibraryViewModel`, `NowPlayingViewModel`, `SettingsViewModel`
- [ ] **✅ Build & Test**:
  - [ ] Run on Windows
  - [ ] Run on Android emulator
  - [ ] Verify WMA plays on Android (no transcoding)

---

## Phase 6: Android-Specific Features 🤖

**Deliverable**: Android permissions, background playback, lock screen controls

- [ ] Configure `AndroidManifest.xml`:
  - [ ] Add storage permissions (READ_EXTERNAL_STORAGE, WRITE_EXTERNAL_STORAGE)
  - [ ] Add foreground service permission
- [ ] Implement permission requests in `MainActivity.cs`:
  - [ ] Request flow with explanation dialog
  - [ ] Handle denial gracefully
- [ ] Add media session support:
  - [ ] Lock screen controls
  - [ ] Notification with playback controls
  - [ ] Handle audio focus (pause on phone call)
- [ ] **✅ Build & Test**:
  - [ ] Install APK on Android 11+ device
  - [ ] Grant permissions
  - [ ] Test background playback
  - [ ] Test lock screen controls

---

## Phase 7: Testing & Coverage ✅

**Deliverable**: Comprehensive tests maintaining 80%+ coverage

- [ ] Create comprehensive tests:
  - [ ] `FSMP.Tests/Core/PlaybackServiceTests.cs`
  - [ ] `FSMP.Tests/Core/LibraryScanServiceTests.cs`
  - [ ] `FSMP.Tests/Platform.Windows/WindowsAudioPlayerTests.cs`
  - [ ] `FSMP.Tests/Platform.Android/AndroidAudioPlayerTests.cs`
  - [ ] `FSMP.Tests/Platform.Android/WmaPlaybackTests.cs`
  - [ ] `FSMP.Tests/Integration/EndToEndTests.cs`
- [ ] **✅ Build & Test**: Run `test-with-coverage.cmd`
- [ ] **✅ Coverage**: Verify 80%+ across all projects
- [ ] Manual testing checklist:
  - [ ] Install on Windows 10/11
  - [ ] Install on Android 11+ device
  - [ ] Add library with WAV, MP3, WMA files
  - [ ] Scan library on both platforms
  - [ ] Play WMA files on Android (verify no transcoded files created)
  - [ ] Play all formats on both platforms
  - [ ] Test playback controls (play, pause, seek)
  - [ ] Test background playback on Android
  - [ ] Test lock screen controls on Android
  - [ ] Edit track metadata and verify persistence
  - [ ] Restart app and verify state restored

---

## Phase 8: Documentation & Migration 📝

**Deliverable**: Updated documentation and migration guide

- [ ] Update `CLAUDE.md`:
  - [ ] Change line 29: `**Platform**: Cross-platform (Windows, Android)`
  - [ ] Update architecture section with new project structure
  - [ ] Add Android build commands
  - [ ] Document WMA playback via ExoPlayer FFmpeg
  - [ ] Add Android permissions guide (Android 11+ scoped storage)
- [ ] Create `MIGRATION.md`:
  - [ ] Instructions to copy database from Windows to Android
  - [ ] Path conversion guide (Windows → Android storage)
  - [ ] Explain WMA support on Android (FFmpeg extension, no transcoding)
- [ ] Update console app (per user requirement):
  - [ ] Refactor `FSMP.UI.Console` to use new `FSMP.Core` and `FSMP.Platform.Windows`
  - [ ] Ensure backward compatibility
  - [ ] Update documentation showing both console and MAUI options
- [ ] Update this `todo.md`:
  - [ ] ✅ Mark completed cross-platform tasks
  - [ ] Document any remaining work

---

## 🏁 Cross-Platform Migration Success Criteria

- [ ] MAUI app runs on Windows 10/11 and Android 11+
- [ ] All formats play on Windows (WAV, WMA, MP3)
- [ ] All formats play on Android (WAV, MP3, WMA via ExoPlayer FFmpeg)
- [ ] WMA files play WITHOUT creating transcoded copies
- [ ] Library scanning works on both platforms
- [ ] Database and config are cross-platform compatible
- [ ] Console app updated to use new architecture
- [ ] **Test coverage remains ≥ 80%**
- [ ] Build commands documented for both platforms
- [ ] User documentation includes Android guide

---

## Build Commands

### Windows
```batch
# Build Windows MAUI app
build.cmd

# Or manually
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" ^
  FSMP.MAUI\FSMP.MAUI.csproj -t:Build -p:Configuration=Debug -p:TargetFramework=net10.0-windows
```

### Android
```batch
# Build Android APK
dotnet build FSMP.MAUI\FSMP.MAUI.csproj ^
  -f:net10.0-android ^
  -c:Release ^
  -p:AndroidPackageFormat=apk
```

---

## Migration Progress Summary

**Status**: Planning complete, ready to begin implementation
**Current Phase**: Phase 1 - Setup Projects
**Completed Phases**: 0 / 8
**Next Action**: Create FSMP.Core, FSMP.Platform.Windows, FSMP.Platform.Android, FSMP.MAUI projects
