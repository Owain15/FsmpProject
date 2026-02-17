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

- [x] Create `AudioFileScanner.cs` in FSMO/
  - [x] `List<FileInfo> ScanDirectory(string sourcePath)` — recursively find all audio files
  - [x] `bool IsSupportedFormat(string extension)` — check .mp3, .wav, .wma (case-insensitive)
  - [x] Input validation: null/empty path, directory not found
- [x] Create `AudioFileScannerTests.cs` in `FSMP.Tests/FSMO/` (21 tests)
  - [x] Test ScanDirectory finds .mp3 files
  - [x] Test ScanDirectory finds .wav files
  - [x] Test ScanDirectory finds .wma files
  - [x] Test ScanDirectory ignores unsupported formats (.txt, .jpg, .flac)
  - [x] Test ScanDirectory searches subdirectories recursively
  - [x] Test ScanDirectory returns empty list for empty directory
  - [x] Test ScanDirectory throws on null/empty path
  - [x] Test ScanDirectory throws on non-existent directory
  - [x] Test IsSupportedFormat case-insensitive (.MP3, .Mp3)
  - [x] Test IsSupportedFormat rejects unsupported extensions
- [x] **Build**: Pass
- [x] **Tests**: Pass
- [x] **Coverage**: ≥80% on AudioFileScanner

---

## Slice 4: Metadata Reader

**What it delivers**: Extract artist, album, and title from audio file metadata via TagLibSharp

- [x] Create `AudioMetadata.cs` POCO in FSMO/
  - [x] Properties: Title, Artist, Album, TrackNumber, Year, Duration
- [x] Create `MetadataReader.cs` in FSMO/
  - [x] `AudioMetadata ReadMetadata(string filePath)` — read tags via TagLibSharp
  - [x] Handle missing/empty tags (return null for each field independently)
  - [x] Handle corrupt files (return metadata with all nulls)
  - [x] Input validation: null path, file not found
- [x] Create `AudioMetadataTests.cs` in `FSMP.Tests/FSMO/` (7 tests)
  - [x] Test default initialization (all properties null)
  - [x] Test all property setters
- [x] Create `MetadataReaderTests.cs` in `FSMP.Tests/FSMO/` (8 tests)
  - [x] Test ReadMetadata extracts artist from MP3 (use sample file)
  - [x] Test ReadMetadata extracts album from MP3
  - [x] Test ReadMetadata extracts title from MP3
  - [x] Test ReadMetadata extracts artist from WMA (use sample file)
  - [x] Test ReadMetadata returns nulls for file with no tags (programmatic WAV)
  - [x] Test ReadMetadata handles corrupt file gracefully
  - [x] Test ReadMetadata throws on null path
  - [x] Test ReadMetadata throws on missing file
- [x] **Build**: Pass
- [x] **Tests**: Pass
- [x] **Coverage**: 92.55% on FSMO (≥80% target met)

---

## Slice 5: Path Builder

**What it delivers**: Given metadata, produce the organized target file path

- [x] Create `PathBuilder.cs` in FSMO/
  - [x] `string BuildTargetPath(string destinationRoot, AudioMetadata metadata, string originalFileName)` — returns `destination/Artist/Album/filename`
  - [x] Fallback: "Unknown Artist" when artist is null/empty
  - [x] Fallback: "Unknown Album" when album is null/empty
  - [x] Sanitize folder names: remove invalid path characters
  - [x] Trim whitespace from artist/album names
- [x] Create `PathBuilderTests.cs` in `FSMP.Tests/FSMO/` (13 tests)
  - [x] Test builds correct path with full metadata (Artist/Album/file.mp3)
  - [x] Test falls back to "Unknown Artist" when artist is null
  - [x] Test falls back to "Unknown Artist" when artist is empty/whitespace
  - [x] Test falls back to "Unknown Album" when album is null
  - [x] Test falls back to "Unknown Album" when album is empty/whitespace
  - [x] Test sanitizes invalid path characters from artist name
  - [x] Test sanitizes invalid path characters from album name
  - [x] Test trims whitespace from artist and album
  - [x] Test preserves original file name and extension
  - [x] Test throws on null destination root
  - [x] Test throws on null original file name
- [x] **Build**: Pass
- [x] **Tests**: Pass
- [x] **Coverage**: 93.80% on FSMO (≥80% target met)

---

## Slice 6: File Organizer — Copy Mode

**What it delivers**: Copy audio files into organized directory structure

- [x] Create `OrganizeMode.cs` enum in FSMO/ — `Copy`, `Move`
- [x] Create `OrganizeResult.cs` POCO in FSMO/
  - [x] Properties: FilesCopied, FilesMoved, FilesSkipped, Errors (List<string>)
- [x] Create `FileOrganizer.cs` in FSMO/
  - [x] Static class calling AudioFileScanner, MetadataReader, PathBuilder
  - [x] `OrganizeResult Organize(string sourcePath, string destinationPath, OrganizeMode mode)` — main entry point
  - [x] Copy mode: copy each file to target path, create directories as needed
  - [x] Return result with counts and any errors
- [x] Create `FileOrganizerTests.cs` in `FSMP.Tests/FSMO/` (10 tests)
  - [x] Test Organize copy mode copies file to correct location
  - [x] Test Organize copy mode creates Artist directory
  - [x] Test Organize copy mode creates Album subdirectory
  - [x] Test Organize copy mode preserves original file (source still exists)
  - [x] Test Organize copy mode returns correct FilesCopied count
  - [x] Test Organize copy mode handles multiple files
  - [x] Test Organize copy mode handles files with no metadata (Unknown Artist/Album)
  - [x] Test Organize throws on null source/destination
  - [x] Test Organize throws on non-existent source directory
- [x] **Build**: Pass
- [x] **Tests**: Pass
- [x] **Coverage**: 89.26% on FSMO (≥80% target met)

---

## Slice 7: File Organizer — Move Mode

**What it delivers**: Move audio files into organized structure, clean up empty source directories

- [x] Extend `FileOrganizer.Organize` with move mode support
  - [x] Move mode: move each file to target path, create directories as needed
  - [x] After moving, remove empty source directories (leaf-first cleanup)
- [x] Add tests to `FileOrganizerTests.cs` (6 new tests, 16 total)
  - [x] Test Organize move mode moves file to correct location
  - [x] Test Organize move mode removes file from source
  - [x] Test Organize move mode creates target directories
  - [x] Test Organize move mode returns correct FilesMoved count
  - [x] Test Organize move mode cleans up empty source directories
  - [x] Test Organize move mode does not delete non-empty source directories
- [x] **Build**: Pass
- [x] **Tests**: Pass
- [x] **Coverage**: 92.59% on FSMO (≥80% target met)

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

**Completed Slices**: 7 / 10
**Status**: File organizer (copy + move modes) implemented and tested, ready for Slice 8
**Next Action**: Slice 8 — Duplicate Handling
