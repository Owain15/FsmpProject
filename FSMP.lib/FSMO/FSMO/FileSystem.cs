using System.IO;

namespace FSMO
{
    public static class DirectoryManager
	{
        public static void ReoginiseDirectory(System.IO.DirectoryInfo dir)
        {
            throw new NotImplementedException();
		}

        public static List<System.IO.FileInfo>GetAllDistinctAudioFileNewDirectory()
        { 
            throw new NotImplementedException();
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
