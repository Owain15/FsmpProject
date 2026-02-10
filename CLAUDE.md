# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Workflow Preferences

**IMPORTANT: Work in small, incremental batches with frequent verification**

When implementing features:
1. **Work in small batches** - Complete one small task at a time (e.g., one model, one test file)
2. **Update ALL relevant todo.md files after each task** - This solution uses a multi-file todo structure. After completing any task, update:
   - The **per-project todo.md** for each project touched (check off items, update status)
   - The **solution-level todo.md** (root) dashboard (update status, coverage, test count)
   - See the "Todo File Locations" section below for file paths
3. **Build frequently** - Run `build.cmd` after each meaningful change
4. **Test frequently** - Run `test.cmd` after each test file is created
5. **Check coverage frequently** - Verify 80%+ coverage is maintained at each step
6. **NEVER commit to git** - Git commits are the user's responsibility. When you reach a natural checkpoint where you would normally commit, PAUSE and ask the user to review your work instead.

Example workflow:
- Create one model file → Update project todo.md → Build → Pass
- Create tests for that model → Update project todo.md → Build & Test → Pass
- Verify coverage → Update project todo.md + solution todo.md (coverage/test counts) → Coverage ≥ 80%
- Move to next task

Do NOT create multiple files/models/tests without verifying each step builds and tests pass.

## Todo File Locations

The solution uses a multi-file todo structure. Each project has its own todo.md, and the root todo.md serves as a solution-level dashboard.

**IMPORTANT: After completing any task, update ALL relevant todo.md files to reflect the current state.**

| File | Purpose |
|------|---------|
| [todo.md](todo.md) | Solution-level dashboard — project status table, coverage, test counts, milestones |
| [FSMP.lib/FsmpLibrary/todo.md](FSMP.lib/FsmpLibrary/todo.md) | FsmpLibrary project tasks |
| [FSMP.db/entity/FsmpDataAcsses/todo.md](FSMP.db/entity/FsmpDataAcsses/todo.md) | FsmpDataAcsses project tasks |
| [FSMP.UI/FSMP.UI.Console/FsmpConsole/todo.md](FSMP.UI/FSMP.UI.Console/FsmpConsole/todo.md) | FsmpConsole project tasks |
| [FSMP.lib/FSMO/todo.md](FSMP.lib/FSMO/todo.md) | FSMO project tasks |
| [FSMP.Tests/todo.md](FSMP.Tests/todo.md) | Test suite tasks |

**Update rules:**
1. After completing a task, update the todo.md for **every project affected** — not just the one you worked in. For example, adding tests for FSMO requires updating the FSMO todo.md (check off test task), the FSMP.Tests todo.md (new test files/counts), and the solution dashboard (test count, coverage)
2. After build/test/coverage runs, update the solution dashboard (root todo.md) with current numbers
3. When starting a new slice or phase, add it to the relevant project todo.md
4. When a project's status changes (e.g., "Not started" → "In progress"), update the solution dashboard table

## Project Overview

FSMP (File System Music Player) is a Windows-only C# .NET 10.0 console application for playing audio files. The application can play WAV, WMA, and MP3 format music files organized in an Artist/Album/Track directory structure.

**Platform**: Windows-only (uses COM interop with Windows Media Player)

## Build & Development Commands

**IMPORTANT**: This project uses COM interop (WMPLib) which requires Visual Studio's MSBuild. The standard `dotnet build` command will fail with error MSB4803.

### Building

```bash
# Build the solution (using build helper script)
build.cmd

# Or use MSBuild directly
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" "FSMP.UI/FSMP.UI.Console/FsmpConsole/FsmpConsole.slnx" -t:Build -p:Configuration=Debug

# Clean build artifacts
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" "FSMP.UI/FSMP.UI.Console/FsmpConsole/FsmpConsole.slnx" -t:Clean
```

### Testing

