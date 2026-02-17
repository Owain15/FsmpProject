using System.IO;

namespace FSMO
{
    public static class DirectoryManager
    {
        public static OrganizeResult ReorganiseDirectory(DirectoryInfo sourceDir, string destinationPath,
            OrganizeMode mode = OrganizeMode.Copy, DuplicateStrategy duplicateStrategy = DuplicateStrategy.Skip)
        {
            ArgumentNullException.ThrowIfNull(sourceDir);
            ArgumentNullException.ThrowIfNull(destinationPath);

            return FileOrganizer.Organize(sourceDir.FullName, destinationPath, mode, duplicateStrategy);
        }

        public static List<FileInfo> GetAllDistinctAudioFiles(string sourcePath)
        {
            ArgumentNullException.ThrowIfNull(sourcePath);

            var files = AudioFileScanner.ScanDirectory(sourcePath);
            return files
                .GroupBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();
        }

        public static void CreateDirectory(string path)
        {
            FileSystem.CreateDirectory(path);
        }
    }

    internal class FileSystem
    {

        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void CreateFile(string path)
        {
            if (!File.Exists(path))
            {
                using (File.Create(path)) { }
            }
        }

        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

        }

        public static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        public static void MoveFile(string sourcePath, string destinationPath)
        {
            if (File.Exists(sourcePath))
            {
                File.Move(sourcePath, destinationPath);
            }
        }

        public static void MoveDirectory(string sourcePath, string destinationPath)
        {
            if (Directory.Exists(sourcePath))
            {
                Directory.Move(sourcePath, destinationPath);
            }
        }

        
    }

}
