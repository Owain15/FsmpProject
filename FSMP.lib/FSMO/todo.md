# FSMO - File System Music Organizer

**Goal**: Scan a source directory for audio files and reorganize them into `Music → Artist → Album → Track` structure based on file metadata. Support both copy and move operations.

**Supported formats**: .mp3, .wav, .wma

---

## Slice 1: Testing Infrastructure

**What it delivers**: FSMO project wired into the test suite with TagLibSharp for metadata reading

- [x] Add TagLibSharp 2.3.0 NuGet package to FSMO.csproj
- [x] Add FSMO project reference to FSMP.Tests.csproj
- [x] Create test directory `FSMP.Tests/FSMO/`
- [x] Create a placeholder test to verify the reference works
- [x] **Build**: Pass
- [x] **Tests**: Pass

---

## Slice 2: FileSystem Tests

**What it delivers**: Full test coverage for the existing `FileSystem` helper class

- [x] Create `FileSystemTests.cs` in `FSMP.Tests/FSMO/`
  - [x] Test CreateDirectory creates new directory
  - [x] Test CreateDirectory does nothing when directory exists
  - [x] Test CreateFile creates new file
  - [x] Test CreateFile does nothing when file exists
  - [x] Test DeleteFile removes existing file
  - [x] Test DeleteFile does nothing when file missing
  - [x] Test DeleteDirectory removes existing directory recursively
  - [x] Test DeleteDirectory does nothing when directory missing
  - [x] Test MoveFile moves file to new location
  - [x] Test MoveFile does nothing when source missing
  - [x] Test MoveDirectory moves directory to new location
  - [x] Test MoveDirectory does nothing when source missing
- [x] **Build**: Pass
- [x] **Tests**: Pass
- [x] **Coverage**: ≥80% on FileSystem class

---

## Slice 3: Audio File Discovery

**What it delivers**: Scan a directory and return all supported audio files

- [ ] Create `AudioFileScanner.cs` in FSMO/
  - [ ] `List<FileInfo> ScanDirectory(string sourcePath)` — recursively find all audio files
  - [ ] `bool IsSupportedFormat(string extension)` — check .mp3, .wav, .wma (case-insensitive)
  - [ ] Input validation: null/empty path, directory not found
- [ ] Create `AudioFileScannerTests.cs` in `FSMP.Tests/FSMO/`
  - [ ] Test ScanDirectory finds .mp3 files
  - [ ] Test ScanDirectory finds .wav files
  - [ ] Test ScanDirectory finds .wma files
  - [ ] Test ScanDirectory ignores unsupported formats (.txt, .jpg, .flac)
  - [ ] Test ScanDirectory searches subdirectories recursively
  - [ ] Test ScanDirectory returns empty list for empty directory
  - [ ] Test ScanDirectory throws on null/empty path
  - [ ] Test ScanDirectory throws on non-existent directory
  - [ ] Test IsSupportedFormat case-insensitive (.MP3, .Mp3)
  - [ ] Test IsSupportedFormat rejects unsupported extensions
- [ ] **Build**: Pass
- [ ] **Tests**: Pass
- [ ] **Coverage**: ≥80% on AudioFileScanner

---

## Slice 4: Metadata Reader

**What it delivers**: Extract artist, album, and title from audio file metadata via TagLibSharp

- [ ] Create `AudioMetadata.cs` POCO in FSMO/
  - [ ] Properties: Title, Artist, Album, TrackNumber, Year, Duration
- [ ] Create `MetadataReader.cs` in FSMO/
  - [ ] `AudioMetadata ReadMetadata(string filePath)` — read tags via TagLibSharp
  - [ ] Handle missing/empty tags (return null for each field independently)
  - [ ] Handle corrupt files (return metadata with all nulls, log error)
  - [ ] Input validation: null path, file not found
- [ ] Create `AudioMetadataTests.cs` in `FSMP.Tests/FSMO/`
  - [ ] Test default initialization (all properties null)
  - [ ] Test all property setters
