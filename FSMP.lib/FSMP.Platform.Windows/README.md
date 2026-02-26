# FSMP.Platform.Windows

Windows-specific audio playback implementation for FSMP.

**Status:** Planned — not yet implemented.

## Responsibilities

**This project is responsible for:**
- Windows-specific IAudioPlayer implementation
- LibVLCSharp / WMPLib integration for audio playback on Windows
- Any other Windows-specific platform services

**This project is NOT responsible for:**
- UI or user interaction (see FsmpConsole / FSMP.MAUI)
- Business logic or orchestration (see FsmpLibrary / FSMP.Core)
- Cross-platform code or abstractions (see FSMP.Core)
- Database access (see FsmpDataAcsses)

## How It Fits In

Platform Layer — implements interfaces defined in FSMP.Core. Injected at startup by the host application (console or MAUI) via dependency injection.
