# FSMP.Platform.Windows

Windows-specific audio playback implementation for FSMP.

**Status:** Complete

## Responsibilities

**This project is responsible for:**
- Windows-specific IAudioPlayer implementation (LibVlcAudioPlayer)
- IAudioPlayerFactory implementation (LibVlcAudioPlayerFactory)
- LibVLCSharp integration for audio playback on Windows (WAV, MP3, WMA)
- IMediaPlayerAdapter abstraction for testability

**This project is NOT responsible for:**
- UI or user interaction (see FsmpConsole / FSMP.MAUI)
- Business logic or orchestration (see FSMP.Core)
- Cross-platform code or abstractions (see FSMP.Core)
- Database access (see FsmpDataAcsses)

## How It Fits In

Platform Layer — implements interfaces defined in FSMP.Core. Injected at startup by the host application (console or MAUI) via dependency injection.
