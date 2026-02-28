using FSMP.Core.Models;

namespace FSMP.Core.Interfaces;

public interface ILibraryScanService
{
    Task<ScanResult> ScanAllLibrariesAsync(List<string> libraryPaths);
}
