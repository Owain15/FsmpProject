# Data Access Project — Startup Checklist

Ordered as an execution sequence. Each step builds on the previous one. Build and test after each step.

---

## 1. Project Reference

- [ ] Add `<ProjectReference>` to FsmpLibrary in `FsmpDataAcsses.csproj`
  - DbContext must be able to see the entity models (Track, Album, Artist, etc.)
  - Models stay in FsmpLibrary — data access layer depends on business logic, not the reverse

## 2. LibraryPath Model

- [ ] Create `LibraryPath.cs` in `FSMP.lib/FsmpLibrary/FsmpLibrary/Models/`
  - `LibraryPathId` (int, PK)
  - `Path` (string, unique — the absolute directory path)
  - `IsActive` (bool)
  - `AddedAt` (DateTime)
  - `LastScannedAt` (DateTime?, nullable)
  - `TrackCount` (int, cached count)
- [ ] Create `LibraryPathTests.cs` in `FSMP.Tests/Models/`
- [ ] Build and test

## 3. Remove Empty Stub

- [ ] Delete `DataAcsses.cs` (the empty placeholder class)

## 4. DbContext

- [ ] Create `FsmpDbContext.cs` in `FSMP.db/entity/FsmpDataAcsses/FsmpDataAcsses/`
  - Constructor: `FsmpDbContext(DbContextOptions<FsmpDbContext> options)`
  - DbSets:
    - `DbSet<Track> Tracks`
    - `DbSet<Album> Albums`
    - `DbSet<Artist> Artists`
    - `DbSet<Genre> Genres`
    - `DbSet<FileExtension> FileExtensions`
    - `DbSet<PlaybackHistory> PlaybackHistories`
    - `DbSet<LibraryPath> LibraryPaths`

## 5. Relationship & Index Configuration

- [ ] Override `OnModelCreating` in FsmpDbContext:
  - **Track**
    - Unique index on `FilePath`
    - Index on `FileHash`
    - Nullable FK → Album
    - Nullable FK → Artist
    - Nullable FK → FileExtension
  - **Album**
    - Nullable FK → Artist
  - **PlaybackHistory**
    - FK → Track with cascade delete
  - **LibraryPath**
    - Unique constraint on `Path`
  - **Many-to-many junction tables** (EF creates these implicitly from the `ICollection` nav properties, but configure explicitly if you want custom table/column names):
    - TrackGenre (Track ↔ Genre)
    - AlbumGenre (Album ↔ Genre)
    - ArtistGenre (Artist ↔ Genre)

## 6. Seed Data

- [ ] Seed Genre lookup rows in `OnModelCreating`:
  - Rock, Jazz, Classic, Metal, Comedy
- [ ] Seed FileExtension lookup rows:
  - wav, wma, mp3

## 7. Tests

- [ ] Create `FsmpDbContextTests.cs` in `FSMP.Tests/Database/`
  - Use `Microsoft.EntityFrameworkCore.InMemory` (already in test .csproj)
  - Test: all DbSets are not null after context creation
  - Test: can add and query each entity type
  - Test: seed data (Genre, FileExtension) is present
  - Test: PlaybackHistory cascade delete works
  - Test: Track.FilePath unique constraint enforced
- [ ] Build and test — coverage must remain ≥ 80%

## 8. Initial Migration

- [ ] Run from the repo root:
  ```
  dotnet ef migrations add InitialCreate --project FSMP.db/entity/FsmpDataAcsses/FsmpDataAcsses --startup-project FSMP.UI/FSMP.UI.Console/FsmpConsole/FsmpConsole
  ```
- [ ] Review the generated migration file in `Migrations/`
- [ ] Build and test

## 9. Smoke Test

- [ ] Add a quick startup call in Program.cs (temporary) to run `context.Database.Migrate()` and verify the `.db` file appears at the configured path
- [ ] Confirm all tables exist in the SQLite file (sqlite3 CLI or any SQLite browser)
- [ ] Remove the temporary smoke-test code once confirmed

---

**References**
- Models live in: `FSMP.lib/FsmpLibrary/FsmpLibrary/Models/`
- DbContext target: `FSMP.db/entity/FsmpDataAcsses/FsmpDataAcsses/`
- Tests target: `FSMP.Tests/`
- Detailed slice coverage in `todo.md` (Slices 3–12)
