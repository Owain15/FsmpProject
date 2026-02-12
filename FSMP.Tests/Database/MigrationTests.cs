using FluentAssertions;
using FsmpDataAcsses;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.Database;

public class MigrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly FsmpDbContext _context;

    public MigrationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new FsmpDbContext(options);
        _context.Database.Migrate();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private List<string> GetTableNames()
    {
        var tables = new List<string>();
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name != '__EFMigrationsHistory' ORDER BY name;";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }
        return tables;
    }

    private List<(string Name, bool Unique)> GetIndexes(string tableName)
    {
        var indexes = new List<(string Name, bool Unique)>();
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = $"SELECT name, [unique] FROM pragma_index_list('{tableName}');";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            indexes.Add((reader.GetString(0), reader.GetBoolean(1)));
        }
        return indexes;
    }

    private List<(int Id, int Seq, string Table, string From, string To, string OnDelete)> GetForeignKeys(string tableName)
    {
        var fks = new List<(int, int, string, string, string, string)>();
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = $"PRAGMA foreign_key_list('{tableName}');";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            fks.Add((
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(6)
            ));
        }
        return fks;
    }

    [Fact]
    public void Migration_ShouldApplySuccessfully()
    {
        // If we got here without exception, the migration applied successfully
        _context.Database.GetAppliedMigrations().Should().HaveCount(2);
    }

    [Fact]
    public void Migration_ShouldCreateAllCoreTables()
    {
        var tables = GetTableNames();

        tables.Should().Contain("Artists");
        tables.Should().Contain("Albums");
        tables.Should().Contain("Tracks");
        tables.Should().Contain("PlaybackHistories");
        tables.Should().Contain("LibraryPaths");
    }

    [Fact]
    public void Migration_ShouldCreateLookupTables()
    {
        var tables = GetTableNames();

        tables.Should().Contain("Genres");
        tables.Should().Contain("FileExtensions");
    }

    [Fact]
    public void Migration_ShouldCreateJunctionTables()
    {
        var tables = GetTableNames();

        tables.Should().Contain("TrackGenre");
        tables.Should().Contain("AlbumGenre");
        tables.Should().Contain("ArtistGenre");
    }

    [Fact]
    public void Track_FilePath_ShouldHaveUniqueIndex()
    {
        var indexes = GetIndexes("Tracks");

        indexes.Should().Contain(i => i.Name == "IX_Tracks_FilePath" && i.Unique);
    }

    [Fact]
    public void Track_FileHash_ShouldHaveIndex()
    {
        var indexes = GetIndexes("Tracks");

        indexes.Should().Contain(i => i.Name == "IX_Tracks_FileHash");
    }

    [Fact]
    public void Album_Artist_Relationship_ShouldExist()
    {
        var fks = GetForeignKeys("Albums");

        fks.Should().Contain(fk => fk.Table == "Artists" && fk.From == "ArtistId" && fk.OnDelete == "SET NULL");
    }

    [Fact]
    public void PlaybackHistory_Track_CascadeDelete_ShouldBeConfigured()
    {
        var fks = GetForeignKeys("PlaybackHistories");

        fks.Should().Contain(fk => fk.Table == "Tracks" && fk.From == "TrackId" && fk.OnDelete == "CASCADE");
    }

    [Fact]
    public void LibraryPath_Path_ShouldHaveUniqueIndex()
    {
        var indexes = GetIndexes("LibraryPaths");

        indexes.Should().Contain(i => i.Name == "IX_LibraryPaths_Path" && i.Unique);
    }

    [Fact]
    public void Genre_SeedData_ShouldBePresent()
    {
        var genres = _context.Genres.OrderBy(g => g.GenreId).ToList();

        genres.Should().HaveCount(5);
        genres.Select(g => g.Name).Should().ContainInOrder("Rock", "Jazz", "Classic", "Metal", "Comedy");
    }

    [Fact]
    public void FileExtension_SeedData_ShouldBePresent()
    {
        var extensions = _context.FileExtensions.OrderBy(fe => fe.FileExtensionId).ToList();

        extensions.Should().HaveCount(3);
        extensions.Select(fe => fe.Extension).Should().ContainInOrder("wav", "wma", "mp3");
    }

    [Fact]
    public void Track_Artist_Relationship_ShouldSetNullOnDelete()
    {
        var fks = GetForeignKeys("Tracks");

        fks.Should().Contain(fk => fk.Table == "Artists" && fk.From == "ArtistId" && fk.OnDelete == "SET NULL");
    }

    [Fact]
    public void Track_Album_Relationship_ShouldSetNullOnDelete()
    {
        var fks = GetForeignKeys("Tracks");

        fks.Should().Contain(fk => fk.Table == "Albums" && fk.From == "AlbumId" && fk.OnDelete == "SET NULL");
    }
}
