using FsmpDataAcsses.Services;
using FsmpLibrary.Models;

namespace FsmpConsole;

/// <summary>
/// Console UI for viewing library and playback statistics.
/// </summary>
public class StatisticsViewer
{
    private readonly StatisticsService _statsService;
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public StatisticsViewer(StatisticsService statsService, TextReader input, TextWriter output)
    {
        _statsService = statsService ?? throw new ArgumentNullException(nameof(statsService));
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    /// <summary>
    /// Entry point — displays statistics menu loop.
    /// </summary>
    public async Task RunAsync()
    {
        while (true)
        {
            _output.WriteLine();
            _output.WriteLine("== Statistics ==");
            _output.WriteLine();
            _output.WriteLine("  1) Overview");
            _output.WriteLine("  2) Most Played");
            _output.WriteLine("  3) Recently Played");
            _output.WriteLine("  4) Favorites");
            _output.WriteLine("  5) Genre Breakdown");
            _output.WriteLine("  0) Back");
            _output.Write("Select: ");

            var choice = _input.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    await DisplayTotalStatisticsAsync();
                    break;
                case "2":
                    await DisplayMostPlayedAsync();
                    break;
                case "3":
                    await DisplayRecentlyPlayedAsync();
                    break;
                case "4":
                    await DisplayFavoritesAsync();
                    break;
                case "5":
                    await DisplayGenreBreakdownAsync();
                    break;
                case "0":
                case null:
                case "":
                    return;
                default:
                    _output.WriteLine("Invalid option.");
                    break;
            }
        }
    }

    /// <summary>
    /// Displays total library statistics (track count, play count, listening time).
    /// </summary>
    public async Task DisplayTotalStatisticsAsync()
    {
        var totalTracks = await _statsService.GetTotalTrackCountAsync();
        var totalPlays = await _statsService.GetTotalPlayCountAsync();
        var totalTime = await _statsService.GetTotalListeningTimeAsync();

        _output.WriteLine();
        _output.WriteLine("== Library Overview ==");
        _output.WriteLine($"  Total tracks:     {totalTracks}");
        _output.WriteLine($"  Total plays:      {totalPlays}");
        _output.WriteLine($"  Listening time:   {FormatTimeSpan(totalTime)}");
    }

    /// <summary>
    /// Displays the top 10 most played tracks.
    /// </summary>
    public async Task DisplayMostPlayedAsync()
    {
        var tracks = (await _statsService.GetMostPlayedTracksAsync(10)).ToList();

        _output.WriteLine();
        _output.WriteLine("== Most Played ==");

        if (tracks.Count == 0)
        {
            _output.WriteLine("  No playback history yet.");
            return;
        }

        for (int i = 0; i < tracks.Count; i++)
        {
            var t = tracks[i];
            _output.WriteLine($"  {i + 1,2}) {FormatTrackLine(t)} | {t.PlayCount} plays");
        }
    }

    /// <summary>
    /// Displays the top 10 recently played tracks.
    /// </summary>
    public async Task DisplayRecentlyPlayedAsync()
    {
        var tracks = (await _statsService.GetRecentlyPlayedTracksAsync(10)).ToList();

        _output.WriteLine();
        _output.WriteLine("== Recently Played ==");

        if (tracks.Count == 0)
        {
            _output.WriteLine("  No playback history yet.");
            return;
        }

        for (int i = 0; i < tracks.Count; i++)
        {
            var t = tracks[i];
            var lastPlayed = t.LastPlayedAt?.ToString("yyyy-MM-dd HH:mm") ?? "Never";
            _output.WriteLine($"  {i + 1,2}) {FormatTrackLine(t)} | {lastPlayed}");
        }
    }

    /// <summary>
    /// Displays all favorite tracks.
    /// </summary>
    public async Task DisplayFavoritesAsync()
    {
        var tracks = (await _statsService.GetFavoritesAsync()).ToList();

        _output.WriteLine();
        _output.WriteLine("== Favorites ==");

        if (tracks.Count == 0)
        {
            _output.WriteLine("  No favorites yet. Mark tracks as favorites from the metadata editor.");
            return;
        }

        for (int i = 0; i < tracks.Count; i++)
        {
            var t = tracks[i];
            var rating = t.Rating.HasValue ? new string('*', t.Rating.Value) : "";
            _output.WriteLine($"  {i + 1,2}) {FormatTrackLine(t)}{(rating.Length > 0 ? $" | {rating}" : "")}");
        }
    }

    /// <summary>
    /// Displays genre breakdown with track counts.
    /// </summary>
    public async Task DisplayGenreBreakdownAsync()
    {
        var genres = await _statsService.GetGenreStatisticsAsync();

        _output.WriteLine();
        _output.WriteLine("== Genre Breakdown ==");

        if (genres.Count == 0)
        {
            _output.WriteLine("  No genre data available.");
            return;
        }

        foreach (var kvp in genres.OrderByDescending(g => g.Value))
        {
            _output.WriteLine($"  {kvp.Key,-15} {kvp.Value} tracks");
        }
    }

    private static string FormatTrackLine(Track track)
    {
        var artist = track.DisplayArtist;
        var title = track.DisplayTitle;
        return $"{artist} - {title}";
    }

    private static string FormatTimeSpan(TimeSpan time)
    {
        if (time.TotalHours >= 1)
            return $"{(int)time.TotalHours}h {time.Minutes}m";
        if (time.TotalMinutes >= 1)
            return $"{time.Minutes}m {time.Seconds}s";
        return $"{time.Seconds}s";
    }
}
