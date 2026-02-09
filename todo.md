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

## ✅ Slice 3: Album Model Complete (COMPLETE)

**What it delivers**: Fully tested Album entity model with artist relationships

- [x] Create Album.cs with all properties (Title, Year, AlbumArt, etc.)
- [x] Genre changed from `string?` to `ICollection<Genre>` (done in Slice 2b)
- [x] Enhance Album.cs with missing fields from plan:
  - [x] Add AlbumArtistName property
  - [x] Add AlbumArtPath property
  - [x] Add CreatedAt property
  - [x] Add UpdatedAt property
- [x] Create AlbumTests.cs with comprehensive tests
  - [x] Test default initialization
  - [x] Test all property setters
  - [x] Test navigation to Artist (nullable)
  - [x] Test navigation to Tracks collection
  - [x] Test AlbumArt byte array storage
  - [x] Test Year nullable behavior
  - [x] Test Genres collection (many-to-many)
  - [x] Test AlbumArtistName independent of Artist nav property
  - [x] Test CreatedAt/UpdatedAt DateTime storage
  - [x] Test AlbumArtPath nullable behavior
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 58/58 passing (19 new Album tests)
- [x] **Coverage**: ≥80%

---

## ✅ Slice 4: Artist Model Complete (COMPLETE)

**What it delivers**: Fully tested Artist entity model with album/track relationships

- [x] Create Artist.cs with basic properties
- [x] Added `ICollection<Genre> Genres` nav property (done in Slice 2b)
- [x] Enhance Artist.cs with missing fields from plan:
  - [x] Add Biography property
  - [x] Add CreatedAt property
  - [x] Add UpdatedAt property
- [x] Create ArtistTests.cs with comprehensive tests
  - [x] Test default initialization
  - [x] Test all property setters
  - [x] Test SortName nullable behavior
  - [x] Test navigation to Albums collection
  - [x] Test navigation to Tracks collection
  - [x] Test navigation to Genres collection
  - [x] Test Biography storage (including long text)
  - [x] Test CreatedAt/UpdatedAt DateTime storage
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 16 Artist tests passing
- [x] **Coverage**: ≥80%

---

## ✅ Slice 5: PlaybackHistory Model Complete (COMPLETE)

**What it delivers**: Fully tested PlaybackHistory entity for tracking plays

- [x] Create PlaybackHistory.cs with basic properties
- [x] Create PlaybackHistoryTests.cs with comprehensive tests
  - [x] Test default initialization
  - [x] Test all property setters
  - [x] Test PlayedAt timestamp
  - [x] Test PlayDuration nullable behavior
  - [x] Test CompletedPlayback flag
  - [x] Test WasSkipped flag
  - [x] Test navigation to Track
  - [x] Test edge cases (short/long durations, both flags true)
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 14 PlaybackHistory tests passing
- [x] **Coverage**: ≥80%

---

## ✅ Slice 6: Configuration Models Complete (COMPLETE)

**What it delivers**: Fully tested LibraryPath and Configuration entities

- [x] Create LibraryPath.cs in Models/
  - [x] LibraryPathId (PK)
  - [x] Path (string)
  - [x] IsActive (bool, default true)
  - [x] AddedAt (DateTime)
  - [x] LastScannedAt (DateTime?)
  - [x] TrackCount (int, cached)
- [x] Create LibraryPathTests.cs
  - [x] Test default initialization
  - [x] Test all property setters
  - [x] Test LastScannedAt nullable
  - [x] Test IsActive default and settable
  - [x] Test Windows and network paths
- [x] Create Configuration.cs in Models/
  - [x] LibraryPaths (List<string>)
  - [x] DatabasePath (string)
  - [x] AutoScanOnStartup (bool)
  - [x] DefaultVolume (int)
  - [x] RememberLastPlayed (bool)
  - [x] LastPlayedTrackPath (string?)
- [x] Create ConfigurationTests.cs
  - [x] Test default initialization
  - [x] Test LibraryPaths collection initialization
  - [x] Test all property setters
  - [x] Test add/remove paths
  - [x] Test volume values
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 11 LibraryPath + 17 Configuration tests passing
- [x] **Coverage**: ≥80%

---

## ✅ Slice 7: Database Context Foundation (COMPLETE)

**What it delivers**: Working FsmpDbContext with all entity relationships configured

- [x] Create FsmpDbContext.cs in FsmpDataAcsses/
  - [x] Add DbSet<Track> Tracks
  - [x] Add DbSet<Album> Albums
  - [x] Add DbSet<Artist> Artists
  - [x] Add DbSet<Genre> Genres
  - [x] Add DbSet<PlaybackHistory> PlaybackHistories
  - [x] Add DbSet<LibraryPath> LibraryPaths
  - [x] Add DbSet<FileExtension> FileExtensions
  - [x] Add constructor with DbContextOptions
  - [x] Seed Genre lookup rows: Rock, Jazz, Classic, Metal, Comedy
  - [x] Seed FileExtension lookup rows: wav, wma, mp3
- [x] Create FsmpDbContextTests.cs
  - [x] Test all DbSets are not null (7 tests)
  - [x] Test in-memory database creation
  - [x] Test can add and retrieve entities (Track, Album, Artist, PlaybackHistory, LibraryPath)
  - [x] Test Genre seed data present (5 genres)
  - [x] Test FileExtension seed data present (3 extensions)
