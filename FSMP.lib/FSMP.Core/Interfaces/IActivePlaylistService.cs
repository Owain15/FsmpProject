using FSMP.Core.Models;

namespace FSMP.Core.Interfaces;

public interface IActivePlaylistService
{
    RepeatMode RepeatMode { get; set; }
    bool IsShuffled { get; }
    int Count { get; }
    int CurrentIndex { get; }
    int? CurrentTrackId { get; }
    IReadOnlyList<int> PlayOrder { get; }
    bool HasNext { get; }
    bool HasPrevious { get; }

    void SetQueue(IReadOnlyList<int> trackIds);
    void Clear();
    int? MoveNext();
    int? MovePrevious();
    void JumpTo(int index);
    void ToggleShuffle();
    QueueState GetState();
    void RestoreState(QueueState state);
}
