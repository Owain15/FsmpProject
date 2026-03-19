using FluentAssertions;
using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.Core.Services;
using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FSMP.Tests.MAUI;

/// <summary>
/// Verifies that the shared (non-platform-conditional) DI registrations from MauiProgram
/// resolve correctly. This mirrors the RegisterServices method without MAUI-specific types.
/// </summary>
public class ServiceRegistrationTests
{
    private ServiceProvider BuildSharedServiceProvider()
    {
        var services = new ServiceCollection();

        // Database — in-memory for testing
        services.AddDbContext<FsmpDbContext>(options =>
            options.UseInMemoryDatabase($"ServiceRegTest_{Guid.NewGuid()}"),
            ServiceLifetime.Scoped);

        services.AddScoped<UnitOfWork>();

        // Data services (same as MauiProgram)
        services.AddScoped<LibraryScanService>();
        services.AddScoped<PlaybackTrackingService>();
        services.AddScoped<StatisticsService>();
        services.AddScoped<PlaylistService>();
        services.AddScoped<MetadataService>();

        // Queue state persistence
        var queueStatePath = Path.Combine(Path.GetTempPath(), $"test-queue-{Guid.NewGuid()}.json");
        services.AddSingleton<IQueueStateRepository>(_ =>
            new FsmpDataAcsses.Repositories.JsonQueueStateRepository(queueStatePath));

        // Config service
        var configPath = Path.Combine(Path.GetTempPath(), $"test-config-{Guid.NewGuid()}.json");
        services.AddSingleton(_ => new ConfigurationService(configPath));

        // Audio — use mock factory since platform-specific
        services.AddSingleton<IAudioPlayerFactory>(new Moq.Mock<IAudioPlayerFactory>().Object);
        services.AddSingleton<IAudioService, AudioService>();
        services.AddSingleton<ActivePlaylistService>();
        services.AddSingleton<IActivePlaylistService>(sp => sp.GetRequiredService<ActivePlaylistService>());

        // Interface mappings
        services.AddScoped<IMetadataService>(sp => sp.GetRequiredService<MetadataService>());
        services.AddScoped<ILibraryScanService>(sp => sp.GetRequiredService<LibraryScanService>());
        services.AddScoped<IPlaylistService>(sp => sp.GetRequiredService<PlaylistService>());
        services.AddSingleton<IConfigurationService>(sp => sp.GetRequiredService<ConfigurationService>());

        // Repository interfaces
        services.AddScoped<ITrackRepository>(sp => sp.GetRequiredService<UnitOfWork>().Tracks);
        services.AddScoped<IArtistRepository>(sp => sp.GetRequiredService<UnitOfWork>().Artists);
        services.AddScoped<IAlbumRepository>(sp => sp.GetRequiredService<UnitOfWork>().Albums);
        services.AddScoped<ITagRepository>(sp => sp.GetRequiredService<UnitOfWork>().Tags);

        // Tag service
        services.AddScoped<ITagService>(sp => new TagService(
            sp.GetRequiredService<ITagRepository>(),
            sp.GetRequiredService<ITrackRepository>(),
            sp.GetRequiredService<IAlbumRepository>(),
            sp.GetRequiredService<IArtistRepository>(),
            sp.GetRequiredService<UnitOfWork>().SaveAsync));

        // Orchestration layer
        services.AddScoped<IPlaybackController, PlaybackController>();
        services.AddScoped<ILibraryBrowser>(sp => new LibraryBrowser(
            sp.GetRequiredService<IArtistRepository>(),
            sp.GetRequiredService<IAlbumRepository>(),
            sp.GetRequiredService<ITrackRepository>(),
            sp.GetRequiredService<ITagRepository>()));
        services.AddScoped<ILibraryManager, LibraryManager>();
        services.AddScoped<IPlaylistManager, PlaylistManager>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public void SharedServices_ShouldResolve_AudioService()
    {
        using var sp = BuildSharedServiceProvider();
        sp.GetRequiredService<IAudioService>().Should().NotBeNull();
    }

    [Fact]
    public void SharedServices_ShouldResolve_ActivePlaylistService()
    {
        using var sp = BuildSharedServiceProvider();
        sp.GetRequiredService<IActivePlaylistService>().Should().NotBeNull();
    }

    [Fact]
    public void SharedServices_ShouldResolve_ConfigurationService()
    {
        using var sp = BuildSharedServiceProvider();
        sp.GetRequiredService<IConfigurationService>().Should().NotBeNull();
    }

    [Fact]
    public void SharedServices_ShouldResolve_QueueStateRepository()
    {
        using var sp = BuildSharedServiceProvider();
        sp.GetRequiredService<IQueueStateRepository>().Should().NotBeNull();
    }

    [Fact]
    public void SharedServices_ShouldResolve_ScopedServices()
    {
        using var sp = BuildSharedServiceProvider();
        using var scope = sp.CreateScope();
        var s = scope.ServiceProvider;

        s.GetRequiredService<IMetadataService>().Should().NotBeNull();
        s.GetRequiredService<ILibraryScanService>().Should().NotBeNull();
        s.GetRequiredService<IPlaylistService>().Should().NotBeNull();
        s.GetRequiredService<IPlaybackController>().Should().NotBeNull();
        s.GetRequiredService<ILibraryBrowser>().Should().NotBeNull();
        s.GetRequiredService<ILibraryManager>().Should().NotBeNull();
        s.GetRequiredService<IPlaylistManager>().Should().NotBeNull();
    }

    [Fact]
    public void SharedServices_ShouldResolve_Repositories()
    {
        using var sp = BuildSharedServiceProvider();
        using var scope = sp.CreateScope();
        var s = scope.ServiceProvider;

        s.GetRequiredService<ITrackRepository>().Should().NotBeNull();
        s.GetRequiredService<IArtistRepository>().Should().NotBeNull();
        s.GetRequiredService<IAlbumRepository>().Should().NotBeNull();
        s.GetRequiredService<ITagRepository>().Should().NotBeNull();
    }

    [Fact]
    public void SharedServices_ShouldResolve_TagService()
    {
        using var sp = BuildSharedServiceProvider();
        using var scope = sp.CreateScope();
        scope.ServiceProvider.GetRequiredService<ITagService>().Should().NotBeNull();
    }

    [Fact]
    public void SharedServices_ActivePlaylistService_ShouldBeSingleton()
    {
        using var sp = BuildSharedServiceProvider();
        var first = sp.GetRequiredService<IActivePlaylistService>();
        var second = sp.GetRequiredService<IActivePlaylistService>();
        first.Should().BeSameAs(second);
    }

    [Fact]
    public void SharedServices_ConfigurationService_ShouldBeSingleton()
    {
        using var sp = BuildSharedServiceProvider();
        var first = sp.GetRequiredService<IConfigurationService>();
        var second = sp.GetRequiredService<IConfigurationService>();
        first.Should().BeSameAs(second);
    }

    [Fact]
    public void SharedServices_ScopedServices_ShouldCreateNewInstancesPerScope()
    {
        using var sp = BuildSharedServiceProvider();
        IPlaybackController first, second;

        using (var scope1 = sp.CreateScope())
            first = scope1.ServiceProvider.GetRequiredService<IPlaybackController>();

        using (var scope2 = sp.CreateScope())
            second = scope2.ServiceProvider.GetRequiredService<IPlaybackController>();

        first.Should().NotBeSameAs(second);
    }
}