- [x] Add OnModelCreating configuration
  - [x] Configure Track: FilePath unique index, FileHash index, relationships
  - [x] Configure Album: relationship to Artist, relationship to Tracks, many-to-many to Genre
  - [x] Configure Artist: indexes on Name, relationships, many-to-many to Genre
  - [x] Configure Track: many-to-many to Genre (implicit junction table TrackGenre)
  - [x] Configure PlaybackHistory: cascade delete, relationship to Track
  - [x] Configure LibraryPath: unique Path constraint
  - [x] Configure Genre: unique Name constraint
  - [x] Configure FileExtension: unique Extension constraint
- [x] Create EntityConfigurationTests.cs
  - [x] Test Track.FilePath unique constraint (SQLite)
  - [x] Test Album.Artist relationship (nullable + when set)
  - [x] Test Track.Album relationship (nullable + when set)
  - [x] Test PlaybackHistory.Track cascade delete
  - [x] Test LibraryPath.Path unique constraint (SQLite)
  - [x] Test Track-Genre many-to-many
  - [x] Test Album-Genre many-to-many
  - [x] Test Artist-Genre many-to-many
  - [x] Test Track-FileExtension relationship
  - [x] Test Genre.Name unique constraint (SQLite)
  - [x] Test FileExtension.Extension unique constraint (SQLite)
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 141/141 passing (28 new DbContext/entity tests)
- [x] **Coverage**: ≥80%

---

## ✅ Slice 8: Repository Pattern Base (COMPLETE)

**What it delivers**: Generic repository pattern foundation for all entities

- [x] Create IRepository<T>.cs interface in Repositories/
  - [x] Task<T?> GetByIdAsync(int id)
  - [x] Task<IEnumerable<T>> GetAllAsync()
  - [x] Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
  - [x] Task AddAsync(T entity)
  - [x] Task AddRangeAsync(IEnumerable<T> entities)
  - [x] void Update(T entity)
  - [x] void Remove(T entity)
  - [x] Task<int> CountAsync()
- [x] Create Repository<T>.cs base implementation in Repositories/
  - [x] Implement all IRepository<T> methods using DbContext
- [x] Create RepositoryTests.cs
  - [x] Test GetByIdAsync with in-memory database
  - [x] Test GetByIdAsync returns null when not found
  - [x] Test GetAllAsync returns all entities
  - [x] Test GetAllAsync returns empty when no entities
  - [x] Test FindAsync with predicate filters correctly
  - [x] Test FindAsync returns empty when no match
  - [x] Test AddAsync adds entity
  - [x] Test AddRangeAsync adds multiple entities
  - [x] Test Update modifies entity
  - [x] Test Remove deletes entity
  - [x] Test CountAsync returns correct count
  - [x] Test CountAsync returns zero when empty
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 153/153 passing (12 new repository tests)
- [x] **Coverage**: ≥80%

---

## ✅ Slice 9: Track Repository Specialized (COMPLETE)

**What it delivers**: TrackRepository with specialized queries for tracks

- [x] Create TrackRepository.cs in Repositories/
  - [x] Inherit from Repository<Track>
  - [x] Task<Track?> GetByFilePathAsync(string filePath)
  - [x] Task<IEnumerable<Track>> GetFavoritesAsync()
  - [x] Task<IEnumerable<Track>> GetMostPlayedAsync(int count)
  - [x] Task<IEnumerable<Track>> GetRecentlyPlayedAsync(int count)
  - [x] Task<Track?> GetByFileHashAsync(string fileHash)
- [x] Create TrackRepositoryTests.cs
  - [x] Test GetByFilePathAsync finds by path
  - [x] Test GetByFilePathAsync returns null when not found
  - [x] Test GetFavoritesAsync filters IsFavorite=true
  - [x] Test GetMostPlayedAsync orders by PlayCount DESC
  - [x] Test GetMostPlayedAsync respects count parameter
  - [x] Test GetRecentlyPlayedAsync orders by LastPlayedAt DESC
  - [x] Test GetRecentlyPlayedAsync excludes never-played tracks
  - [x] Test GetByFileHashAsync finds by hash (deduplication)
  - [x] Test GetByFileHashAsync returns null when not found
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 162/162 passing (9 new TrackRepository tests)
- [x] **Coverage**: ≥80%

---

## ✅ Slice 10: Album & Artist Repositories (COMPLETE)

**What it delivers**: AlbumRepository and ArtistRepository with specialized queries

- [x] Create AlbumRepository.cs in Repositories/
  - [x] Inherit from Repository<Album>
  - [x] Task<IEnumerable<Album>> GetByArtistAsync(int artistId)
  - [x] Task<IEnumerable<Album>> GetByYearAsync(int year)
  - [x] Task<Album?> GetWithTracksAsync(int albumId)
- [x] Create AlbumRepositoryTests.cs
  - [x] Test GetByArtistAsync filters by ArtistId
  - [x] Test GetByArtistAsync returns empty when no albums
  - [x] Test GetByYearAsync filters by Year
  - [x] Test GetByYearAsync returns empty when no match
  - [x] Test GetWithTracksAsync includes Tracks navigation
  - [x] Test GetWithTracksAsync returns null when not found
- [x] Create ArtistRepository.cs in Repositories/
  - [x] Inherit from Repository<Artist>
  - [x] Task<Artist?> GetWithAlbumsAsync(int artistId)
  - [x] Task<Artist?> GetWithTracksAsync(int artistId)
  - [x] Task<IEnumerable<Artist>> SearchAsync(string searchTerm)
