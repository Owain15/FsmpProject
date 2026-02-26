# FsmpLibrary

Core business logic for FSMP — audio playback orchestration, service interfaces, and metadata management.

## Responsibilities

**This project is responsible for:**
- Core business logic and service interfaces
- Audio playback orchestration (via IAudioPlayer abstraction)
- Metadata reading and management (TagLibSharp)
- Library scanning and track discovery
- Configuration management

**This project is NOT responsible for:**
- UI or user interaction (see FsmpConsole / FSMP.MAUI)
- Direct database access or queries (see FsmpDataAcsses)
- Platform-specific playback implementation (see FSMP.Platform.Windows)

## How It Fits In

Business Logic Layer — sits between the UI layer and the data access layer. The UI projects depend on this project for all domain operations. This project depends on FsmpDataAcsses for persistence.
