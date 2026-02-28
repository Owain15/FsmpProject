using FSMP.Core.Models;

namespace FSMP.Core.Interfaces;

public interface IConfigurationService
{
    Task<Configuration> LoadConfigurationAsync();
    Task SaveConfigurationAsync(Configuration config);
    Task AddLibraryPathAsync(string path);
    Task RemoveLibraryPathAsync(string path);
}
