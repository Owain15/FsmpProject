namespace FSMP.Core.Interfaces;

/// <summary>
/// Represents the current state of audio playback.
/// </summary>
public enum PlaybackState
{
    /// <summary>Playback is stopped.</summary>
    Stopped,

    /// <summary>Media is loading.</summary>
    Loading,

    /// <summary>Media is currently playing.</summary>
    Playing,

    /// <summary>Playback is paused.</summary>
    Paused,

    /// <summary>An error occurred during playback.</summary>
    Error
}