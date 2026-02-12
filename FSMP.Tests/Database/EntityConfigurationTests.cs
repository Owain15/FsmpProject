using FluentAssertions;
using FsmpDataAcsses;
using FsmpLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.Database;

public class EntityConfigurationTests : IDisposable
{
    private readonly FsmpDbContext _context;

    public EntityConfigurationTests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private static FsmpDbContext CreateSqliteContext()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        var ctx = new FsmpDbContext(options);
        ctx.Database.OpenConnection();
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public void Track_FilePath_ShouldBeUnique()
    {
        using var ctx = CreateSqliteContext();

        var track1 = new Track { Title = "Song 1", FilePath = @"C:\Music\same.mp3", FileHash = "hash1" };
        var track2 = new Track { Title = "Song 2", FilePath = @"C:\Music\same.mp3", FileHash = "hash2" };

        ctx.Tracks.Add(track1);
        ctx.SaveChanges();

        ctx.Tracks.Add(track2);
        var act = () => ctx.SaveChanges();

        act.Should().Throw<DbUpdateException>();
    }

    [Fact]
    public void Album_Artist_Relationship_ShouldBeNullable()
    {
        var album = new Album { Title = "Orphan Album" };

        _context.Albums.Add(album);
        _context.SaveChanges();

        var retrieved = _context.Albums.First(a => a.Title == "Orphan Album");
        retrieved.ArtistId.Should().BeNull();
        retrieved.Artist.Should().BeNull();
    }

    [Fact]
    public void Album_Artist_Relationship_ShouldWorkWhenSet()
    {
        var artist = new Artist { Name = "Test Artist" };
        _context.Artists.Add(artist);
        _context.SaveChanges();

        var album = new Album { Title = "Linked Album", ArtistId = artist.ArtistId };
        _context.Albums.Add(album);
        _context.SaveChanges();

        var retrieved = _context.Albums
            .Include(a => a.Artist)
            .First(a => a.Title == "Linked Album");

        retrieved.ArtistId.Should().Be(artist.ArtistId);
        retrieved.Artist.Should().NotBeNull();
        retrieved.Artist!.Name.Should().Be("Test Artist");
    }

    [Fact]
    public void Track_Album_Relationship_ShouldBeNullable()
    {
        var track = new Track { Title = "Loose Track", FilePath = @"C:\track.mp3", FileHash = "hash1" };

        _context.Tracks.Add(track);
        _context.SaveChanges();

        var retrieved = _context.Tracks.First(t => t.Title == "Loose Track");
        retrieved.AlbumId.Should().BeNull();
        retrieved.Album.Should().BeNull();
    }

    [Fact]
    public void Track_Album_Relationship_ShouldWorkWhenSet()
    {
        var album = new Album { Title = "Test Album" };
        _context.Albums.Add(album);
        _context.SaveChanges();

        var track = new Track
        {
            Title = "Album Track",
            FilePath = @"C:\album-track.mp3",
            FileHash = "hash1",
            AlbumId = album.AlbumId
        };
        _context.Tracks.Add(track);
        _context.SaveChanges();

        var retrieved = _context.Tracks
            .Include(t => t.Album)
            .First(t => t.Title == "Album Track");

        retrieved.AlbumId.Should().Be(album.AlbumId);
        retrieved.Album.Should().NotBeNull();
        retrieved.Album!.Title.Should().Be("Test Album");
    }

    [Fact]
    public void PlaybackHistory_Track_CascadeDelete()
    {
        var track = new Track { Title = "Delete Me", FilePath = @"C:\delete.mp3", FileHash = "hash1" };
        _context.Tracks.Add(track);
        _context.SaveChanges();

        var history = new PlaybackHistory
        {
            TrackId = track.TrackId,
            PlayedAt = DateTime.UtcNow,
            CompletedPlayback = true
        };
        _context.PlaybackHistories.Add(history);
        _context.SaveChanges();

        _context.PlaybackHistories.Should().HaveCount(1);

        _context.Tracks.Remove(track);
        _context.SaveChanges();

        _context.PlaybackHistories.Should().BeEmpty();
    }

    [Fact]
    public void LibraryPath_Path_ShouldBeUnique()
    {
        using var ctx = CreateSqliteContext();

        var path1 = new LibraryPath { Path = @"C:\Music", AddedAt = DateTime.UtcNow };
        var path2 = new LibraryPath { Path = @"C:\Music", AddedAt = DateTime.UtcNow };

        ctx.LibraryPaths.Add(path1);
        ctx.SaveChanges();

        ctx.LibraryPaths.Add(path2);
        var act = () => ctx.SaveChanges();

        act.Should().Throw<DbUpdateException>();
    }

