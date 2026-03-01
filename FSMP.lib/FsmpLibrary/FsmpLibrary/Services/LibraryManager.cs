using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;

namespace FsmpLibrary.Services;

public class LibraryManager : ILibraryManager
{
    private readonly IConfigurationService _configService;
    private readonly ILibraryScanService _scanService;

    public LibraryManager(
        IConfigurationService configService,
        ILibraryScanService scanService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _scanService = scanService ?? throw new ArgumentNullException(nameof(scanService));
    }

    public async Task<Result<Configuration>> LoadConfigurationAsync()
    {
        try
        {
            var config = await _configService.LoadConfigurationAsync();
            return Result.Success(config);
        }
        catch (Exception ex)
        {
            return Result.Failure<Configuration>($"Error loading configuration: {ex.Message}");
        }
    }

    public async Task<Result> AddLibraryPathAsync(string path)
    {
        try
        {
            if (!Directory.Exists(path))
                return Result.Failure($"Directory not found: {path}");

            await _configService.AddLibraryPathAsync(path);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error adding path: {ex.Message}");
        }
    }

    public async Task<Result> RemoveLibraryPathAsync(string path)
    {
        try
        {
            await _configService.RemoveLibraryPathAsync(path);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error removing path: {ex.Message}");
        }
    }

    public async Task<Result<ScanResult>> ScanAllLibrariesAsync()
    {
        try
        {
            var config = await _configService.LoadConfigurationAsync();
            if (config.LibraryPaths.Count == 0)
                return Result.Failure<ScanResult>("No library paths configured.");

            var result = await _scanService.ScanAllLibrariesAsync(config.LibraryPaths);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            return Result.Failure<ScanResult>($"Error scanning libraries: {ex.Message}");
        }
    }
}
