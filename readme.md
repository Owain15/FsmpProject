# FSMP — File System Music Player

A Windows console application for playing audio files organized in an Artist / Album / Track directory structure.

---

## Specification

| Item | Detail |
|---|---|
| Platform | Windows 10/11 |
| Language | C# / .NET 10.0 |
| Architecture | Three-tier: Console UI → FsmpLibrary → FsmpDataAcsses |
| Target Architecture | 64bit / arm64 |
| Audio Formats | WAV, WMA, MP3 |
| Playback Engine | LibVLCSharp (cross-platform VLC wrapper) — supports WAV, WMA, MP3, and 100+ formats |
| Persistence | SQLite via Entity Framework Core 9.0 |
| Metadata | Non-destructive — originals read with TagLibSharp; custom edits stored in the database |
| Configuration | JSON at `%AppData%\FSMP\config.json` |
| Testing | xUnit · Moq · FluentAssertions · 80 % code-coverage minimum |

---

## What It Does

- Scans one or more configured music-library directories for supported audio files
- Plays WAV, WMA, MP3 (and 100+ other formats) using LibVLCSharp
- Reads file metadata (title, artist, album, duration, bitrate, album art) via TagLibSharp
- Lets users override metadata in the database without touching the original files
- Tracks playback history, play counts, skip counts, favorites, and star ratings
- Tags tracks with genres using a lookup table (many-to-many with Artist, Album, and Track)
- Detects duplicate files by SHA-256 hash on import
- Exposes an interactive console menu: browse library, play tracks, edit metadata, manage libraries, view statistics

---

## Status

**Working** — Console UI, WAV/WMA/MP3 playback via LibVLCSharp, all entity models (Track, Album, Artist, Genre, FileExtension, PlaybackHistory), dependency injection, IAudioPlayer abstraction, 51 passing unit tests.

**In Progress** — EF Core DbContext, repository pattern, configuration/metadata/scan/playback services, full menu system.

**Planned** — Cross-platform support (Windows + Android) via .NET MAUI with ExoPlayer FFmpeg for WMA on Android.
