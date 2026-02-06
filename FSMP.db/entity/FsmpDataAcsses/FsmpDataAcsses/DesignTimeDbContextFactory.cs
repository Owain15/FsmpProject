using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FsmpDataAcsses;

/// <summary>
/// Factory for creating FsmpDbContext at design time (used by EF Core migrations tooling).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FsmpDbContext>
{
    public FsmpDbContext CreateDbContext(string[] args)
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var fsmpDir = Path.Combine(appDataPath, "FSMP");
        Directory.CreateDirectory(fsmpDir);
        var dbPath = Path.Combine(fsmpDir, "fsmp.db");

        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        return new FsmpDbContext(options);
    }
}
