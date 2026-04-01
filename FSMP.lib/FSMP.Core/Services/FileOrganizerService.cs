using FSMP.Core.Interfaces;
using FSMO;

namespace FSMP.Core.Services;

public class FileOrganizerService : IFileOrganizerService
{
    public OrganizePreview Preview(string sourcePath, string destinationPath, OrganizeMode mode,
        DuplicateStrategy duplicateStrategy = DuplicateStrategy.Skip)
    {
        return FileOrganizer.Preview(sourcePath, destinationPath, mode, duplicateStrategy);
    }

    public OrganizeResult Organize(string sourcePath, string destinationPath, OrganizeMode mode,
        DuplicateStrategy duplicateStrategy = DuplicateStrategy.Skip)
    {
        return FileOrganizer.Organize(sourcePath, destinationPath, mode, duplicateStrategy);
    }

    public List<FileInfo> FindMissingTracks(string appPath, string targetPath)
    {
        return DirectoryComparer.FindMissingTracks(appPath, targetPath);
    }

    public OrganizeResult CopyMissingToApp(string appPath, string targetPath,
        DuplicateStrategy duplicateStrategy = DuplicateStrategy.Skip)
    {
        return DirectoryComparer.CopyMissingToApp(appPath, targetPath, duplicateStrategy);
    }
}