- [ ] Create `MetadataReaderTests.cs` in `FSMP.Tests/FSMO/`
  - [ ] Test ReadMetadata extracts artist from MP3 (use sample file)
  - [ ] Test ReadMetadata extracts album from MP3
  - [ ] Test ReadMetadata extracts title from MP3
  - [ ] Test ReadMetadata extracts artist from WMA (use sample file)
  - [ ] Test ReadMetadata returns nulls for file with no tags (programmatic WAV)
  - [ ] Test ReadMetadata handles corrupt file gracefully
  - [ ] Test ReadMetadata throws on null path
  - [ ] Test ReadMetadata throws on missing file
- [ ] **Build**: Pass
- [ ] **Tests**: Pass
- [ ] **Coverage**: ≥80% on MetadataReader

---

## Slice 5: Path Builder

**What it delivers**: Given metadata, produce the organized target file path

- [ ] Create `PathBuilder.cs` in FSMO/
  - [ ] `string BuildTargetPath(string destinationRoot, AudioMetadata metadata, string originalFileName)` — returns `destination/Artist/Album/filename`
  - [ ] Fallback: "Unknown Artist" when artist is null/empty
  - [ ] Fallback: "Unknown Album" when album is null/empty
  - [ ] Sanitize folder names: remove invalid path characters
  - [ ] Trim whitespace from artist/album names
- [ ] Create `PathBuilderTests.cs` in `FSMP.Tests/FSMO/`
  - [ ] Test builds correct path with full metadata (Artist/Album/file.mp3)
  - [ ] Test falls back to "Unknown Artist" when artist is null
  - [ ] Test falls back to "Unknown Artist" when artist is empty/whitespace
  - [ ] Test falls back to "Unknown Album" when album is null
  - [ ] Test falls back to "Unknown Album" when album is empty/whitespace
  - [ ] Test sanitizes invalid path characters from artist name
  - [ ] Test sanitizes invalid path characters from album name
  - [ ] Test trims whitespace from artist and album
  - [ ] Test preserves original file name and extension
  - [ ] Test throws on null destination root
  - [ ] Test throws on null original file name
- [ ] **Build**: Pass
- [ ] **Tests**: Pass
- [ ] **Coverage**: ≥80% on PathBuilder

---

## Slice 6: File Organizer — Copy Mode

**What it delivers**: Copy audio files into organized directory structure

- [ ] Create `OrganizeMode.cs` enum in FSMO/ — `Copy`, `Move`
- [ ] Create `OrganizeResult.cs` POCO in FSMO/
  - [ ] Properties: FilesCopied, FilesMoved, FilesSkipped, Errors (List<string>)
- [ ] Create `FileOrganizer.cs` in FSMO/
  - [ ] Constructor with `AudioFileScanner`, `MetadataReader`, `PathBuilder`
  - [ ] `OrganizeResult Organize(string sourcePath, string destinationPath, OrganizeMode mode)` — main entry point
  - [ ] Copy mode: copy each file to target path, create directories as needed
  - [ ] Return result with counts and any errors
- [ ] Create `FileOrganizerTests.cs` in `FSMP.Tests/FSMO/`
  - [ ] Test Organize copy mode copies file to correct location
  - [ ] Test Organize copy mode creates Artist directory
  - [ ] Test Organize copy mode creates Album subdirectory
  - [ ] Test Organize copy mode preserves original file (source still exists)
  - [ ] Test Organize copy mode returns correct FilesCopied count
  - [ ] Test Organize copy mode handles multiple files
  - [ ] Test Organize copy mode handles files with no metadata (Unknown Artist/Album)
  - [ ] Test Organize throws on null source/destination
  - [ ] Test Organize throws on non-existent source directory
- [ ] **Build**: Pass
- [ ] **Tests**: Pass
- [ ] **Coverage**: ≥80% on FileOrganizer

---

## Slice 7: File Organizer — Move Mode

**What it delivers**: Move audio files into organized structure, clean up empty source directories

- [ ] Extend `FileOrganizer.Organize` with move mode support
  - [ ] Move mode: move each file to target path, create directories as needed
  - [ ] After moving, remove empty source directories (leaf-first cleanup)
