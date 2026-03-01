using FSMP.Core.Models;

namespace FSMP.Core.Interfaces;

public interface IQueueStateRepository
{
    Task<QueueState?> LoadAsync();
    Task SaveAsync(QueueState state);
}