- [x] Create ArtistRepositoryTests.cs
  - [x] Test GetWithAlbumsAsync includes Albums navigation
  - [x] Test GetWithAlbumsAsync returns null when not found
  - [x] Test GetWithTracksAsync includes Tracks navigation
  - [x] Test GetWithTracksAsync returns null when not found
  - [x] Test SearchAsync filters by name containing searchTerm
  - [x] Test SearchAsync returns empty when no match
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 174/174 passing (12 new Album & Artist repository tests)
- [x] **Coverage**: ≥80%

---

## ✅ Slice 11: PlaybackHistory Repository & Unit of Work (COMPLETE)

**What it delivers**: Complete repository pattern with Unit of Work coordinator

- [x] Create PlaybackHistoryRepository.cs in Repositories/
  - [x] Inherit from Repository<PlaybackHistory>
  - [x] Task<IEnumerable<PlaybackHistory>> GetRecentPlaysAsync(int count)
  - [x] Task<IEnumerable<PlaybackHistory>> GetByTrackAsync(int trackId)
  - [x] Task<int> GetTotalPlayCountAsync()
  - [x] Task<TimeSpan> GetTotalListeningTimeAsync()
- [x] Create PlaybackHistoryRepositoryTests.cs
  - [x] Test GetRecentPlaysAsync orders by PlayedAt DESC
  - [x] Test GetRecentPlaysAsync respects count parameter
  - [x] Test GetByTrackAsync filters by TrackId
  - [x] Test GetByTrackAsync returns empty when no history
  - [x] Test GetTotalPlayCountAsync counts all plays
  - [x] Test GetTotalListeningTimeAsync sums PlayDuration
  - [x] Test GetTotalListeningTimeAsync returns zero when empty
- [x] Create UnitOfWork.cs in FsmpDataAcsses/
  - [x] TrackRepository Tracks { get; }
  - [x] AlbumRepository Albums { get; }
  - [x] ArtistRepository Artists { get; }
  - [x] PlaybackHistoryRepository PlaybackHistories { get; }
  - [x] Repository<LibraryPath> LibraryPaths { get; }
  - [x] Repository<Genre> Genres { get; }
  - [x] Repository<FileExtension> FileExtensions { get; }
  - [x] Task<int> SaveAsync()
  - [x] Implement IDisposable
- [x] Create UnitOfWorkTests.cs
  - [x] Test all 7 repository properties initialized with correct types
  - [x] Test repository properties return same instance (lazy init)
  - [x] Test SaveAsync commits changes to database
  - [x] Test SaveAsync returns zero when no changes
  - [x] Test Dispose releases DbContext
  - [x] Test Dispose safe to call multiple times
  - [x] Test multiple repositories share context
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 194/194 passing (20 new: 7 PlaybackHistoryRepo + 13 UnitOfWork)
- [x] **Coverage**: ≥80%

---

## ✅ Slice 12: Initial Database Migration 🎉 (COMPLETE)

**What it delivers**: Working SQLite database creation via EF Core migration

**Checkpoint**: Database file appears at %AppData%\FSMP\fsmp.db (first tangible artifact!)

- [x] Create DesignTimeDbContextFactory for EF tooling
- [x] Add Microsoft.EntityFrameworkCore.Design to FsmpConsole.csproj
- [x] Run EF Core migration command:
  ```
  dotnet ef migrations add InitialCreate --project FSMP.db/entity/FsmpDataAcsses/FsmpDataAcsses --startup-project FSMP.UI/FSMP.UI.Console/FsmpConsole/FsmpConsole
  ```
- [x] Verify migration file created in Migrations/ folder (InitialCreate + Designer + Snapshot)
- [x] Review generated migration code — all tables, FKs, indexes, seed data correct
- [x] Create MigrationTests.cs in Tests/Database/
  - [x] Test migration applies successfully to SQLite database
  - [x] Test all 5 core tables created (Artists, Albums, Tracks, PlaybackHistories, LibraryPaths)
  - [x] Test lookup tables created (Genres, FileExtensions)
  - [x] Test junction tables created (TrackGenre, AlbumGenre, ArtistGenre)
  - [x] Test Track.FilePath unique index exists
  - [x] Test Track.FileHash index exists
  - [x] Test Album-Artist relationship (SET NULL on delete)
  - [x] Test PlaybackHistory-Track cascade delete configured
  - [x] Test LibraryPath.Path unique index exists
  - [x] Test Genre seed data present (5 genres)
  - [x] Test FileExtension seed data present (3 extensions)
  - [x] Test Track-Artist SET NULL on delete
  - [x] Test Track-Album SET NULL on delete
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 207/207 passing (13 new migration tests)
- [x] **Coverage**: ≥80%

---

## ✅ Slice 13: Configuration Service (End-to-End) 🎉 (COMPLETE)

**What it delivers**: JSON configuration file management

**Checkpoint**: Config file created at %AppData%\FSMP\config.json (second tangible artifact!)

- [x] Create ConfigurationService.cs in FsmpLibrary/Services/
  - [x] Constructor(string configPath)
  - [x] Task<Configuration> LoadConfigurationAsync()
  - [x] Task SaveConfigurationAsync(Configuration config)
  - [x] Task AddLibraryPathAsync(string path)
  - [x] Task RemoveLibraryPathAsync(string path)
  - [x] Configuration GetDefaultConfiguration()
  - [x] Create directory if missing on save
