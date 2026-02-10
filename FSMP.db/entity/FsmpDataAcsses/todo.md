# FsmpDataAcsses - Data Access Layer

Entity Framework Core data access layer with SQLite, implementing the repository pattern and Unit of Work for FSMP.

## Completed Work Summary

### Database Context

- `FsmpDbContext.cs` -- 7 DbSets (Tracks, Albums, Artists, Genres, PlaybackHistories, LibraryPaths, FileExtensions), seed data for genres (Rock, Jazz, Classic, Metal, Comedy) and file extensions (wav, wma, mp3), full entity configuration with indexes, relationships, and constraints

### Repositories (`Repositories/`)

- `IRepository.cs` / `Repository.cs` -- Generic repository base (GetById, GetAll, Find, Add, AddRange, Update, Remove, Count)
- `TrackRepository.cs` -- GetByFilePath, GetFavorites, GetMostPlayed, GetRecentlyPlayed, GetByFileHash
- `AlbumRepository.cs` -- GetByArtist, GetByYear, GetWithTracks
- `ArtistRepository.cs` -- GetWithAlbums, GetWithTracks, Search
- `PlaybackHistoryRepository.cs` -- GetRecentPlays, GetByTrack, GetTotalPlayCount, GetTotalListeningTime

### Unit of Work

- `UnitOfWork.cs` -- Coordinates all 7 repositories with shared DbContext, IDisposable, SaveAsync

### Migrations

- `DesignTimeDbContextFactory.cs` -- For EF Core tooling
- `Migrations/20260206102128_InitialCreate.cs` -- Full schema with all tables, FKs, indexes, junction tables

### Services (`Services/`)

- `LibraryScanService.cs` -- Scan directories, import tracks with metadata, SHA256 duplicate detection, artist/album auto-creation
- `PlaybackTrackingService.cs` -- Record playback history, update play count/skip count/last played timestamps
- `StatisticsService.cs` -- Most played, recently played, favorites, genre breakdown, total counts/listening time

## Current Status

**Status**: Complete (v1) | **Coverage**: 98.18% | **Tests**: see FSMP.Tests

---

## Future Work

### Cross-Platform Migration (when Phase 1 begins)

- [ ] Ensure SQLite database paths work on Android (scoped storage)
- [ ] Verify EF Core migrations apply correctly on Android runtime
- [ ] Consider path conversion utilities for Windows -> Android

### Potential Schema Changes

- [ ] Future migrations as new features require schema updates

---

## Progress Summary

**Status**: Complete (v1)
**Next Action**: Cross-platform migration Phase 1
