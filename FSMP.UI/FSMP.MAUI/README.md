# FSMP.MAUI

Cross-platform MAUI UI for FSMP — Windows and Android.

**Status:** Planned — not yet implemented.

## Responsibilities

**This project is responsible for:**
- Cross-platform UI using .NET MAUI (Views, ViewModels)
- Platform bootstrapping and dependency injection setup
- Navigation and page routing
- Platform-specific UI configuration (Platforms/ folders)

**This project is NOT responsible for:**
- Business logic or orchestration (see FsmpLibrary / FSMP.Core)
- Database access (see FsmpDataAcsses)
- Audio playback implementation (see FSMP.Platform.Windows / FSMP.Platform.Android)

## How It Fits In

UI Layer — the graphical counterpart to FsmpConsole. Depends on FSMP.Core for business logic and FsmpDataAcsses for persistence. Will register platform-specific implementations via dependency injection.