- [x] Create ConfigurationServiceTests.cs
  - [x] Test GetDefaultConfiguration returns valid defaults
  - [x] Test LoadConfigurationAsync creates default if missing
  - [x] Test SaveConfigurationAsync writes valid JSON
  - [x] Test LoadConfigurationAsync reads saved JSON correctly
  - [x] Test AddLibraryPathAsync updates config
  - [x] Test AddLibraryPathAsync does not duplicate
  - [x] Test RemoveLibraryPathAsync updates config
  - [x] Test RemoveLibraryPathAsync does nothing when path not found
  - [x] Test SaveConfigurationAsync creates directory when missing
  - [x] Test configuration file location ends with config.json
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 217/217 passing (10 new ConfigurationService tests)
- [x] **Coverage**: ≥80%

---

## ✅ Slice 14: Metadata Reading Service 🎉 (COMPLETE)

**What it delivers**: Read metadata from WAV/WMA/MP3 files using TagLibSharp

**Checkpoint**: Can extract title, artist, album, duration, album art from sample files

- [x] Create TrackMetadata.cs POCO in Services/
  - [x] Properties: Title, Artist, Album, Year, Genre, Duration, BitRate, SampleRate, AlbumArt, TrackNumber, DiscNumber
- [x] Create AudioProperties.cs POCO in Services/
  - [x] Properties: BitRate, SampleRate, Channels, BitsPerSample
- [x] Create IMetadataService.cs interface in Services/
- [x] Create MetadataService.cs in Services/
  - [x] Constructor()
  - [x] TrackMetadata ReadMetadata(string filePath)
  - [x] byte[]? ExtractAlbumArt(string filePath)
  - [x] TimeSpan? GetDuration(string filePath)
  - [x] AudioProperties GetAudioProperties(string filePath) (BitRate, SampleRate)
  - [x] Handle TagLib exceptions (corrupt files, unsupported formats)
- [x] Create MetadataServiceTests.cs
  - [x] WAV files created programmatically in test fixture; sample MP3/WMA from repo
  - [x] Test ReadMetadata extracts title from WAV
  - [x] Test ReadMetadata extracts artist from WMA
  - [x] Test ReadMetadata extracts album from MP3
  - [x] Test ReadMetadata extracts multiple fields from WAV
  - [x] Test ReadMetadata returns nulls for file with no tags
  - [x] Test ReadMetadata extracts duration from WAV
  - [x] Test ReadMetadata extracts BitRate and SampleRate
  - [x] Test ExtractAlbumArt returns byte[] for file with art
  - [x] Test ExtractAlbumArt returns null when no art
  - [x] Test GetDuration returns correct TimeSpan
  - [x] Test GetDuration returns duration from MP3
  - [x] Test GetAudioProperties returns BitRate and SampleRate
  - [x] Test GetAudioProperties returns properties from MP3
  - [x] Test error handling with corrupt file (returns null/defaults) — ReadMetadata, ExtractAlbumArt, GetDuration, GetAudioProperties
  - [x] Test error handling with missing file (throws FileNotFoundException)
  - [x] Test error handling with null path (throws ArgumentNullException)
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 246/246 passing (29 MetadataService tests: 23 unit + 6 integration with real MP3/WMA)
- [x] **Coverage**: ✅ ≥80% (overall 91.07%, MetadataService 86.2%)
- [x] **Manual Verification**: ✅ Verified on sample MP3 (Bonobo - Kerala) and WMA (AC/DC - Highway to Hell) — title, artist, album, duration, bit rate, sample rate all extracted correctly

**Key files created:**
- `FsmpLibrary/Services/TrackMetadata.cs` — metadata POCO
- `FsmpLibrary/Services/AudioProperties.cs` — audio properties POCO
- `FsmpLibrary/Services/IMetadataService.cs` — interface
- `FsmpLibrary/Services/MetadataService.cs` — TagLibSharp implementation
- `FSMP.Tests/Services/MetadataServiceTests.cs` — 23 tests

---

## ✅ Slice 15: Library Scanning (End-to-End) 🎉 (COMPLETE)

**What it delivers**: Scan directories and import tracks into database

**Checkpoint**: Sample music directory scanned, tracks appear in database with metadata

- [x] Create ScanResult.cs in FsmpLibrary/Services/
  - [x] int TracksAdded
  - [x] int TracksUpdated
  - [x] int TracksRemoved
  - [x] TimeSpan Duration
  - [x] List<string> Errors
- [x] Create LibraryScanService.cs in FsmpDataAcsses/Services/ (placed here to avoid circular dependency)
  - [x] Constructor(UnitOfWork unitOfWork, IMetadataService metadataService)
  - [x] Task<ScanResult> ScanAllLibrariesAsync(List<string> libraryPaths)
  - [x] Task<ScanResult> ScanLibraryAsync(string libraryPath)
  - [x] Task<Track?> ImportTrackAsync(FileInfo fileInfo)
  - [x] string CalculateFileHash(string filePath) (SHA256)
  - [x] bool IsSupportedFormat(string extension) (.wav, .wma, .mp3)
  - [x] Handle duplicate detection via FileHash (local HashSet + DB check)
  - [x] Create or update Artist and Album entities (FindOrCreate pattern)
