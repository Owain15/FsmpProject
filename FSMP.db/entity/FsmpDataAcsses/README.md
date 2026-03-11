# FsmpDataAcsses

Data access layer for FSMP — entity models, DbContext, repositories, and database queries.

## Responsibilities

**This project is responsible for:**
- Entity models (Track, Album, Artist, Tag, FileExtension, PlaybackHistory, Playlist, etc.)
- EF Core DbContext and database configuration
- Repository pattern implementation
- Unit of Work coordination
- Database migrations
- Database queries and persistence operations
- Data services (LibraryScanService, PlaybackTrackingService, StatisticsService, PlaylistService)

**This project is NOT responsible for:**
- UI or user interaction (see FsmpConsole / FSMP.MAUI)
- Business logic or orchestration (see FSMP.Core)
- Audio playback (see FSMP.Platform.Windows)

## How It Fits In

Data Access Layer — the bottom tier of the architecture. FSMP.Core depends on this project for all persistence operations. This project has no dependencies on other FSMP projects.