    [Fact]
    public void Track_Genre_ManyToMany_ShouldWork()
    {
        var rock = _context.Genres.First(g => g.Name == "Rock");
        var metal = _context.Genres.First(g => g.Name == "Metal");

        var track = new Track
        {
            Title = "Rock Metal Song",
            FilePath = @"C:\rockmetal.mp3",
            FileHash = "hash1"
        };
        track.Genres.Add(rock);
        track.Genres.Add(metal);

        _context.Tracks.Add(track);
        _context.SaveChanges();

        var retrieved = _context.Tracks
            .Include(t => t.Genres)
            .First(t => t.Title == "Rock Metal Song");

        retrieved.Genres.Should().HaveCount(2);
        retrieved.Genres.Select(g => g.Name).Should().Contain("Rock");
        retrieved.Genres.Select(g => g.Name).Should().Contain("Metal");
    }

    [Fact]
    public void Album_Genre_ManyToMany_ShouldWork()
    {
        var jazz = _context.Genres.First(g => g.Name == "Jazz");

        var album = new Album { Title = "Jazz Album" };
        album.Genres.Add(jazz);

        _context.Albums.Add(album);
        _context.SaveChanges();

        var retrieved = _context.Albums
            .Include(a => a.Genres)
            .First(a => a.Title == "Jazz Album");

        retrieved.Genres.Should().HaveCount(1);
        retrieved.Genres.First().Name.Should().Be("Jazz");
    }

    [Fact]
    public void Artist_Genre_ManyToMany_ShouldWork()
    {
        var classic = _context.Genres.First(g => g.Name == "Classic");

        var artist = new Artist { Name = "Classical Artist" };
        artist.Genres.Add(classic);

        _context.Artists.Add(artist);
        _context.SaveChanges();

        var retrieved = _context.Artists
            .Include(a => a.Genres)
            .First(a => a.Name == "Classical Artist");

        retrieved.Genres.Should().HaveCount(1);
        retrieved.Genres.First().Name.Should().Be("Classic");
    }

    [Fact]
    public void Track_FileExtension_Relationship_ShouldWork()
    {
        var mp3 = _context.FileExtensions.First(fe => fe.Extension == "mp3");

        var track = new Track
        {
            Title = "MP3 Track",
            FilePath = @"C:\test.mp3",
            FileHash = "hash1",
            FileExtensionId = mp3.FileExtensionId
        };

        _context.Tracks.Add(track);
        _context.SaveChanges();

        var retrieved = _context.Tracks
            .Include(t => t.FileExtension)
            .First(t => t.Title == "MP3 Track");

        retrieved.FileExtension.Should().NotBeNull();
        retrieved.FileExtension!.Extension.Should().Be("mp3");
    }

    [Fact]
    public void Genre_Name_ShouldBeUnique()
    {
        using var ctx = CreateSqliteContext();

        var duplicate = new Genre { Name = "Rock" };

        ctx.Genres.Add(duplicate);
        var act = () => ctx.SaveChanges();

        act.Should().Throw<DbUpdateException>();
    }

    [Fact]
    public void FileExtension_Extension_ShouldBeUnique()
    {
        using var ctx = CreateSqliteContext();

        var duplicate = new FileExtension { Extension = "mp3" };

        ctx.FileExtensions.Add(duplicate);
        var act = () => ctx.SaveChanges();

        act.Should().Throw<DbUpdateException>();
    }