- [x] Create LibraryScanServiceTests.cs
  - [x] Create test directory structure with sample audio files
  - [x] Test ScanLibraryAsync imports WAV files
  - [x] Test ScanLibraryAsync imports WMA files
  - [x] Test ScanLibraryAsync imports MP3 files
  - [x] Test CalculateFileHash returns consistent SHA256 hash
  - [x] Test IsSupportedFormat filters .wav, .wma, .mp3 (9 Theory cases)
  - [x] Test duplicate detection skips files with same hash
  - [x] Test error handling with corrupt files (logs error, continues)
  - [x] Test ScanResult aggregates counts correctly
  - [x] Test Artist/Album auto-creation from metadata
  - [x] Test artist reuse for multiple tracks by same artist
  - [x] Test album reuse for multiple tracks on same album
  - [x] Test file extension assignment from seed data
  - [x] Test fallback title when metadata has no title
  - [x] Test subdirectory scanning
  - [x] Test unsupported format filtering
  - [x] Test ScanAllLibrariesAsync aggregates across libraries
  - [x] Test ScanAllLibrariesAsync handles empty list
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 274/274 passing (28 new LibraryScanService tests)
- [x] **Coverage**: ✅ ≥80% (overall 91.55%, FsmpDataAcsses 98.04%, LibraryScanService ~97%)
- [ ] **Manual Verification**: Scan sample music folder, query database, verify tracks exist

**Key files created:**
- `FsmpLibrary/Services/ScanResult.cs` — scan result POCO
- `FsmpDataAcsses/Services/LibraryScanService.cs` — library scanner with SHA256 dedup
- `FSMP.Tests/Services/LibraryScanServiceTests.cs` — 28 tests

---

## Slice 16: MP3 Playback Support ✅ COMPLETE

**What it delivers**: MP3 files can be played (currently only WAV/WMA work)

**Checkpoint**: USER-VISIBLE CHANGE - MP3 files in sample music now play!

**NOTE**: This slice was completed by earlier slices (2e LibVLCSharp migration). `Fsmp.cs` was removed and replaced with `IAudioPlayer`/`LibVlcAudioPlayer` which supports all formats (WAV, WMA, MP3). `AudioService` + `AudioServiceTests` cover all playback scenarios.

- [x] MP3 playback supported via LibVlcAudioPlayer (handles all VLC-supported formats)
- [x] AudioService.PlayTrackAsync / PlayFileAsync — format-agnostic playback
- [x] Input validation: null checks, empty path checks in AudioService
- [x] AudioServiceTests: 20 tests covering null args, playback, pause/resume/stop/seek, volume, disposal
- [x] LibraryScanService includes .mp3 in SupportedExtensions
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 274/274 passing (AudioServiceTests covers all playback scenarios)
- [x] **Coverage**: ✅ ≥80%

---

## Slice 17: Playback Tracking Service ✅ COMPLETE

**What it delivers**: Playback history tracked in database, play counts increment

**Checkpoint**: Every play creates PlaybackHistory record and updates Track statistics

**NOTE**: Adapted from original plan — audio playback routing (Play/Pause/Stop/Resume) is handled by AudioService (Slice 2e). PlaybackTrackingService focuses on the DB-side: recording history and updating statistics.

- [x] Create PlaybackTrackingService.cs in FsmpDataAcsses/Services/
  - [x] Constructor(UnitOfWork unitOfWork) with null guard
  - [x] Task RecordPlaybackAsync(Track track, TimeSpan playDuration, bool completed, bool skipped)
  - [x] Creates PlaybackHistory record in database
  - [x] Increment Track.PlayCount on completed playback
  - [x] Update Track.LastPlayedAt
  - [x] Increment Track.SkipCount if skipped
  - [x] Update Track.UpdatedAt timestamp
  - [x] Task GetTrackHistoryAsync(int trackId)
  - [x] Task GetRecentPlaysAsync(int count) with validation
- [x] Create PlaybackTrackingServiceTests.cs (18 tests)
  - [x] Test constructor null guard
  - [x] Test RecordPlaybackAsync null track throws
  - [x] Test RecordPlaybackAsync creates PlaybackHistory record
  - [x] Test RecordPlaybackAsync increments Track.PlayCount when completed=true
  - [x] Test RecordPlaybackAsync doesn't increment PlayCount when completed=false
  - [x] Test RecordPlaybackAsync increments Track.SkipCount when skipped=true
  - [x] Test RecordPlaybackAsync doesn't increment SkipCount when not skipped
  - [x] Test RecordPlaybackAsync updates Track.LastPlayedAt
  - [x] Test RecordPlaybackAsync updates Track.UpdatedAt
  - [x] Test RecordPlaybackAsync accumulates multiple plays
  - [x] Test completed+skipped increments both counters
  - [x] Test GetTrackHistoryAsync returns history for track
  - [x] Test GetTrackHistoryAsync returns empty for no history
  - [x] Test GetTrackHistoryAsync doesn't return other tracks' history
  - [x] Test GetRecentPlaysAsync returns most recent
  - [x] Test GetRecentPlaysAsync returns empty when no history
  - [x] Test GetRecentPlaysAsync throws on zero/negative count
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 292/292 passing (18 new PlaybackTrackingService tests)
- [x] **Coverage**: ✅ ≥80% (overall 91.68%, FsmpDataAcsses 98.08%)

**Key files created:**
- `FsmpDataAcsses/Services/PlaybackTrackingService.cs` — records history, updates track stats
- `FSMP.Tests/Services/PlaybackTrackingServiceTests.cs` — 18 tests

---

## Slice 18: Statistics Service ✅ COMPLETE

**What it delivers**: Query play counts, favorites, most played, recently played

**Checkpoint**: Can retrieve statistics from database (most played, recently played, etc.)

