namespace FSMO;

public static class DirectoryComparer
{
    public static List<FileInfo> FindMissingTracks(string appPath, string targetPath)
    {
        ArgumentNullException.ThrowIfNull(appPath);
        ArgumentNullException.ThrowIfNull(targetPath);

        if (string.IsNullOrWhiteSpace(appPath))
            throw new ArgumentException("App path cannot be empty.", nameof(appPath));
        if (string.IsNullOrWhiteSpace(targetPath))
            throw new ArgumentException("Target path cannot be empty.", nameof(targetPath));

        if (!Directory.Exists(appPath))
            throw new DirectoryNotFoundException($"App directory not found: {appPath}");
        if (!Directory.Exists(targetPath))
            throw new DirectoryNotFoundException($"Target directory not found: {targetPath}");

        var appFiles = AudioFileScanner.ScanDirectory(appPath);
        var targetFiles = AudioFileScanner.ScanDirectory(targetPath);

        var appFileNames = new HashSet<string>(
            appFiles.Select(f => f.Name),
            StringComparer.OrdinalIgnoreCase);

        return targetFiles.Where(f => !appFileNames.Contains(f.Name)).ToList();
    }

    public static OrganizeResult CopyMissingToApp(string appPath, string targetPath,
        DuplicateStrategy duplicateStrategy = DuplicateStrategy.Skip)
    {
        ArgumentNullException.ThrowIfNull(appPath);
        ArgumentNullException.ThrowIfNull(targetPath);

        if (string.IsNullOrWhiteSpace(appPath))
            throw new ArgumentException("App path cannot be empty.", nameof(appPath));
        if (string.IsNullOrWhiteSpace(targetPath))
            throw new ArgumentException("Target path cannot be empty.", nameof(targetPath));

        if (!Directory.Exists(appPath))
            throw new DirectoryNotFoundException($"App directory not found: {appPath}");
        if (!Directory.Exists(targetPath))
            throw new DirectoryNotFoundException($"Target directory not found: {targetPath}");

        var result = new OrganizeResult();
        var missingFiles = FindMissingTracks(appPath, targetPath);

        foreach (var file in missingFiles)
        {
            try
            {
                var metadata = MetadataReader.ReadMetadata(file.FullName);
                var copyTargetPath = PathBuilder.BuildTargetPath(appPath, metadata, file.Name);
                var targetDir = Path.GetDirectoryName(copyTargetPath)!;

                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                if (File.Exists(copyTargetPath))
                {
                    switch (duplicateStrategy)
                    {
                        case DuplicateStrategy.Skip:
                            result.FilesSkipped++;
                            continue;
                        case DuplicateStrategy.Overwrite:
                            File.Delete(copyTargetPath);
                            break;
                        case DuplicateStrategy.Rename:
                            copyTargetPath = GetUniqueFilePath(copyTargetPath);
                            break;
                    }
                }

                File.Copy(file.FullName, copyTargetPath);
                result.FilesCopied++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"{file.FullName}: {ex.Message}");
            }
        }

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
}
