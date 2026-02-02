# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

FSMP (File System Music Player) is a Windows-only C# .NET 10.0 console application for playing audio files. The application can play WAV and WMA format music files organized in an Artist/Album/Track directory structure.

**Platform**: Windows-only (uses COM interop with Windows Media Player)

## Build & Development Commands

```bash
# Build the solution
dotnet build "FSMP.UI/FSMP.UI.Console/FsmpConsole/FsmpConsole.slnx"

# Build in Release mode
dotnet build -c Release "FSMP.UI/FSMP.UI.Console/FsmpConsole/FsmpConsole.slnx"

# Run the application
dotnet run --project "FSMP.UI/FSMP.UI.Console/FsmpConsole/FsmpConsole/FsmpConsole.csproj"

# Clean build artifacts
dotnet clean "FSMP.UI/FSMP.UI.Console/FsmpConsole/FsmpConsole.slnx"

# Publish for deployment
dotnet publish -c Release "FSMP.UI/FSMP.UI.Console/FsmpConsole/FsmpConsole.csproj"
```

**Note**: No test framework or linting tools are currently configured.

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
- Data access layer (currently empty placeholder)
- Intended for future data persistence features

## Technology Stack

- **.NET**: 10.0
- **Language**: C# with ImplicitUsings and Nullable enabled
- **Audio Playback**:
  - WAV files: `System.Media.SoundPlayer`
  - WMA files: `WMPLib.WindowsMediaPlayer` (COM interop)
- **External Dependencies**:
  - Windows Media Player (WMPLib) - COM reference
  - System.Windows.Extensions 10.0.2 (NuGet)

## Key Implementation Details

### Audio Playback

The `Fsmp` class ([Fsmp.cs](FSMP.lib/FsmpLibrary/FsmpLibrary/Fsmp.cs)) provides static methods for audio operations:
- `CheckFileLocation(string filePath)` - Scans directory structure and plays first available track
- `PlayWav(string wavPath)` - Plays WAV files using System.Media.SoundPlayer
- `PlayWma(string wmaPath)` - Plays WMA files using Windows Media Player COM interop

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
          └── Track Files (.wav, .wma)
```

Currently, the application plays only the first track found in the first album of the first artist (array index [0]).

### Hardcoded Paths

[Program.cs:13](FSMP.UI/FSMP.UI.Console/FsmpConsole/FsmpConsole/Program.cs#L13) contains a hardcoded path to sample music:
```csharp
string testFileRoot = @"C:\Users\Admin\source\repos\FsmpProject\res\sampleMusic";
```

When modifying or testing, update this path or refactor to use configuration.

### Data Access Layer Status

The `FsmpDataAcsses` project is currently a placeholder with an empty `DataAcsses` class. No data persistence is implemented yet.

## Project Status

- **Stage**: Early development
- **Working Features**: Console UI, WAV/WMA playback, directory scanning
- **Testing**: No test framework configured
- **CI/CD**: Not configured
- **Pending**: Data access layer implementation, configuration management, test coverage
