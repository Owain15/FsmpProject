namespace FsmpLibrary.Models;

/// <summary>
/// Represents a music track with file information, metadata, and playback statistics.
/// Metadata edits are non-destructive - custom fields override file metadata without modifying the original file.
/// </summary>
public class Track
{
    /// <summary>
    /// Primary key for the track entity.
    /// </summary>
    public int TrackId { get; set; }

    /// <summary>
    /// Title of the track from file metadata.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Absolute path to the audio file.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// File format/extension (WAV, WMA, MP3).
    /// </summary>
    public string FileFormat { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Track number from file metadata.
    /// </summary>
    public int? TrackNumber { get; set; }

    /// <summary>
    /// Disc number for multi-disc albums.
    /// </summary>
    public int? DiscNumber { get; set; }

    /// <summary>
    /// Duration of the track.
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// Bit rate in kbps (kilobits per second).
    /// </summary>
    public int? BitRate { get; set; }

    /// <summary>
    /// Sample rate in Hz.
    /// </summary>
    public int? SampleRate { get; set; }

    // Custom metadata fields (non-destructive overrides)

    /// <summary>
    /// Custom title override (stored in database, does not modify file).
    /// </summary>
    public string? CustomTitle { get; set; }

    /// <summary>
    /// Custom artist override (stored in database, does not modify file).
    /// </summary>
    public string? CustomArtist { get; set; }

    /// <summary>
    /// Custom album override (stored in database, does not modify file).
    /// </summary>
    public string? CustomAlbum { get; set; }

    /// <summary>
    /// Custom year override (stored in database, does not modify file).
    /// </summary>
    public int? CustomYear { get; set; }

    /// <summary>
    /// Custom genre override (stored in database, does not modify file).
    /// </summary>
    public string? CustomGenre { get; set; }

    /// <summary>
    /// User comment or notes about the track.
    /// </summary>
    public string? Comment { get; set; }

    // Relationships

    /// <summary>
    /// Foreign key to Artist entity.
    /// </summary>
    public int? ArtistId { get; set; }

    /// <summary>
    /// Foreign key to Album entity.
    /// </summary>
    public int? AlbumId { get; set; }

    // Statistics and user data

    /// <summary>
    /// Number of times the track has been played to completion.
    /// </summary>
    public int PlayCount { get; set; }

    /// <summary>
    /// Number of times the track was skipped during playback.
    /// </summary>
    public int SkipCount { get; set; }

    /// <summary>
    /// Date and time when the track was last played.
    /// </summary>
    public DateTime? LastPlayedAt { get; set; }

    /// <summary>
    /// Whether the track is marked as a favorite.
    /// </summary>
    public bool IsFavorite { get; set; }

    /// <summary>
    /// User rating (1-5 stars).
    /// </summary>
    public int? Rating { get; set; }

    // System metadata

    /// <summary>
    /// Date and time when the track was first imported into the database.
    /// </summary>
    public DateTime ImportedAt { get; set; }

    /// <summary>
    /// Date and time when the track metadata was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// SHA256 hash of the file for deduplication and integrity checking.
    /// </summary>
    public string FileHash { get; set; } = string.Empty;

    // Navigation properties

    /// <summary>
    /// Navigation property to the associated Artist.
    /// </summary>
    public Artist? Artist { get; set; }

    /// <summary>
    /// Navigation property to the associated Album.
    /// </summary>
    public Album? Album { get; set; }

    /// <summary>
    /// Navigation property to all playback history records for this track.
    /// </summary>
    public ICollection<PlaybackHistory> PlaybackHistories { get; set; } = new List<PlaybackHistory>();

    /// <summary>
    /// Gets the display title, preferring custom title over file metadata.
    /// </summary>
    public string DisplayTitle => CustomTitle ?? Title;

    /// <summary>
    /// Gets the display artist, preferring custom artist over file metadata.
    /// </summary>
    public string DisplayArtist => CustomArtist ?? Artist?.Name ?? "Unknown Artist";

    /// <summary>
    /// Gets the display album, preferring custom album over file metadata.
    /// </summary>
    public string DisplayAlbum => CustomAlbum ?? Album?.Title ?? "Unknown Album";
}
