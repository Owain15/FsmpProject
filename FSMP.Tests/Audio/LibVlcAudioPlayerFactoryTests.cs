using FluentAssertions;
using FsmpLibrary.Audio;
using FsmpLibrary.Interfaces;
using FSMP.Tests.TestHelpers;

namespace FSMP.Tests.Audio;

public class LibVlcAudioPlayerFactoryTests
{
    [Fact]
    public void CreatePlayer_WithAdapterFactory_ShouldReturnLibVlcAudioPlayer()
    {
        var factory = new LibVlcAudioPlayerFactory(() => new MockMediaPlayerAdapter());

        var player = factory.CreatePlayer();

        player.Should().BeOfType<LibVlcAudioPlayer>();
    }

    [Fact]
    public void CreatePlayer_ShouldReturnNewInstanceEachCall()
    {
        var factory = new LibVlcAudioPlayerFactory(() => new MockMediaPlayerAdapter());

        var player1 = factory.CreatePlayer();
        var player2 = factory.CreatePlayer();

        player1.Should().NotBeSameAs(player2);
    }
}
