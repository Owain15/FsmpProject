using System.Text.Json;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;

namespace FsmpDataAcsses.Repositories;

public class JsonQueueStateRepository : IQueueStateRepository
{
    private readonly string _filePath;

    public JsonQueueStateRepository(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    public async Task<QueueState?> LoadAsync()
    {
        try
        {
            if (!File.Exists(_filePath))
                return null;

            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<QueueState>(json);
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveAsync(QueueState state)
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var tempPath = _filePath + ".tmp";
        var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(tempPath, json);
        File.Move(tempPath, _filePath, overwrite: true);
    }
}
