# FSMO — File System Music Organizer

Scan directories for audio files (.mp3, .wav, .wma) and organize them into an `Artist/Album/Track` directory structure using file metadata.

## Public API

All classes and methods are **static**.

### AudioFileScanner

Discover audio files in a directory tree.

```csharp
// Recursively find all supported audio files
List<FileInfo> files = AudioFileScanner.ScanDirectory(string sourcePath);

// Check if a file extension is supported (.mp3, .wav, .wma)
bool supported = AudioFileScanner.IsSupportedFormat(string extension);
```

### MetadataReader

Extract metadata from audio files via TagLibSharp.

```csharp
// Returns AudioMetadata with Title, Artist, Album, TrackNumber, Year, Duration
AudioMetadata metadata = MetadataReader.ReadMetadata(string filePath);
```

Returns empty metadata (all nulls) for corrupt or unsupported files.

### PathBuilder

Build organized target paths from metadata.

```csharp
// Returns: destinationRoot/Artist/Album/originalFileName
// Falls back to "Unknown Artist" / "Unknown Album" when metadata is missing
string path = PathBuilder.BuildTargetPath(string destinationRoot, AudioMetadata metadata, string originalFileName);
```

### FileOrganizer

Copy or move audio files into an organized directory structure.

```csharp
OrganizeResult result = FileOrganizer.Organize(
    string sourcePath,
    string destinationPath,
    OrganizeMode mode,                              // Copy or Move
    DuplicateStrategy duplicateStrategy = Skip       // Skip, Overwrite, or Rename
);

// result.FilesCopied, result.FilesMoved, result.FilesSkipped, result.Errors
```

### DirectoryManager

High-level wrapper combining scanning and organizing.

```csharp
// Organize files from source into destination
OrganizeResult result = DirectoryManager.ReorganiseDirectory(
    DirectoryInfo sourceDir,
    string destinationPath,
    OrganizeMode mode = Copy,
    DuplicateStrategy duplicateStrategy = Skip
);

// Get unique audio files (deduplicated by filename, case-insensitive)
List<FileInfo> files = DirectoryManager.GetAllDistinctAudioFiles(string sourcePath);

// Create a directory (no-op if exists)
DirectoryManager.CreateDirectory(string path);
```

### DirectoryComparer

Compare two directories and sync missing tracks.

```csharp
// Find audio files in targetPath that don't exist in appPath (by filename, case-insensitive)
List<FileInfo> missing = DirectoryComparer.FindMissingTracks(string appPath, string targetPath);

// Copy missing tracks into appPath using Artist/Album structure from metadata
OrganizeResult result = DirectoryComparer.CopyMissingToApp(
    string appPath,
    string targetPath,
    DuplicateStrategy duplicateStrategy = Skip
);
```

### FileSystem

Low-level file system helpers.

```csharp
FileSystem.CreateDirectory(string path);
FileSystem.CreateFile(string path);
FileSystem.DeleteFile(string path);
FileSystem.DeleteDirectory(string path);
FileSystem.MoveFile(string sourcePath, string destinationPath);
FileSystem.MoveDirectory(string sourcePath, string destinationPath);
```

All methods are safe to call on non-existent paths (no-op).

## Expected Directory Structure

```
Music Root/
  └── Artist Name/
      └── Album Name/
          ├── track1.mp3
          ├── track2.wav
          └── track3.wma
```

## Enums

| Enum | Values | Description |
|------|--------|-------------|
| `OrganizeMode` | `Copy`, `Move` | Whether to copy or move source files |
| `DuplicateStrategy` | `Skip`, `Overwrite`, `Rename` | How to handle files that already exist at destination |

## Usage Example

```csharp
using FSMO;

// Organize a messy music folder into Artist/Album structure
var result = FileOrganizer.Organize(
    @"C:\Downloads\Music",
    @"C:\Music\Library",
    OrganizeMode.Copy,
    DuplicateStrategy.Skip
);
Console.WriteLine($"Copied: {result.FilesCopied}, Skipped: {result.FilesSkipped}");

// Sync tracks from an external drive into your library
var syncResult = DirectoryComparer.CopyMissingToApp(
    @"C:\Music\Library",
    @"E:\Friend's Music"
);
Console.WriteLine($"Added {syncResult.FilesCopied} new tracks");
```
