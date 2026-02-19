using FluentAssertions;
using FsmpConsole;
using FsmpDataAcsses;
using FSMP.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.UI;

public class MetadataEditorTests : IDisposable
{
    private readonly FsmpDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public MetadataEditorTests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();
        _unitOfWork = new UnitOfWork(_context);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
    }

    // --- Helpers ---

    private (MetadataEditor editor, StringWriter output) CreateEditorWithOutput(string inputLines)
    {
        var input = new StringReader(inputLines);
        var output = new StringWriter();
        var editor = new MetadataEditor(_unitOfWork, input, output);
        return (editor, output);
    }

    private async Task<Track> CreateTrackAsync(string title, string? artistName = null, string? albumTitle = null)
    {
        Artist? artist = null;
        if (artistName != null)
        {
            artist = new Artist { Name = artistName, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            await _unitOfWork.Artists.AddAsync(artist);
            await _unitOfWork.SaveAsync();
        }

        Album? album = null;
        if (albumTitle != null)
        {
            album = new Album
            {
                Title = albumTitle,
                ArtistId = artist?.ArtistId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            await _unitOfWork.Albums.AddAsync(album);
            await _unitOfWork.SaveAsync();
        }

        var track = new Track
        {
            Title = title,
            FilePath = $@"C:\Music\{title}.mp3",
            FileHash = Guid.NewGuid().ToString(),
            ArtistId = artist?.ArtistId,
            AlbumId = album?.AlbumId,
            Duration = TimeSpan.FromSeconds(200),
            ImportedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _unitOfWork.Tracks.AddAsync(track);
        await _unitOfWork.SaveAsync();
        return track;
    }

    // ========== Constructor Tests ==========

    [Fact]
    public void Constructor_WithNullUnitOfWork_ShouldThrow()
    {
        var act = () => new MetadataEditor(null!, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("unitOfWork");
    }

    [Fact]
    public void Constructor_WithNullInput_ShouldThrow()
    {
        var act = () => new MetadataEditor(_unitOfWork, null!, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("input");
    }

    [Fact]
    public void Constructor_WithNullOutput_ShouldThrow()
    {
        var act = () => new MetadataEditor(_unitOfWork, TextReader.Null, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("output");
    }

    // ========== SearchTrackAsync Tests ==========

    [Fact]
    public async Task SearchTrackAsync_EmptySearch_ShouldReturnNull()
    {
        var (editor, output) = CreateEditorWithOutput("\n");

        var result = await editor.SearchTrackAsync();

        result.Should().BeNull();
        output.ToString().Should().Contain("No search term entered");
    }

    [Fact]
    public async Task SearchTrackAsync_NoResults_ShouldReturnNull()
    {
        var (editor, output) = CreateEditorWithOutput("nonexistent\n");

        var result = await editor.SearchTrackAsync();

        result.Should().BeNull();
        output.ToString().Should().Contain("No tracks found");
    }

    [Fact]
    public async Task SearchTrackAsync_FindsByTitle_ShouldReturnTrack()
    {
        var track = await CreateTrackAsync("Kerala", "Bonobo", "Migration");

        // Search "Kerala", then select track 1
        var (editor, output) = CreateEditorWithOutput("Kerala\n1\n");

        var result = await editor.SearchTrackAsync();

        result.Should().NotBeNull();
        result!.TrackId.Should().Be(track.TrackId);
        output.ToString().Should().Contain("Search Results");
    }

    [Fact]
    public async Task SearchTrackAsync_FindsByArtist_ShouldReturnResults()
    {
        await CreateTrackAsync("Kerala", "Bonobo", "Migration");

        // Search "Bonobo", select track 1
        var (editor, output) = CreateEditorWithOutput("Bonobo\n1\n");

        var result = await editor.SearchTrackAsync();

        result.Should().NotBeNull();
        output.ToString().Should().Contain("Kerala");
    }

    [Fact]
    public async Task SearchTrackAsync_CancelSelection_ShouldReturnNull()
    {
        await CreateTrackAsync("Kerala", "Bonobo");

        // Search "Kerala", then cancel with 0
        var (editor, output) = CreateEditorWithOutput("Kerala\n0\n");

        var result = await editor.SearchTrackAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchTrackAsync_InvalidSelection_ShouldReturnNull()
    {
        await CreateTrackAsync("Kerala", "Bonobo");

        // Search "Kerala", then invalid selection
        var (editor, output) = CreateEditorWithOutput("Kerala\n999\n");

        var result = await editor.SearchTrackAsync();

        result.Should().BeNull();
        output.ToString().Should().Contain("Invalid selection");
    }

    [Fact]
    public async Task SearchTrackAsync_FindsByCustomTitle_ShouldReturnResults()
    {
        var track = await CreateTrackAsync("Track01", "Bonobo");
        track.CustomTitle = "Kerala Remix";
        _unitOfWork.Tracks.Update(track);
        await _unitOfWork.SaveAsync();

        var (editor, output) = CreateEditorWithOutput("Kerala\n1\n");

        var result = await editor.SearchTrackAsync();

        result.Should().NotBeNull();
    }

    // ========== DisplayMetadataAsync Tests ==========

    [Fact]
    public async Task DisplayMetadataAsync_ShouldShowFileMetadata()
    {
        var track = await CreateTrackAsync("Kerala", "Bonobo", "Migration");

        var (editor, output) = CreateEditorWithOutput("");

        await editor.DisplayMetadataAsync(track);

        var text = output.ToString();
        text.Should().Contain("Track Metadata");
        text.Should().Contain("Kerala");
        text.Should().Contain("Bonobo");
        text.Should().Contain("Migration");
    }

    [Fact]
    public async Task DisplayMetadataAsync_ShouldShowCustomOverrides()
    {
        var track = await CreateTrackAsync("Kerala", "Bonobo");
        track.CustomTitle = "Kerala (Live)";
        track.CustomArtist = "Bonobo Live";
        track.Rating = 5;
        track.IsFavorite = true;
        track.Comment = "Great track";
        _unitOfWork.Tracks.Update(track);
        await _unitOfWork.SaveAsync();

        var (editor, output) = CreateEditorWithOutput("");

        await editor.DisplayMetadataAsync(track);

        var text = output.ToString();
        text.Should().Contain("Custom Overrides");
        text.Should().Contain("Kerala (Live)");
        text.Should().Contain("Bonobo Live");
        text.Should().Contain("5/5");
        text.Should().Contain("Yes");
        text.Should().Contain("Great track");
    }

    [Fact]
    public async Task DisplayMetadataAsync_NoCustomOverrides_ShouldShowNone()
    {
        var track = await CreateTrackAsync("Kerala");

        var (editor, output) = CreateEditorWithOutput("");

        await editor.DisplayMetadataAsync(track);

        var text = output.ToString();
        text.Should().Contain("(none)");
        text.Should().Contain("Favorite: No");
    }

    [Fact]
    public async Task DisplayMetadataAsync_WithDuration_ShouldShowDuration()
    {
        var track = await CreateTrackAsync("Kerala");

        var (editor, output) = CreateEditorWithOutput("");

        await editor.DisplayMetadataAsync(track);

        output.ToString().Should().Contain("3:20");
    }

    // ========== EditMetadataAsync Tests ==========

    [Fact]
    public async Task EditMetadataAsync_SetCustomTitle_ShouldUpdate()
    {
        var track = await CreateTrackAsync("Kerala", "Bonobo");

        // Title: "Kerala Live", rest: keep defaults (empty)
        var (editor, output) = CreateEditorWithOutput("Kerala Live\n\n\n\n\n\n");

        await editor.EditMetadataAsync(track);

        var updated = await _unitOfWork.Tracks.GetByIdAsync(track.TrackId);
        updated!.CustomTitle.Should().Be("Kerala Live");
        output.ToString().Should().Contain("Metadata saved");
    }

    [Fact]
    public async Task EditMetadataAsync_SetCustomArtist_ShouldUpdate()
    {
        var track = await CreateTrackAsync("Kerala", "Bonobo");

        // Title: keep, Artist: "Bonobo Live", rest: keep
        var (editor, output) = CreateEditorWithOutput("\nBonobo Live\n\n\n\n\n");

        await editor.EditMetadataAsync(track);

        var updated = await _unitOfWork.Tracks.GetByIdAsync(track.TrackId);
        updated!.CustomArtist.Should().Be("Bonobo Live");
    }

    [Fact]
    public async Task EditMetadataAsync_SetCustomAlbum_ShouldUpdate()
    {
        var track = await CreateTrackAsync("Kerala", "Bonobo");

        // Title: keep, Artist: keep, Album: "Live Album", rest: keep
        var (editor, output) = CreateEditorWithOutput("\n\nLive Album\n\n\n\n");

        await editor.EditMetadataAsync(track);

        var updated = await _unitOfWork.Tracks.GetByIdAsync(track.TrackId);
        updated!.CustomAlbum.Should().Be("Live Album");
    }

    [Fact]
    public async Task EditMetadataAsync_SetValidRating_ShouldUpdate()
    {
        var track = await CreateTrackAsync("Kerala");

        // Title, Artist, Album: keep, Rating: 4, rest: keep
        var (editor, output) = CreateEditorWithOutput("\n\n\n4\n\n\n");

        await editor.EditMetadataAsync(track);

        var updated = await _unitOfWork.Tracks.GetByIdAsync(track.TrackId);
        updated!.Rating.Should().Be(4);
    }

    [Fact]
    public async Task EditMetadataAsync_InvalidRating_ShouldShowError()
    {
        var track = await CreateTrackAsync("Kerala");

        // Rating: 7 (invalid), rest: keep
        var (editor, output) = CreateEditorWithOutput("\n\n\n7\n\n\n");

        await editor.EditMetadataAsync(track);

        output.ToString().Should().Contain("Invalid rating");
        var updated = await _unitOfWork.Tracks.GetByIdAsync(track.TrackId);
        updated!.Rating.Should().BeNull();
    }

    [Fact]
    public async Task EditMetadataAsync_SetFavoriteYes_ShouldUpdate()
    {
        var track = await CreateTrackAsync("Kerala");

        // All keep except Favorite: y
        var (editor, output) = CreateEditorWithOutput("\n\n\n\ny\n\n");

        await editor.EditMetadataAsync(track);

        var updated = await _unitOfWork.Tracks.GetByIdAsync(track.TrackId);
        updated!.IsFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task EditMetadataAsync_SetFavoriteNo_ShouldUpdate()
    {
        var track = await CreateTrackAsync("Kerala");
        track.IsFavorite = true;
        _unitOfWork.Tracks.Update(track);
        await _unitOfWork.SaveAsync();

        // All keep except Favorite: n
        var (editor, output) = CreateEditorWithOutput("\n\n\n\nn\n\n");

        await editor.EditMetadataAsync(track);

        var updated = await _unitOfWork.Tracks.GetByIdAsync(track.TrackId);
        updated!.IsFavorite.Should().BeFalse();
    }

    [Fact]
    public async Task EditMetadataAsync_SetComment_ShouldUpdate()
    {
        var track = await CreateTrackAsync("Kerala");

        // All keep except Comment: "Love this"
        var (editor, output) = CreateEditorWithOutput("\n\n\n\n\nLove this\n");

        await editor.EditMetadataAsync(track);

        var updated = await _unitOfWork.Tracks.GetByIdAsync(track.TrackId);
        updated!.Comment.Should().Be("Love this");
    }

    [Fact]
    public async Task EditMetadataAsync_ClearCustomTitle_ShouldSetNull()
    {
        var track = await CreateTrackAsync("Kerala");
        track.CustomTitle = "Old Title";
        _unitOfWork.Tracks.Update(track);
        await _unitOfWork.SaveAsync();

        // Title: "-" to clear, rest: keep
        var (editor, output) = CreateEditorWithOutput("-\n\n\n\n\n\n");

        await editor.EditMetadataAsync(track);

        var updated = await _unitOfWork.Tracks.GetByIdAsync(track.TrackId);
        updated!.CustomTitle.Should().BeNull();
    }

    [Fact]
    public async Task EditMetadataAsync_ClearRating_ShouldSetNull()
    {
        var track = await CreateTrackAsync("Kerala");
        track.Rating = 3;
        _unitOfWork.Tracks.Update(track);
        await _unitOfWork.SaveAsync();

        // Rating: "-" to clear
        var (editor, output) = CreateEditorWithOutput("\n\n\n-\n\n\n");

        await editor.EditMetadataAsync(track);

        var updated = await _unitOfWork.Tracks.GetByIdAsync(track.TrackId);
        updated!.Rating.Should().BeNull();
    }

    [Fact]
    public async Task EditMetadataAsync_KeepAllDefaults_ShouldNotChange()
    {
        var track = await CreateTrackAsync("Kerala");

        // All empty (keep defaults)
        var (editor, output) = CreateEditorWithOutput("\n\n\n\n\n\n");

        await editor.EditMetadataAsync(track);

        var updated = await _unitOfWork.Tracks.GetByIdAsync(track.TrackId);
        updated!.CustomTitle.Should().BeNull();
        updated.CustomArtist.Should().BeNull();
        updated.CustomAlbum.Should().BeNull();
        updated.Rating.Should().BeNull();
        updated.IsFavorite.Should().BeFalse();
        updated.Comment.Should().BeNull();
        output.ToString().Should().Contain("Metadata saved");
    }

    // ========== SaveChangesAsync Tests ==========

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        var track = await CreateTrackAsync("Kerala");
        track.CustomTitle = "Kerala Live";
        var beforeUpdate = track.UpdatedAt;

        var (editor, output) = CreateEditorWithOutput("");

        await editor.SaveChangesAsync(track);

        var reloaded = await _unitOfWork.Tracks.GetByIdAsync(track.TrackId);
        reloaded!.CustomTitle.Should().Be("Kerala Live");
        reloaded.UpdatedAt.Should().BeAfter(beforeUpdate);
        output.ToString().Should().Contain("Metadata saved");
    }

    // ========== RunAsync Integration Test ==========

    [Fact]
    public async Task RunAsync_EmptySearch_ShouldExitGracefully()
    {
        var (editor, output) = CreateEditorWithOutput("\n");

        await editor.RunAsync();

        output.ToString().Should().Contain("No search term entered");
    }

    [Fact]
    public async Task RunAsync_FullWorkflow_ShouldSearchDisplayAndEdit()
    {
        await CreateTrackAsync("Kerala", "Bonobo", "Migration");

        // Search "Kerala" → select 1 → edit: set title "Kerala Live", keep rest
        var (editor, output) = CreateEditorWithOutput("Kerala\n1\nKerala Live\n\n\n\n\n\n");

        await editor.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Search Results");
        text.Should().Contain("Track Metadata");
        text.Should().Contain("Metadata saved");
    }
}