    [Fact]
    public void Playlist_CanBeCreated()
    {
        var playlist = new Playlist
        {
            Name = "My Playlist",
            Description = "A test playlist",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Playlists.Add(playlist);
        _context.SaveChanges();

        var retrieved = _context.Playlists.First(p => p.Name == "My Playlist");
        retrieved.Name.Should().Be("My Playlist");
        retrieved.Description.Should().Be("A test playlist");
        retrieved.PlaylistId.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Playlist_Description_ShouldBeOptional()
    {
        var playlist = new Playlist
        {
            Name = "No Description Playlist",
            Description = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Playlists.Add(playlist);
        _context.SaveChanges();

        var retrieved = _context.Playlists.First(p => p.Name == "No Description Playlist");
        retrieved.Description.Should().BeNull();
    }

    [Fact]
    public void Playlist_PlaylistTracks_Navigation_ShouldWork()
    {
        var track = new Track { Title = "Nav Track", FilePath = @"C:\nav.mp3", FileHash = "navhash" };
        _context.Tracks.Add(track);
        _context.SaveChanges();

        var playlist = new Playlist
        {
            Name = "Nav Playlist",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        playlist.PlaylistTracks.Add(new PlaylistTrack
        {
            TrackId = track.TrackId,
            Position = 0,
            AddedAt = DateTime.UtcNow
        });

        _context.Playlists.Add(playlist);
        _context.SaveChanges();

        var retrieved = _context.Playlists
            .Include(p => p.PlaylistTracks)
            .First(p => p.Name == "Nav Playlist");

        retrieved.PlaylistTracks.Should().HaveCount(1);
        retrieved.PlaylistTracks.First().TrackId.Should().Be(track.TrackId);
    }

    [Fact]
    public void PlaylistTrack_Playlist_Relationship_ShouldWork()
    {
        var track = new Track { Title = "PT Track", FilePath = @"C:\pt.mp3", FileHash = "pthash" };
        _context.Tracks.Add(track);
        var playlist = new Playlist
        {
            Name = "PT Playlist",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Playlists.Add(playlist);
        _context.SaveChanges();

        var playlistTrack = new PlaylistTrack
        {
            PlaylistId = playlist.PlaylistId,
            TrackId = track.TrackId,
            Position = 0,
            AddedAt = DateTime.UtcNow
        };
        _context.PlaylistTracks.Add(playlistTrack);
        _context.SaveChanges();

        var retrieved = _context.PlaylistTracks
            .Include(pt => pt.Playlist)
            .First(pt => pt.PlaylistTrackId == playlistTrack.PlaylistTrackId);

        retrieved.Playlist.Should().NotBeNull();
        retrieved.Playlist!.Name.Should().Be("PT Playlist");
    }

    [Fact]
    public void PlaylistTrack_Track_Relationship_ShouldWork()
    {
        var track = new Track { Title = "Rel Track", FilePath = @"C:\rel.mp3", FileHash = "relhash" };
        _context.Tracks.Add(track);
        var playlist = new Playlist
        {
            Name = "Rel Playlist",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Playlists.Add(playlist);
        _context.SaveChanges();

        var playlistTrack = new PlaylistTrack
        {
            PlaylistId = playlist.PlaylistId,
            TrackId = track.TrackId,
            Position = 0,
            AddedAt = DateTime.UtcNow
        };
        _context.PlaylistTracks.Add(playlistTrack);
        _context.SaveChanges();

        var retrieved = _context.PlaylistTracks
            .Include(pt => pt.Track)
            .First(pt => pt.PlaylistTrackId == playlistTrack.PlaylistTrackId);

        retrieved.Track.Should().NotBeNull();
        retrieved.Track!.Title.Should().Be("Rel Track");
    }

    [Fact]
    public void PlaylistTrack_Playlist_CascadeDelete()
    {
        var track = new Track { Title = "Cascade Track", FilePath = @"C:\cascade.mp3", FileHash = "cascadehash" };
        _context.Tracks.Add(track);
        var playlist = new Playlist
        {
            Name = "Cascade Playlist",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Playlists.Add(playlist);
        _context.SaveChanges();

        var playlistTrack = new PlaylistTrack
        {
            PlaylistId = playlist.PlaylistId,
            TrackId = track.TrackId,
            Position = 0,
            AddedAt = DateTime.UtcNow
        };
        _context.PlaylistTracks.Add(playlistTrack);
        _context.SaveChanges();

        _context.PlaylistTracks.Should().HaveCount(1);

        _context.Playlists.Remove(playlist);
        _context.SaveChanges();

        _context.PlaylistTracks.Should().BeEmpty();
    }

    [Fact]
    public void PlaylistTrack_Track_CascadeDelete()
    {
        var track = new Track { Title = "TCascade Track", FilePath = @"C:\tcascade.mp3", FileHash = "tcascadehash" };
        _context.Tracks.Add(track);
        var playlist = new Playlist
        {
            Name = "TCascade Playlist",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Playlists.Add(playlist);
        _context.SaveChanges();

        var playlistTrack = new PlaylistTrack
        {
            PlaylistId = playlist.PlaylistId,
            TrackId = track.TrackId,
            Position = 0,
            AddedAt = DateTime.UtcNow
        };
        _context.PlaylistTracks.Add(playlistTrack);
        _context.SaveChanges();

        _context.PlaylistTracks.Should().HaveCount(1);

        _context.Tracks.Remove(track);
        _context.SaveChanges();

        _context.PlaylistTracks.Should().BeEmpty();
    }
}
