namespace FSMP.Core.Interfaces;

/// <summary>
/// Factory for creating audio player instances. Enables proper lifetime management and testing.
/// </summary>
public interface IAudioPlayerFactory
{
    /// <summary>
    /// Creates a new audio player instance.
    /// </summary>
    /// <returns>A new <see cref="IAudioPlayer"/> instance.</returns>
    IAudioPlayer CreatePlayer();

    /// <summary>
    /// Eagerly initializes the underlying audio engine. Returns true if successful.
    /// </summary>
    Task<bool> InitializeAsync();
}