```bash
# Run tests (using test helper script)
test.cmd

# Run tests with code coverage
test-with-coverage.cmd

# Or manually: build first, then test with --no-build
build.cmd
dotnet test FSMP.Tests/FSMP.Tests.csproj --no-build
```

**Test Requirements**:
- Minimum 80% code coverage required
- All tests must pass before committing
- Build and test after every code change

### Running

```bash
# Run the application
dotnet run --project "FSMP.UI/FSMP.UI.Console/FsmpConsole/FsmpConsole/FsmpConsole.csproj"

# Or run the built executable
"FSMP.UI/FSMP.UI.Console/FsmpConsole/FsmpConsole/bin/Debug/net10.0/FsmpConsole.exe"
```

### Publishing

```bash
# Publish for deployment (requires MSBuild)
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" "FSMP.UI/FSMP.UI.Console/FsmpConsole/FsmpConsole/FsmpConsole.csproj" -t:Publish -p:Configuration=Release
```

## Architecture

### Three-Tier Layered Architecture

The project follows a layered architecture with clear separation of concerns:

```
UI Layer (FSMP.UI.Console)
    ↓
Business Logic Layer (FSMP.lib)
    ↓
Data Access Layer (FSMP.db)
```

**FSMP.UI.Console** (`FSMP.UI/FSMP.UI.Console/FsmpConsole/FsmpConsole/`)
- Presentation layer with console interface
- Contains [Program.cs](FSMP.UI/FSMP.UI.Console/FsmpConsole/FsmpConsole/Program.cs) (application entry point) and [Print.cs](FSMP.UI/FSMP.UI.Console/FsmpConsole/FsmpConsole/Print.cs) (UI utilities)
- Dependencies: References both FsmpLibrary and FsmpDataAcsses projects

**FSMP.lib - FsmpLibrary** (`FSMP.lib/FsmpLibrary/FsmpLibrary/`)
- Core business logic layer
- Contains [Fsmp.cs](FSMP.lib/FsmpLibrary/FsmpLibrary/Fsmp.cs) with static methods for audio playback
- Uses static facade pattern for media operations
- Dependencies: WMPLib (COM), System.Windows.Extensions 10.0.2

**FSMP.db - FsmpDataAcsses** (`FSMP.db/entity/FsmpDataAcsses/FsmpDataAcsses/`)
- Data access layer with Entity Framework Core and SQLite
- Implements repository pattern for data access
- Contains entity models, DbContext, and repositories

## Technology Stack

- **.NET**: 10.0
- **Language**: C# with ImplicitUsings and Nullable enabled
- **Audio Playback**:
  - WAV files: `System.Media.SoundPlayer`
  - WMA files: `WMPLib.WindowsMediaPlayer` (COM interop)
  - MP3 files: `WMPLib.WindowsMediaPlayer` (COM interop)
- **Data Persistence**:
  - SQLite with Entity Framework Core 9.0
  - Repository pattern with Unit of Work
- **Metadata Reading**:
  - TagLibSharp 2.3.0 for reading audio file metadata
- **External Dependencies**:
  - Windows Media Player (WMPLib) - COM reference
  - System.Windows.Extensions 10.0.2 (NuGet)
  - Microsoft.EntityFrameworkCore.Sqlite 9.0.0 (NuGet)
  - TagLibSharp 2.3.0 (NuGet)
  - System.Text.Json 9.0.0 (NuGet)

## Testing

The project uses **xUnit** for unit testing with the following test infrastructure:

- **Test Project**: `FSMP.Tests/` (references FsmpLibrary and FsmpDataAcsses)
- **Test Frameworks**:
  - xUnit - Test framework
  - Moq 4.20.72 - Mocking library
  - FluentAssertions 8.8.0 - Assertion library
  - Coverlet - Code coverage
  - Microsoft.EntityFrameworkCore.InMemory 9.0.0 - In-memory database for testing

**Coverage Requirement**: Minimum 80% code coverage on all non-test code

