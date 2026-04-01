namespace FSMP.Core.Interfaces;

public interface ILibraryStatsService
{
    Task<DirectoryStats> GetDirectoryStatsAsync(string directoryPath);
    Task<DirectoryStats> GetTotalStatsAsync();
}

public record DirectoryStats(int TrackCount, int AlbumCount, int ArtistCount);
