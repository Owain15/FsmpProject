using Microsoft.EntityFrameworkCore;
using FsmpLibrary.Models;

namespace FsmpDataAcsses;

/// <summary>
/// Entity Framework Core database context for FSMP music player.
/// Manages all entity sets and configures relationships.
/// </summary>
public class FsmpDbContext : DbContext
{
    public FsmpDbContext(DbContextOptions<FsmpDbContext> options) : base(options)
    {
    }

    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<FileExtension> FileExtensions => Set<FileExtension>();
    public DbSet<PlaybackHistory> PlaybackHistories => Set<PlaybackHistory>();
    public DbSet<LibraryPath> LibraryPaths => Set<LibraryPath>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<PlaylistTrack> PlaylistTracks => Set<PlaylistTrack>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureTrack(modelBuilder);
        ConfigureAlbum(modelBuilder);
        ConfigureArtist(modelBuilder);
        ConfigureGenre(modelBuilder);
        ConfigureFileExtension(modelBuilder);
        ConfigurePlaybackHistory(modelBuilder);
        ConfigureLibraryPath(modelBuilder);
        ConfigurePlaylist(modelBuilder);
        ConfigurePlaylistTrack(modelBuilder);

        SeedData(modelBuilder);
    }

    private static void ConfigureTrack(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasKey(t => t.TrackId);

            entity.HasIndex(t => t.FilePath)
                .IsUnique();

            entity.HasIndex(t => t.FileHash);

            entity.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(t => t.FilePath)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(t => t.FileHash)
                .HasMaxLength(64);

            entity.Property(t => t.CustomTitle)
                .HasMaxLength(500);

            entity.Property(t => t.CustomArtist)
                .HasMaxLength(500);

            entity.Property(t => t.CustomAlbum)
                .HasMaxLength(500);

            entity.Property(t => t.Comment)
                .HasMaxLength(2000);

            // Relationship to Artist (many-to-one, nullable)
            entity.HasOne(t => t.Artist)
                .WithMany(a => a.Tracks)
                .HasForeignKey(t => t.ArtistId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relationship to Album (many-to-one, nullable)
            entity.HasOne(t => t.Album)
                .WithMany(a => a.Tracks)
                .HasForeignKey(t => t.AlbumId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relationship to FileExtension (many-to-one, nullable)
            entity.HasOne(t => t.FileExtension)
                .WithMany(fe => fe.Tracks)
                .HasForeignKey(t => t.FileExtensionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Many-to-many relationship with Genre (implicit junction table)
            entity.HasMany(t => t.Genres)
                .WithMany(g => g.Tracks)
                .UsingEntity(j => j.ToTable("TrackGenre"));
        });
    }

    private static void ConfigureAlbum(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Album>(entity =>
        {
            entity.HasKey(a => a.AlbumId);

            entity.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(a => a.AlbumArtistName)
                .HasMaxLength(500);

            entity.Property(a => a.AlbumArtPath)
                .HasMaxLength(1000);

            // Relationship to Artist (many-to-one, nullable)
            entity.HasOne(a => a.Artist)
                .WithMany(ar => ar.Albums)
                .HasForeignKey(a => a.ArtistId)
                .OnDelete(DeleteBehavior.SetNull);

            // Many-to-many relationship with Genre (implicit junction table)
            entity.HasMany(a => a.Genres)
                .WithMany(g => g.Albums)
                .UsingEntity(j => j.ToTable("AlbumGenre"));
        });
    }

    private static void ConfigureArtist(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasKey(a => a.ArtistId);

            entity.HasIndex(a => a.Name);

            entity.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(a => a.SortName)
                .HasMaxLength(500);

            entity.Property(a => a.Biography)
                .HasMaxLength(10000);

            // Many-to-many relationship with Genre (implicit junction table)
            entity.HasMany(a => a.Genres)
                .WithMany(g => g.Artists)
                .UsingEntity(j => j.ToTable("ArtistGenre"));
        });
    }

    private static void ConfigureGenre(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(g => g.GenreId);

            entity.HasIndex(g => g.Name)
                .IsUnique();

            entity.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(100);
        });
    }

    private static void ConfigureFileExtension(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileExtension>(entity =>
        {
            entity.HasKey(fe => fe.FileExtensionId);

            entity.HasIndex(fe => fe.Extension)
                .IsUnique();

            entity.Property(fe => fe.Extension)
                .IsRequired()
                .HasMaxLength(10);
        });
    }

    private static void ConfigurePlaybackHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlaybackHistory>(entity =>
        {
            entity.HasKey(ph => ph.PlaybackHistoryId);

            entity.HasIndex(ph => ph.PlayedAt);

            // Relationship to Track (many-to-one, required with cascade delete)
            entity.HasOne(ph => ph.Track)
                .WithMany(t => t.PlaybackHistories)
                .HasForeignKey(ph => ph.TrackId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });
    }

    private static void ConfigureLibraryPath(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LibraryPath>(entity =>
        {
            entity.HasKey(lp => lp.LibraryPathId);

            entity.HasIndex(lp => lp.Path)
                .IsUnique();

            entity.Property(lp => lp.Path)
                .IsRequired()
                .HasMaxLength(1000);
        });
    }

    private static void ConfigurePlaylist(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasKey(p => p.PlaylistId);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(p => p.Description)
                .HasMaxLength(2000);
        });
    }

    private static void ConfigurePlaylistTrack(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlaylistTrack>(entity =>
        {
            entity.HasKey(pt => pt.PlaylistTrackId);

            entity.HasIndex(pt => new { pt.PlaylistId, pt.Position });

            // Relationship to Playlist (many-to-one, required with cascade delete)
            entity.HasOne(pt => pt.Playlist)
                .WithMany(p => p.PlaylistTracks)
                .HasForeignKey(pt => pt.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Relationship to Track (many-to-one, required with cascade delete)
            entity.HasOne(pt => pt.Track)
                .WithMany(t => t.PlaylistTracks)
                .HasForeignKey(pt => pt.TrackId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Genre lookup values
        modelBuilder.Entity<Genre>().HasData(
            new Genre { GenreId = 1, Name = "Rock" },
            new Genre { GenreId = 2, Name = "Jazz" },
            new Genre { GenreId = 3, Name = "Classic" },
            new Genre { GenreId = 4, Name = "Metal" },
            new Genre { GenreId = 5, Name = "Comedy" }
        );

        // Seed FileExtension lookup values
        modelBuilder.Entity<FileExtension>().HasData(
            new FileExtension { FileExtensionId = 1, Extension = "wav" },
            new FileExtension { FileExtensionId = 2, Extension = "wma" },
            new FileExtension { FileExtensionId = 3, Extension = "mp3" }
        );
    }
}