**Test Directory Structure**:
```
FSMP.Tests/
├── Models/         # Entity model tests
├── Services/       # Business logic service tests
├── Repositories/   # Repository tests
├── Database/       # DbContext and migration tests
├── UI/             # UI component tests
├── Integration/    # Integration tests
└── TestHelpers/    # Test utilities and helpers
```

## Key Implementation Details

### Audio Playback

The `Fsmp` class ([Fsmp.cs](FSMP.lib/FsmpLibrary/FsmpLibrary/Fsmp.cs)) provides static methods for audio operations:
- `CheckFileLocation(string filePath)` - Scans directory structure and plays first available track
- `PlayWav(string wavPath)` - Plays WAV files using System.Media.SoundPlayer
- `PlayWma(string wmaPath)` - Plays WMA files using Windows Media Player COM interop
- `PlayMp3(string mp3Path)` - Plays MP3 files using Windows Media Player COM interop

All playback methods include:
- Platform validation using `OperatingSystem.IsWindows()`
- Error handling for file access and playback failures
- Input validation for file paths and formats

### Directory Structure Expectations

The application expects music files organized in this hierarchy:
```
Music Root/
  └── Artist Name/
      └── Album Name/
          └── Track Files (.wav, .wma, .mp3)
```

The application will scan all configured library locations and import tracks into the database.

### Configuration

Configuration is stored in a JSON file at:
```
%AppData%\FSMP\config.json
```

Example configuration:
```json
{
  "libraryPaths": [
    "C:\\Users\\Admin\\source\\repos\\FsmpProject\\res\\sampleMusic",
    "D:\\Music"
  ],
  "databasePath": "%AppData%\\FSMP\\fsmp.db",
  "autoScanOnStartup": true,
  "defaultVolume": 75
}
```

### Data Access Layer

The `FsmpDataAcsses` project implements:
- **Entity Models**: Track, Album, Artist, PlaybackHistory, LibraryPath
- **DbContext**: FsmpDbContext with EF Core configuration
- **Repository Pattern**: Generic repository with specialized repositories
- **Unit of Work**: Coordinates multiple repositories and transactions

### Metadata Management

Metadata is handled non-destructively:
- Original file metadata is read using TagLibSharp
- Custom metadata overrides are stored in the database
- Original files are never modified
- Display shows custom metadata if available, otherwise file metadata

## Project Status

- **Stage**: Active development - implementing data storage and metadata features
- **Working Features**: Console UI, WAV/WMA/MP3 playback, directory scanning, test infrastructure
- **Testing**: xUnit configured with 80% coverage requirement
- **CI/CD**: Not configured
- **In Progress**: Data access layer with EF Core, multi-location library support, metadata editing

---

## 🌐 PLANNED: Cross-Platform Migration (Windows + Android)

**Status**: Planning complete, implementation not yet started
**Plan Document**: `.claude/plans/elegant-honking-locket.md`
**Tracking**: See "CROSS-PLATFORM MIGRATION PROJECT" section in [todo.md](todo.md)

### Migration Overview

FSMP is being redesigned to support both Windows and Android platforms using .NET MAUI while maintaining backward compatibility with the existing Windows console application.

### Key Goals

- ✅ Support Windows 10/11 and Android 11+
- ✅ Play WMA files WITHOUT altering originals (real-time decoding via ExoPlayer FFmpeg on Android)
- ✅ Update console app to use new architecture (not deprecate)
- ✅ Maintain 80%+ test coverage throughout migration
- ✅ Keep existing database/config compatible

### Planned Architecture Changes

