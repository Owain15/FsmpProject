# FSMP.Core

Cross-platform abstractions and platform-agnostic services for FSMP.

**Status:** Planned — not yet implemented.

## Responsibilities

**This project is responsible for:**
- Cross-platform abstractions (IAudioPlayer interface)
- Platform-agnostic service implementations
- Shared models used across platforms

**This project is NOT responsible for:**
- Platform-specific implementations (see FSMP.Platform.Windows / FSMP.Platform.Android)
- UI or user interaction (see FsmpConsole / FSMP.MAUI)
- Database access (see FsmpDataAcsses)

## How It Fits In

Business Logic Layer — provides the abstractions that platform projects implement. Will be consumed by both the console app and the MAUI app.