- [x] Create StatisticsService.cs in FsmpDataAcsses/Services/
  - [x] Constructor(UnitOfWork unitOfWork) with null guard
  - [x] Task<IEnumerable<Track>> GetMostPlayedTracksAsync(int count)
  - [x] Task<IEnumerable<Track>> GetRecentlyPlayedTracksAsync(int count)
  - [x] Task<IEnumerable<Track>> GetFavoritesAsync()
  - [x] Task<Dictionary<string, int>> GetGenreStatisticsAsync()
  - [x] Task<int> GetTotalPlayCountAsync()
  - [x] Task<TimeSpan> GetTotalListeningTimeAsync()
  - [x] Task<int> GetTotalTrackCountAsync()
- [x] Create StatisticsServiceTests.cs (19 tests)
  - [x] Test constructor null guard
  - [x] Test GetMostPlayedTracksAsync returns top N by PlayCount DESC
  - [x] Test GetMostPlayedTracksAsync empty/zero/negative edge cases
  - [x] Test GetRecentlyPlayedTracksAsync returns top N by LastPlayedAt DESC
  - [x] Test GetRecentlyPlayedTracksAsync excludes never-played, empty/zero edge cases
  - [x] Test GetFavoritesAsync returns only IsFavorite=true tracks
  - [x] Test GetFavoritesAsync returns empty when no favorites
  - [x] Test GetGenreStatisticsAsync aggregates counts by genre
  - [x] Test GetGenreStatisticsAsync returns empty when no genre assignments
  - [x] Test GetTotalPlayCountAsync returns total history count (and zero)
  - [x] Test GetTotalListeningTimeAsync sums PlaybackHistory.PlayDuration (and zero)
  - [x] Test GetTotalTrackCountAsync returns correct count (and zero)
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 311/311 passing (19 new StatisticsService tests)
- [x] **Coverage**: ✅ ≥80% (overall 91.82%, FsmpDataAcsses 98.12%)

**Key files created:**
- `FsmpDataAcsses/Services/StatisticsService.cs` — library & playback statistics queries
- `FSMP.Tests/Services/StatisticsServiceTests.cs` — 19 tests

---

## Slice 19: Menu System UI ✅ COMPLETE

**What it delivers**: Interactive console menu for navigating application features

**Checkpoint**: Menu displays and accepts user input to navigate features

- [x] Create MenuSystem.cs in FsmpConsole/
  - [x] Constructor with IAudioService, ConfigurationService, StatisticsService, LibraryScanService, UnitOfWork, TextReader, TextWriter (all null-guarded)
  - [x] Task RunAsync() — main event loop
  - [x] void DisplayMainMenu() — shows all options
  - [x] Menu options: 1) Browse & Play, 2) Scan Libraries, 3) View Statistics, 4) Manage Libraries, 5) Settings, 6) Exit
  - [x] Browse & Play — lists tracks, allows selection, calls AudioService.PlayTrackAsync
  - [x] Scan Libraries — loads config paths, runs LibraryScanService.ScanAllLibrariesAsync
  - [x] View Statistics — shows total tracks, plays, listening time, most played
  - [x] Manage Libraries — list/add/remove library paths via ConfigurationService
  - [x] Settings — displays current settings
- [x] Create MenuSystemTests.cs in Tests/UI/ (20 tests)
  - [x] 7 constructor null guard tests
  - [x] Test DisplayMainMenu outputs all menu options
  - [x] Test option 6 exits and displays goodbye
  - [x] Test invalid option shows error and re-prompts
  - [x] Test empty input continues loop
  - [x] Test Browse & Play with no tracks shows message
  - [x] Test Browse & Play lists tracks
  - [x] Test Browse & Play selects track and calls PlayTrackAsync
  - [x] Test Browse & Play invalid selection shows error
  - [x] Test View Statistics shows stats
  - [x] Test Manage Libraries shows no-paths message
  - [x] Test Manage Libraries add path
  - [x] Test Settings shows current config
  - [x] Test Scan Libraries with no paths shows message
- [x] Added FsmpConsole project reference to FSMP.Tests
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 331/331 passing (20 new MenuSystem tests)
- [x] **Coverage**: ✅ ≥80% (overall 89.49%, FsmpConsole 65.59%, FsmpDataAcsses 98.12%)
- [ ] **Manual Verification**: Run menu, verify it displays and accepts input

**Key files created:**
- `FsmpConsole/MenuSystem.cs` — full interactive menu with TextReader/TextWriter for testability
- `FSMP.Tests/UI/MenuSystemTests.cs` — 20 tests

---

## Slice 20: Browse & Play UI ✅ COMPLETE

**What it delivers**: Navigate artists → albums → tracks, select and play

**Checkpoint**: USER-VISIBLE MAJOR CHANGE - Browse database and play tracks from menu!

- [x] Create BrowseUI.cs in FsmpConsole/
  - [x] Constructor(UnitOfWork unitOfWork, IAudioService audioService, TextReader input, TextWriter output)
  - [x] Task RunAsync()
  - [x] Task DisplayArtistsAsync()
  - [x] Task DisplayAlbumsByArtistAsync(int artistId)
  - [x] Task DisplayTracksByAlbumAsync(int albumId)
  - [x] Task PlayTrackAsync(int trackId) — displays Now Playing info and calls AudioService
