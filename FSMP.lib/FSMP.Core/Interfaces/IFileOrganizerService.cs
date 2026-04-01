using FSMO;

namespace FSMP.Core.Interfaces;

public interface IFileOrganizerService
{
    OrganizePreview Preview(string sourcePath, string destinationPath, OrganizeMode mode,
        DuplicateStrategy duplicateStrategy = DuplicateStrategy.Skip);

    OrganizeResult Organize(string sourcePath, string destinationPath, OrganizeMode mode,
        DuplicateStrategy duplicateStrategy = DuplicateStrategy.Skip);

    List<FileInfo> FindMissingTracks(string appPath, string targetPath);

    OrganizeResult CopyMissingToApp(string appPath, string targetPath,
        DuplicateStrategy duplicateStrategy = DuplicateStrategy.Skip);
}
