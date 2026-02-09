using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using FsmpLibrary.Services;

namespace FsmpConsole;

/// <summary>
/// Console UI for managing library paths and triggering scans.
/// </summary>
public class LibraryManager
{
    private readonly ConfigurationService _configService;
    private readonly LibraryScanService _scanService;
    private readonly UnitOfWork _unitOfWork;
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public LibraryManager(
        ConfigurationService configService,
        LibraryScanService scanService,
        UnitOfWork unitOfWork,
        TextReader input,
        TextWriter output)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _scanService = scanService ?? throw new ArgumentNullException(nameof(scanService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    /// <summary>
    /// Entry point — displays library management menu.
    /// </summary>
    public async Task RunAsync()
    {
        while (true)
        {
            await DisplayLibraryPathsAsync();

            _output.WriteLine();
            _output.WriteLine("  A) Add path");
            _output.WriteLine("  R) Remove path");
            _output.WriteLine("  S) Scan all libraries");
            _output.WriteLine("  0) Back");
            _output.Write("Select: ");

            var choice = _input.ReadLine()?.Trim()?.ToLowerInvariant();

            switch (choice)
            {
                case "a":
                    await AddLibraryPathAsync();
                    break;
                case "r":
                    await RemoveLibraryPathAsync();
                    break;
                case "s":
                    await ScanAllLibrariesAsync();
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
    /// Displays all configured library paths with track counts.
    /// </summary>
    public async Task DisplayLibraryPathsAsync()
    {
        var config = await _configService.LoadConfigurationAsync();

        _output.WriteLine();
        _output.WriteLine("== Library Paths ==");

        if (config.LibraryPaths.Count == 0)
        {
            _output.WriteLine("  (none configured)");
            return;
        }

        var totalTracks = await _unitOfWork.Tracks.CountAsync();

        for (int i = 0; i < config.LibraryPaths.Count; i++)
        {
            _output.WriteLine($"  {i + 1}) {config.LibraryPaths[i]}");
        }

        _output.WriteLine($"  Total tracks in database: {totalTracks}");
    }

    /// <summary>
    /// Prompts for a path and adds it to the configuration.
    /// </summary>
    public async Task AddLibraryPathAsync()
    {
        _output.Write("Enter path: ");
        var path = _input.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(path))
        {
            _output.WriteLine("No path entered.");
            return;
        }

        await _configService.AddLibraryPathAsync(path);
        _output.WriteLine("Path added.");
    }

    /// <summary>
    /// Prompts for a path number and removes it from the configuration.
    /// </summary>
    public async Task RemoveLibraryPathAsync()
    {
        var config = await _configService.LoadConfigurationAsync();

        if (config.LibraryPaths.Count == 0)
        {
            _output.WriteLine("No paths to remove.");
            return;
        }

        _output.Write("Enter path number to remove: ");
        var input = _input.ReadLine()?.Trim();

        if (int.TryParse(input, out var index) && index >= 1 && index <= config.LibraryPaths.Count)
        {
            var removed = config.LibraryPaths[index - 1];
            await _configService.RemoveLibraryPathAsync(removed);
            _output.WriteLine($"Removed: {removed}");
        }
        else
        {
            _output.WriteLine("Invalid selection.");
        }
    }

    /// <summary>
    /// Scans a single library path.
    /// </summary>
    public async Task ScanLibraryAsync(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        _output.WriteLine($"Scanning: {path}");
        var result = await _scanService.ScanLibraryAsync(path);

        _output.WriteLine($"  Added: {result.TracksAdded}, Updated: {result.TracksUpdated}, Removed: {result.TracksRemoved}");
        if (result.Errors.Count > 0)
            _output.WriteLine($"  {result.Errors.Count} error(s) occurred.");
    }

    /// <summary>
    /// Scans all configured library paths.
    /// </summary>
    public async Task ScanAllLibrariesAsync()
    {
        var config = await _configService.LoadConfigurationAsync();

        if (config.LibraryPaths.Count == 0)
        {
            _output.WriteLine("No library paths configured.");
            return;
        }

        _output.WriteLine("Scanning all libraries...");
        var result = await _scanService.ScanAllLibrariesAsync(config.LibraryPaths);

        _output.WriteLine($"Scan complete: {result.TracksAdded} added, {result.TracksUpdated} updated, {result.TracksRemoved} removed");
        if (result.Errors.Count > 0)
            _output.WriteLine($"  {result.Errors.Count} error(s) occurred.");
        _output.WriteLine($"  Duration: {result.Duration.TotalSeconds:F1}s");
    }
}
