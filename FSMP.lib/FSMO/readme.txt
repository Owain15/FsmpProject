F.S.M.O
file system music organizer

Responsibilities
----------------
This project is responsible for:
- Organising music files into an Artist/Album/Track directory structure
- Scanning source directories for supported audio files
- Moving or copying files into the structured layout

This project is NOT responsible for:
- Audio playback (see FsmpLibrary / FSMP.Platform.Windows)
- Database access (see FsmpDataAcsses)
- UI or user interaction (see FsmpConsole)

How It Fits In:
  Business Logic Layer — provides file organisation services.
  Used by the UI layer for library management operations.

This is a simple command line tool to organize music files based on their metadata.
It can move or copy files into a structured directory format based on artist, album, and track information.

file structure:
Music -> Artist -> Album -> Track

where "Music" is the root directory specified by the user.

main features:

given a source directory, the tool will scan for music files (e.g., .mp3, .wav, .wmp) and restucture the directory aproreatly.

given a destination directory, the tool will move or copy the organized files to the specified location.