using FsmpLibrary.Interfaces;

namespace FSMP.Tests.TestHelpers;

/// <summary>
/// Mock factory that returns a pre-configured MockAudioPlayer.
/// </summary>
public class MockAudioPlayerFactory : IAudioPlayerFactory
{
    private readonly MockAudioPlayer _player;

    public MockAudioPlayerFactory(MockAudioPlayer player)
    {
        _player = player;
    }

    public IAudioPlayer CreatePlayer() => _player;
}