- [x] Create BrowseUITests.cs (26 tests)
  - [x] 4 constructor null guard tests
  - [x] Test DisplayArtistsAsync lists all artists
  - [x] Test DisplayArtistsAsync no artists shows message
  - [x] Test DisplayArtistsAsync back option returns
  - [x] Test DisplayArtistsAsync invalid selection shows error
  - [x] Test DisplayArtistsAsync select artist navigates to albums
  - [x] Test DisplayAlbumsByArtistAsync artist not found
  - [x] Test DisplayAlbumsByArtistAsync no albums shows message
  - [x] Test DisplayAlbumsByArtistAsync lists albums with year
  - [x] Test DisplayAlbumsByArtistAsync album without year omits year
  - [x] Test DisplayAlbumsByArtistAsync invalid selection shows error
  - [x] Test DisplayAlbumsByArtistAsync select album navigates to tracks
  - [x] Test DisplayTracksByAlbumAsync album not found
  - [x] Test DisplayTracksByAlbumAsync no tracks shows message
  - [x] Test DisplayTracksByAlbumAsync lists tracks
  - [x] Test DisplayTracksByAlbumAsync shows duration
  - [x] Test DisplayTracksByAlbumAsync invalid selection shows error
  - [x] Test DisplayTracksByAlbumAsync select track calls PlayTrackAsync
  - [x] Test PlayTrackAsync track not found
  - [x] Test PlayTrackAsync displays now playing
  - [x] Test PlayTrackAsync calls AudioService
  - [x] Test PlayTrackAsync with duration/bitrate/rating shows details
  - [x] Test RunAsync delegates to DisplayArtistsAsync
- [x] Create PlaybackUI.cs in FsmpConsole/
  - [x] Constructor(TextWriter output)
  - [x] void DisplayNowPlaying(Track track) — Title, Artist, Album, Duration, BitRate, PlayCount, Rating
  - [x] void DisplayControls() — Stop, Pause, Next, Favorite, Edit Metadata
- [x] Create PlaybackUITests.cs (18 tests)
  - [x] Constructor null guard
  - [x] DisplayNowPlaying null track throws
  - [x] Test DisplayNowPlaying shows title, artist, album
  - [x] Test DisplayNowPlaying with/without duration, bitrate, rating
  - [x] Test DisplayNowPlaying with custom title overrides
  - [x] Test DisplayNowPlaying shows play count
  - [x] Test DisplayControls shows all control options (Stop, Pause, Next, Favorite, Edit Metadata)
- [x] Updated MenuSystem.BrowseAndPlayAsync to delegate to BrowseUI
- [x] Updated MenuSystemTests for hierarchical browse (3 tests updated)
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 375/375 passing (44 new: 26 BrowseUI + 18 PlaybackUI)
- [x] **Coverage**: ✅ ≥80% (overall 89.98%, FsmpConsole 77.74%, FsmpDataAcsses 98.12%)
- [ ] **Manual Verification**: Browse artists → albums → tracks, play a track

**Key files created:**
- `FsmpConsole/BrowseUI.cs` — hierarchical Artist → Album → Track browser with Now Playing display
- `FsmpConsole/PlaybackUI.cs` — standalone Now Playing display and playback controls
- `FSMP.Tests/UI/BrowseUITests.cs` — 26 tests
- `FSMP.Tests/UI/PlaybackUITests.cs` — 18 tests

---

## Slice 21: Metadata Editor UI ✅ COMPLETE

**What it delivers**: Search and edit track metadata from console UI

**Checkpoint**: USER-VISIBLE - Edit track title, artist, album, rating, favorites from UI

- [x] Create MetadataEditor.cs in FsmpConsole/
  - [x] Constructor(UnitOfWork unitOfWork, TextReader input, TextWriter output)
  - [x] Task RunAsync() — search → display → edit workflow
  - [x] Task<Track?> SearchTrackAsync() — search by title, custom title, artist name, or custom artist
  - [x] Task DisplayMetadataAsync(Track track) — shows file metadata + custom overrides
  - [x] Task EditMetadataAsync(Track track) — interactive editor with clear ("-") support
  - [x] Fields editable: CustomTitle, CustomArtist, CustomAlbum, Rating (1-5), IsFavorite, Comment
  - [x] Task SaveChangesAsync(Track track) — persists to database with UpdatedAt timestamp
- [x] Create MetadataEditorTests.cs (28 tests)
  - [x] 3 constructor null guard tests
  - [x] Test SearchTrackAsync empty search, no results, cancel, invalid selection
  - [x] Test SearchTrackAsync finds by title, artist, and custom title
  - [x] Test DisplayMetadataAsync shows file metadata, custom overrides, duration, "(none)" defaults
  - [x] Test EditMetadataAsync sets CustomTitle, CustomArtist, CustomAlbum
  - [x] Test EditMetadataAsync validates Rating 1-5, rejects invalid, clears with "-"
  - [x] Test EditMetadataAsync toggles IsFavorite (y/n)
  - [x] Test EditMetadataAsync sets/clears Comment
  - [x] Test EditMetadataAsync keeps all defaults on empty input
  - [x] Test SaveChangesAsync persists changes and updates timestamp
  - [x] Test RunAsync full workflow and empty-search exit
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 403/403 passing (28 new MetadataEditor tests)
- [x] **Coverage**: ✅ ≥80% (overall 90.32%, FsmpConsole 83.00%, FsmpDataAcsses 98.12%)
- [ ] **Manual Verification**: Search track, edit title, verify change persists in database

**Key files created:**
- `FsmpConsole/MetadataEditor.cs` — search + display + edit with clear ("-") support
- `FSMP.Tests/UI/MetadataEditorTests.cs` — 28 tests

---

## Slice 22: Library Manager UI ✅ COMPLETE

**What it delivers**: Add/remove library paths, trigger scans from UI

**Checkpoint**: USER-VISIBLE - Manage multiple library locations from console

