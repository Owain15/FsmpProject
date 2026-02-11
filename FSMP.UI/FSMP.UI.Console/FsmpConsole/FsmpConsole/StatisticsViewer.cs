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
            Print.WriteSelectionMenu(_output, "Statistics",
                new[] { "Overview", "Most Played", "Recently Played", "Favorites", "Genre Breakdown" },
                "Select");

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

        Print.WriteDetailCard(_output, "Library Overview", new List<(string Label, string Value)>
        {
            ("Total tracks:", totalTracks.ToString()),
            ("Total plays:", totalPlays.ToString()),
            ("Listening time:", FormatTimeSpan(totalTime))
        });
    }

    /// <summary>
    /// Displays the top 10 most played tracks.
    /// </summary>
    public async Task DisplayMostPlayedAsync()
    {
        var tracks = (await _statsService.GetMostPlayedTracksAsync(10)).ToList();

        Print.WriteDataList(_output, "Most Played",
            tracks.Select(t => $"{FormatTrackLine(t)} | {t.PlayCount} plays").ToList(),
            "No playback history yet.");
    }

    /// <summary>
    /// Displays the top 10 recently played tracks.
    /// </summary>
    public async Task DisplayRecentlyPlayedAsync()
    {
        var tracks = (await _statsService.GetRecentlyPlayedTracksAsync(10)).ToList();

        Print.WriteDataList(_output, "Recently Played",
            tracks.Select(t =>
            {
                var lastPlayed = t.LastPlayedAt?.ToString("yyyy-MM-dd HH:mm") ?? "Never";
                return $"{FormatTrackLine(t)} | {lastPlayed}";
            }).ToList(),
            "No playback history yet.");
    }

    /// <summary>
    /// Displays all favorite tracks.
    /// </summary>
    public async Task DisplayFavoritesAsync()
    {
        var tracks = (await _statsService.GetFavoritesAsync()).ToList();

        Print.WriteDataList(_output, "Favorites",
            tracks.Select(t =>
            {
                var rating = t.Rating.HasValue ? new string('*', t.Rating.Value) : "";
                return $"{FormatTrackLine(t)}{(rating.Length > 0 ? $" | {rating}" : "")}";
            }).ToList(),
            "No favorites yet. Mark tracks as favorites from the metadata editor.");
    }

    /// <summary>
    /// Displays genre breakdown with track counts.
    /// </summary>
    public async Task DisplayGenreBreakdownAsync()
    {
        var genres = await _statsService.GetGenreStatisticsAsync();

        Print.WriteDataList(_output, "Genre Breakdown",
            genres.OrderByDescending(g => g.Value)
                .Select(kvp => $"{kvp.Key,-15} {kvp.Value} tracks")
                .ToList(),
            "No genre data available.");
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
