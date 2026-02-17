namespace FSMO;

public static class FileOrganizer
{
    public static OrganizeResult Organize(string sourcePath, string destinationPath, OrganizeMode mode,
        DuplicateStrategy duplicateStrategy = DuplicateStrategy.Skip)
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

                if (File.Exists(targetPath))
                {
                    switch (duplicateStrategy)
                    {
                        case DuplicateStrategy.Skip:
                            result.FilesSkipped++;
                            continue;
                        case DuplicateStrategy.Overwrite:
                            File.Delete(targetPath);
                            break;
                        case DuplicateStrategy.Rename:
                            targetPath = GetUniqueFilePath(targetPath);
                            break;
                    }
                }

                if (mode == OrganizeMode.Copy)
                {
                    File.Copy(file.FullName, targetPath);
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

    private static string GetUniqueFilePath(string path)
    {
        var dir = Path.GetDirectoryName(path)!;
        var nameWithoutExt = Path.GetFileNameWithoutExtension(path);
        var ext = Path.GetExtension(path);
        var counter = 1;

        string candidate;
        do
        {
            candidate = Path.Combine(dir, $"{nameWithoutExt}_{counter}{ext}");
            counter++;
        } while (File.Exists(candidate));

        return candidate;
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