- [ ] Add tests to `FileOrganizerTests.cs`
  - [ ] Test Organize move mode moves file to correct location
  - [ ] Test Organize move mode removes file from source
  - [ ] Test Organize move mode creates target directories
  - [ ] Test Organize move mode returns correct FilesMoved count
  - [ ] Test Organize move mode cleans up empty source directories
  - [ ] Test Organize move mode does not delete non-empty source directories
- [ ] **Build**: Pass
- [ ] **Tests**: Pass
- [ ] **Coverage**: ≥80% on FileOrganizer

---

## Slice 8: Duplicate Handling

**What it delivers**: Handle files that already exist at the destination

- [ ] Create `DuplicateStrategy.cs` enum in FSMO/ — `Skip`, `Overwrite`, `Rename`
- [ ] Extend `FileOrganizer.Organize` with `DuplicateStrategy` parameter (default: Skip)
  - [ ] Skip: do not copy/move, increment FilesSkipped count
  - [ ] Overwrite: replace existing file at destination
  - [ ] Rename: append `_1`, `_2`, etc. suffix before extension
- [ ] Add tests to `FileOrganizerTests.cs`
  - [ ] Test Skip strategy skips existing file and increments FilesSkipped
  - [ ] Test Overwrite strategy replaces existing file
  - [ ] Test Rename strategy creates file with _1 suffix
  - [ ] Test Rename strategy increments suffix (_1, _2, _3) for multiple duplicates
  - [ ] Test default strategy is Skip
- [ ] **Build**: Pass
- [ ] **Tests**: Pass
- [ ] **Coverage**: ≥80% on FileOrganizer

---

## Slice 9: DirectoryManager Integration

**What it delivers**: Wire the existing `DirectoryManager` stubs to use all new components

- [ ] Implement `DirectoryManager.ReorganiseDirectory(DirectoryInfo dir)`
  - [ ] Create `AudioFileScanner`, `MetadataReader`, `PathBuilder`, `FileOrganizer`
  - [ ] Call `FileOrganizer.Organize` with the source directory
- [ ] Implement `DirectoryManager.GetAllDistinctAudioFileNewDirectory()`
  - [ ] Return list of audio files found in a specified directory (deduplicated by file name)
- [ ] Create `DirectoryManagerTests.cs` in `FSMP.Tests/FSMO/`
  - [ ] Test ReorganiseDirectory organizes files into correct structure
  - [ ] Test ReorganiseDirectory with mixed formats (.mp3, .wav, .wma)
  - [ ] Test GetAllDistinctAudioFileNewDirectory returns unique files
  - [ ] Test GetAllDistinctAudioFileNewDirectory excludes unsupported formats
- [ ] **Build**: Pass
- [ ] **Tests**: Pass
- [ ] **Coverage**: ≥80% on DirectoryManager

---

## Slice 10: Edge Cases & Polish

**What it delivers**: Robust handling of real-world file system scenarios

- [ ] Handle special characters in artist/album names (e.g., AC/DC, Guns N' Roses)
- [ ] Handle very long path names (truncate to stay under MAX_PATH)
- [ ] Handle read-only source files gracefully
- [ ] Handle files with identical names but different content
- [ ] Handle empty artist AND album (file goes to Unknown Artist/Unknown Album/)
- [ ] Add error tests
  - [ ] Test special characters sanitized from folder names
  - [ ] Test long path names truncated safely
  - [ ] Test read-only file can still be copied
  - [ ] Test corrupt audio file is skipped with error logged
  - [ ] Test mixed valid/corrupt files — valid files still organized
- [ ] **Build**: Pass
- [ ] **Tests**: Pass
- [ ] **Coverage**: ≥80% overall on FSMO project

---

## Possible Extensions

- Project could be rewritten in C++ and integrated via P/Invoke for performance-critical operations

---

## Progress Summary

**Completed Slices**: 2 / 10
**Status**: FileSystem fully tested, ready for Slice 3
**Next Action**: Slice 3 — Audio File Discovery