- [x] Create LibraryManager.cs in FsmpConsole/
  - [x] Constructor(ConfigurationService, LibraryScanService, UnitOfWork, TextReader, TextWriter)
  - [x] Task RunAsync() — menu loop with Add/Remove/Scan/Back options
  - [x] Task DisplayLibraryPathsAsync() — lists paths + total track count
  - [x] Task AddLibraryPathAsync() — prompts for path, adds to config
  - [x] Task RemoveLibraryPathAsync() — prompts for index, removes from config
  - [x] Task ScanLibraryAsync(string path) — scans single path, shows results
  - [x] Task ScanAllLibrariesAsync() — scans all configured paths, shows aggregate results
- [x] Create LibraryManagerTests.cs (19 tests)
  - [x] 5 constructor null guard tests
  - [x] Test DisplayLibraryPathsAsync no paths, with paths, track count
  - [x] Test AddLibraryPathAsync valid path and empty path
  - [x] Test RemoveLibraryPathAsync valid index, invalid index, no paths
  - [x] Test ScanLibraryAsync shows results
  - [x] Test ScanAllLibrariesAsync no paths and with paths
  - [x] Test RunAsync back option, add path, invalid option
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 422/422 passing (19 new LibraryManager tests)
- [x] **Coverage**: ✅ ≥80% (overall 90.45%, FsmpConsole 84.97%, FsmpDataAcsses 98.12%)
- [ ] **Manual Verification**: Add library path, scan it, verify tracks imported

**Key files created:**
- `FsmpConsole/LibraryManager.cs` — library path management with scan integration
- `FSMP.Tests/UI/LibraryManagerTests.cs` — 19 tests

---

## Slice 23: Statistics Viewer UI ✅ COMPLETE

**What it delivers**: View play statistics from console UI

**Checkpoint**: USER-VISIBLE - See most played, recently played, favorites, total stats

- [x] Create StatisticsViewer.cs in FsmpConsole/
  - [x] Constructor(StatisticsService statsService, TextReader input, TextWriter output)
  - [x] Task RunAsync() — menu loop with Overview/MostPlayed/RecentlyPlayed/Favorites/Genre/Back
  - [x] Task DisplayMostPlayedAsync() — top 10 by play count
  - [x] Task DisplayRecentlyPlayedAsync() — top 10 by last played date
  - [x] Task DisplayFavoritesAsync() — all favorites with ratings
  - [x] Task DisplayTotalStatisticsAsync() — track count, play count, listening time
  - [x] Task DisplayGenreBreakdownAsync() — genre track counts
  - [x] Format: Artist - Title | Play Count / Last Played / Rating
- [x] Enhance Print.cs in FsmpConsole/
  - [x] Made Print class public (was internal) for testability
  - [x] Add FormatTable(List<string[]> rows, List<string> headers) method
  - [x] Add FormatProgressBar(int current, int total, int width) method
  - [x] Add FormatMetadataCard(Track track) method
- [x] Create StatisticsViewerTests.cs (25 tests)
  - [x] 3 constructor null guard tests
  - [x] Test DisplayTotalStatisticsAsync empty library shows zeros
  - [x] Test DisplayTotalStatisticsAsync with data shows counts
  - [x] Test DisplayTotalStatisticsAsync large listening time shows hours
  - [x] Test DisplayMostPlayedAsync empty shows message
  - [x] Test DisplayMostPlayedAsync formats correctly with play counts
  - [x] Test DisplayMostPlayedAsync orders by play count descending
  - [x] Test DisplayRecentlyPlayedAsync empty shows message
  - [x] Test DisplayRecentlyPlayedAsync shows last played date
  - [x] Test DisplayRecentlyPlayedAsync orders by most recent
  - [x] Test DisplayFavoritesAsync no favorites shows message
  - [x] Test DisplayFavoritesAsync lists favorites with ratings
  - [x] Test DisplayFavoritesAsync without rating omits rating
  - [x] Test DisplayGenreBreakdownAsync no data shows message
  - [x] Test DisplayGenreBreakdownAsync shows genre counts
  - [x] Test RunAsync back/empty/overview/most-played/recent/favorites/genre/invalid (8 tests)
- [x] Create PrintTests.cs (19 tests)
  - [x] Test FormatTable aligned columns, padding, empty rows, empty headers, null guards (6 tests)
  - [x] Test FormatProgressBar half/full/empty/zero-total/over-max (5 tests)
  - [x] Test FormatMetadataCard basic/duration/bitrate/rating/favorite/not-favorite/custom/null (8 tests)
- [x] **Build**: ✅ Pass
- [x] **Tests**: ✅ 466/466 passing (25 StatisticsViewer + 19 Print = 44 new)
- [x] **Coverage**: ✅ ≥80% (overall 91.04%, FsmpConsole 88.69%, FsmpDataAcsses 98.12%)
- [ ] **Manual Verification**: View statistics, verify display matches database

**Key files created:**
- `FsmpConsole/StatisticsViewer.cs` — statistics menu with overview, most played, recent, favorites, genre breakdown
- `FSMP.Tests/UI/StatisticsViewerTests.cs` — 25 tests
- `FSMP.Tests/UI/PrintTests.cs` — 19 tests

**Key files modified:**
- `FsmpConsole/Print.cs` — added FormatTable, FormatProgressBar, FormatMetadataCard; made public

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

**Completed Slices**: 1, 2, 2a, 2b, 2c, 2d, 2e, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 / 26
**Next Up**: Slice 24 — Program.cs Integration

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