**New Project Structure:**
```
FSMP.Core/                    # NEW: Cross-platform business logic
├── Interfaces/
│   └── IAudioPlayer.cs      # Platform abstraction
├── Services/
│   ├── PlaybackService.cs   # Platform-agnostic orchestration
│   ├── MetadataService.cs
│   └── LibraryScanService.cs
└── Models/                   # Moved from FsmpLibrary
    ├── Track.cs
    ├── Album.cs
    ├── Artist.cs
    └── PlaybackHistory.cs

FSMP.Platform.Windows/        # NEW: Windows-specific implementation
└── WindowsAudioPlayer.cs    # MediaElement + WMPLib for WMA

FSMP.Platform.Android/        # NEW: Android-specific implementation
└── AndroidAudioPlayer.cs    # ExoPlayer with FFmpeg extension

FSMP.MAUI/                    # NEW: Cross-platform UI
├── Platforms/
│   ├── Windows/
│   └── Android/
├── Views/
│   ├── LibraryPage.xaml
│   ├── NowPlayingPage.xaml
│   └── SettingsPage.xaml
└── ViewModels/

FSMP.db/                      # UNCHANGED: SQLite is already cross-platform

FSMP.UI.Console/              # UPDATED: Will use new FSMP.Core architecture
```

### Technical Approach

**Audio Playback:**
- **Windows**: Continue using WMPLib (COM) for WMA, MediaElement for MP3/WAV
- **Android**: ExoPlayer with FFmpeg extension for real-time WMA decoding (no transcoding!)
- **Abstraction**: `IAudioPlayer` interface with platform-specific implementations

**WMA Support on Android:**
- Use `androidx.media3:media3-exoplayer-ffmpeg` for real-time WMA decoding
- No file modification or transcoding required (user requirement)
- Original files remain untouched
- Slightly higher CPU usage during playback vs transcoding

**Platform Support:**
- **Minimum Android**: Android 11 (API 30) - scoped storage, 90%+ device coverage
- **Windows**: Windows 10/11 (existing requirement)

### Migration Phases

1. **Phase 1**: Setup Projects - Create FSMP.Core, Platform.Windows, Platform.Android, FSMP.MAUI
2. **Phase 2**: Platform Abstraction - IAudioPlayer interface with implementations
3. **Phase 3**: Migrate Business Logic - Move models/services to FSMP.Core
4. **Phase 4**: Configure ExoPlayer FFmpeg - WMA support on Android
5. **Phase 5**: Build MAUI UI - Cross-platform interface
6. **Phase 6**: Android-Specific Features - Permissions, background playback, lock screen controls
7. **Phase 7**: Testing & Coverage - Comprehensive tests maintaining 80%+
8. **Phase 8**: Documentation & Migration - Update docs, console app, migration guide

### Build Commands (After Migration)

**Windows MAUI:**
```batch
build.cmd
# Or manually:
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" ^
  FSMP.MAUI\FSMP.MAUI.csproj -t:Build -p:Configuration=Debug -p:TargetFramework=net10.0-windows
```

**Android APK:**
```batch
dotnet build FSMP.MAUI\FSMP.MAUI.csproj ^
  -f:net10.0-android ^
  -c:Release ^
  -p:AndroidPackageFormat=apk
```

### Key Design Decisions

1. **.NET MAUI over Xamarin**: MAUI is the official successor, .NET 10.0 compatible
2. **ExoPlayer FFmpeg over transcoding**: Real-time decoding preserves original files (user requirement)
3. **Platform abstraction via DI**: Clean separation, testable, maintainable
4. **Update console app (not deprecate)**: Maintain backward compatibility per user preference
5. **Android 11+ minimum**: Scoped storage simplifies permissions, covers 90%+ active devices

### Success Criteria

- MAUI app runs on Windows 10/11 and Android 11+
- All formats play on Windows (WAV, WMA, MP3)
- All formats play on Android (WAV, MP3, WMA via ExoPlayer FFmpeg)
- WMA files play WITHOUT creating transcoded copies
- Library scanning works on both platforms
- Database and config are cross-platform compatible
- Console app updated to use new architecture
- **Test coverage remains ≥ 80%**
- Build commands documented for both platforms
- User documentation includes Android guide

### Current Implementation Status

**⚠️ IMPORTANT**: The cross-platform migration has NOT been implemented yet. The current codebase remains Windows-only. All information in this section describes planned changes. Refer to [todo.md](todo.md) for implementation progress.
