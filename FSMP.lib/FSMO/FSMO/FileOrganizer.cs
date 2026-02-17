namespace FSMO;

public static class FileOrganizer
{
    public static OrganizeResult Organize(string sourcePath, string destinationPath, OrganizeMode mode)
    {
        ArgumentNullException.ThrowIfNull(sourcePath);
        ArgumentNullException.ThrowIfNull(destinationPath);

        if (!Directory.Exists(sourcePath))
            throw new DirectoryNotFoundException($"Source directory not found: {sourcePath}");

        var result = new OrganizeResult();
        var files = AudioFileScanner.ScanDirectory(sourcePath);

        foreach (var file in files)
        {
            try
            {
                var metadata = MetadataReader.ReadMetadata(file.FullName);
                var targetPath = PathBuilder.BuildTargetPath(destinationPath, metadata, file.Name);
                var targetDir = Path.GetDirectoryName(targetPath)!;

                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                if (mode == OrganizeMode.Copy)
                {
                    File.Copy(file.FullName, targetPath, overwrite: false);
                    result.FilesCopied++;
                }
                else
                {
                    File.Move(file.FullName, targetPath);
                    result.FilesMoved++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"{file.FullName}: {ex.Message}");
            }
        }

        if (mode == OrganizeMode.Move)
            CleanupEmptyDirectories(sourcePath);

        return result;
    }

    private static void CleanupEmptyDirectories(string rootPath)
    {
        foreach (var dir in Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories)
                     .OrderByDescending(d => d.Length))
        {
            if (Directory.Exists(dir) &&
                !Directory.EnumerateFileSystemEntries(dir).Any())
            {
                Directory.Delete(dir);
            }
        }
    }
}