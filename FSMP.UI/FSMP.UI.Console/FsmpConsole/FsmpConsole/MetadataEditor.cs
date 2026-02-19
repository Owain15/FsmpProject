using FsmpDataAcsses;
using FSMP.Core.Models;

namespace FsmpConsole;

/// <summary>
/// Console UI for searching and editing track metadata.
/// Custom metadata overrides are stored in the database without modifying original files.
/// </summary>
public class MetadataEditor
{
    private readonly UnitOfWork _unitOfWork;
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public MetadataEditor(UnitOfWork unitOfWork, TextReader input, TextWriter output)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    /// <summary>
    /// Entry point — search for a track then edit its metadata.
    /// </summary>
    public async Task RunAsync()
    {
        var track = await SearchTrackAsync();
        if (track == null)
            return;

        await DisplayMetadataAsync(track);
        await EditMetadataAsync(track);
    }

    /// <summary>
    /// Prompts the user for a search term and returns matching tracks.
    /// Searches by title and artist name (case-insensitive).
    /// </summary>
    public async Task<Track?> SearchTrackAsync()
    {
        _output.Write("Search (title or artist): ");
        var searchTerm = _input.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(searchTerm))
        {
            _output.WriteLine("No search term entered.");
            return null;
        }

        var results = (await _unitOfWork.Tracks.FindAsync(t =>
            t.Title.Contains(searchTerm) ||
            (t.CustomTitle != null && t.CustomTitle.Contains(searchTerm)) ||
            (t.Artist != null && t.Artist.Name.Contains(searchTerm)) ||
            (t.CustomArtist != null && t.CustomArtist.Contains(searchTerm))
        )).ToList();

        if (results.Count == 0)
        {
            _output.WriteLine("No tracks found.");
            return null;
        }

        Print.WriteSelectionMenu(_output, $"Search Results ({results.Count} found)",
            results.Select(t => $"{t.DisplayTitle} - {t.DisplayArtist}").ToList(),
            "Select track", "Cancel");

        var input = _input.ReadLine()?.Trim();
        if (input == "0" || string.IsNullOrEmpty(input))
            return null;

        if (int.TryParse(input, out var index) && index >= 1 && index <= results.Count)
            return results[index - 1];

        _output.WriteLine("Invalid selection.");
        return null;
    }

    /// <summary>
    /// Displays both file metadata and custom overrides for a track.
    /// </summary>
    public async Task DisplayMetadataAsync(Track track)
    {
        ArgumentNullException.ThrowIfNull(track);

        // Reload to ensure navigation properties are loaded
        var loaded = await _unitOfWork.Tracks.GetByIdAsync(track.TrackId);
        if (loaded != null)
            track = loaded;

        var fields = new List<(string Label, string Value)>
        {
            ("Title:", track.Title),
            ("Artist:", track.Artist?.Name ?? "Unknown"),
            ("Album:", track.Album?.Title ?? "Unknown"),
            ("File:", track.FilePath)
        };
        if (track.Duration.HasValue)
            fields.Add(("Duration:", $"{track.Duration.Value.Minutes}:{track.Duration.Value.Seconds:D2}"));

        Print.WriteDetailCard(_output, "Track Metadata", fields);

        Print.WriteDetailCard(_output, "Custom Overrides", new List<(string Label, string Value)>
        {
            ("Title:", track.CustomTitle ?? "(none)"),
            ("Artist:", track.CustomArtist ?? "(none)"),
            ("Album:", track.CustomAlbum ?? "(none)"),
            ("Rating:", track.Rating.HasValue ? $"{track.Rating}/5" : "(none)"),
            ("Favorite:", track.IsFavorite ? "Yes" : "No"),
            ("Comment:", track.Comment ?? "(none)")
        });
    }

    /// <summary>
    /// Interactive editor for track metadata fields.
    /// Empty input keeps existing value. Enter "-" to clear a field.
    /// </summary>
    public async Task EditMetadataAsync(Track track)
    {
        ArgumentNullException.ThrowIfNull(track);

        _output.WriteLine();
        _output.WriteLine("== Edit Metadata (Enter to keep, '-' to clear) ==");
        _output.WriteLine();

        // CustomTitle
        _output.Write($"  Title [{track.CustomTitle ?? track.Title}]: ");
        var titleInput = _input.ReadLine()?.Trim();
        if (titleInput == "-")
            track.CustomTitle = null;
        else if (!string.IsNullOrEmpty(titleInput))
            track.CustomTitle = titleInput;

        // CustomArtist
        _output.Write($"  Artist [{track.CustomArtist ?? track.Artist?.Name ?? "Unknown"}]: ");
        var artistInput = _input.ReadLine()?.Trim();
        if (artistInput == "-")
            track.CustomArtist = null;
        else if (!string.IsNullOrEmpty(artistInput))
            track.CustomArtist = artistInput;

        // CustomAlbum
        _output.Write($"  Album [{track.CustomAlbum ?? track.Album?.Title ?? "Unknown"}]: ");
        var albumInput = _input.ReadLine()?.Trim();
        if (albumInput == "-")
            track.CustomAlbum = null;
        else if (!string.IsNullOrEmpty(albumInput))
            track.CustomAlbum = albumInput;

        // Rating
        _output.Write($"  Rating (1-5) [{(track.Rating.HasValue ? track.Rating.ToString() : "none")}]: ");
        var ratingInput = _input.ReadLine()?.Trim();
        if (ratingInput == "-")
            track.Rating = null;
        else if (!string.IsNullOrEmpty(ratingInput))
        {
            if (int.TryParse(ratingInput, out var rating) && rating >= 1 && rating <= 5)
                track.Rating = rating;
            else
                _output.WriteLine("    Invalid rating. Must be 1-5.");
        }

        // IsFavorite
        _output.Write($"  Favorite (y/n) [{(track.IsFavorite ? "y" : "n")}]: ");
        var favInput = _input.ReadLine()?.Trim()?.ToLowerInvariant();
        if (favInput == "y")
            track.IsFavorite = true;
        else if (favInput == "n")
            track.IsFavorite = false;

        // Comment
        _output.Write($"  Comment [{track.Comment ?? "none"}]: ");
        var commentInput = _input.ReadLine()?.Trim();
        if (commentInput == "-")
            track.Comment = null;
        else if (!string.IsNullOrEmpty(commentInput))
            track.Comment = commentInput;

        await SaveChangesAsync(track);
    }

    /// <summary>
    /// Persists metadata changes to the database.
    /// </summary>
    public async Task SaveChangesAsync(Track track)
    {
        ArgumentNullException.ThrowIfNull(track);

        track.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Tracks.Update(track);
        await _unitOfWork.SaveAsync();
        _output.WriteLine("Metadata saved.");
    }
}
