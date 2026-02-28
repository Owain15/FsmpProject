using FSMP.Core.Models;

namespace FSMP.Core.Interfaces;

public interface ILibraryManager
{
    Task<Result<Configuration>> LoadConfigurationAsync();
    Task<Result> AddLibraryPathAsync(string path);
    Task<Result> RemoveLibraryPathAsync(string path);
    Task<Result<ScanResult>> ScanAllLibrariesAsync();
}
