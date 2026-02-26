# FsmpDataAcsses

Data access layer for FSMP — entity models, DbContext, repositories, and database queries.

## Responsibilities

**This project is responsible for:**
- Entity models (Track, Album, Artist, Genre, FileExtension, PlaybackHistory, etc.)
- EF Core DbContext and database configuration
- Repository pattern implementation
- Unit of Work coordination
- Database migrations
- Database queries and persistence operations

**This project is NOT responsible for:**
- UI or user interaction (see FsmpConsole / FSMP.MAUI)
- Business logic or orchestration (see FsmpLibrary)
- Audio playback (see FSMP.Platform.Windows)

## How It Fits In

Data Access Layer — the bottom tier of the architecture. FsmpLibrary depends on this project for all persistence operations. This project has no dependencies on other FSMP projects.
