FSMP - File System Music Player (Console)
==========================================

FSMP is a Windows console application for playing and managing audio files
(WAV, WMA, MP3) organised in an Artist/Album/Track directory structure.

Features
--------
- Browse & Play    : Navigate artists, albums and tracks; play audio files.

- Library Scanning : Point FSMP at one or more music folders and it imports
                     every supported file into a local SQLite database,
                     reading metadata (title, artist, album, duration, etc.)
                     via TagLibSharp.

- Metadata Editing : Search for a track and override its title, artist,
                     album, rating (1-5), favourite flag or comment.
                     Overrides are stored in the database; original files
                     are never modified.

- Statistics       : View total track count, play counts, most played,
                     recently played, favourites and genre breakdown.
                     
- Configuration    : Settings are stored in a JSON file at
                     %AppData%\FSMP\config.json (library paths, default
                     volume, auto-scan on startup, etc.).

Requirements
------------
- Windows 10 or later
- .NET 10.0 runtime
- Visual Studio 2022+ with the ".NET desktop development" workload
  (needed for MSBuild / COM interop during build)

Building
--------
From the repository root run:

    build.cmd

Or use MSBuild directly:

    "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" ^
      FSMP.UI\FSMP.UI.Console\FsmpConsole\FsmpConsole.slnx -t:Build -p:Configuration=Debug

Running
-------
After building:

    dotnet run --project FSMP.UI\FSMP.UI.Console\FsmpConsole\FsmpConsole\FsmpConsole.csproj

Or launch the compiled executable:

    FSMP.UI\FSMP.UI.Console\FsmpConsole\FsmpConsole\bin\Debug\net10.0\FsmpConsole.exe

On first launch FSMP creates a default config.json and an SQLite database
(fsmp.db) under %AppData%\FSMP\.

Usage
-----
The main menu offers these options:

    1) Browse & Play      - Browse artists/albums/tracks and play audio.
    2) Scan Libraries     - Scan all configured library paths for new files.
    3) View Statistics    - See play counts, favourites and listening time.
    4) Manage Libraries   - Add or remove library folder paths.
    5) Settings           - View current configuration.
    6) Exit

Typical first-time workflow:

    1. Choose option 4 (Manage Libraries) and add a music folder path.
    2. Choose option 2 (Scan Libraries) to import tracks.
    3. Choose option 1 (Browse & Play) to find and play music.

Testing
-------
Run the test suite from the repository root:

    test.cmd

Or manually:

    dotnet build FSMP.Tests\FSMP.Tests.csproj
    dotnet test  FSMP.Tests\FSMP.Tests.csproj --no-build

To include code coverage:

    test-with-coverage.cmd
