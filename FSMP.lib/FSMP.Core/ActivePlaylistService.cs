using FSMP.Core.Interfaces;
using FSMP.Core.Models;

namespace FSMP.Core;

public class ActivePlaylistService : IActivePlaylistService
{
    private List<int> _originalOrder = new();
    private List<int> _playOrder = new();
    private int _currentIndex = -1;
    private bool _isShuffled;
    private readonly Random _random;

    public ActivePlaylistService()
        : this(new Random())
    {
    }

    public ActivePlaylistService(Random random)
    {
        _random = random;
    }

    public RepeatMode RepeatMode { get; set; } = RepeatMode.None;

    public bool IsShuffled => _isShuffled;

    public int Count => _playOrder.Count;

    public int CurrentIndex => _currentIndex;

    public int? CurrentTrackId =>
        _currentIndex >= 0 && _currentIndex < _playOrder.Count
            ? _playOrder[_currentIndex]
            : null;

    public IReadOnlyList<int> PlayOrder => _playOrder.AsReadOnly();

    public bool HasNext
    {
        get
        {
            if (_playOrder.Count == 0) return false;
            if (RepeatMode == RepeatMode.One) return true;
            if (RepeatMode == RepeatMode.All) return true;
            return _currentIndex < _playOrder.Count - 1;
        }
    }

    public bool HasPrevious
    {
        get
        {
            if (_playOrder.Count == 0) return false;
            if (RepeatMode == RepeatMode.One) return true;
            if (RepeatMode == RepeatMode.All) return true;
            return _currentIndex > 0;
        }
    }

    public void SetQueue(IReadOnlyList<int> trackIds)
    {
        _originalOrder = new List<int>(trackIds);
        _playOrder = new List<int>(trackIds);
        _currentIndex = trackIds.Count > 0 ? 0 : -1;
        _isShuffled = false;
    }

    public void Clear()
    {
        _originalOrder.Clear();
        _playOrder.Clear();
        _currentIndex = -1;
        _isShuffled = false;
    }

    public int? MoveNext()
    {
        if (_playOrder.Count == 0) return null;

        if (RepeatMode == RepeatMode.One)
        {
            return CurrentTrackId;
        }

        if (_currentIndex < _playOrder.Count - 1)
        {
            _currentIndex++;
            return CurrentTrackId;
        }

        if (RepeatMode == RepeatMode.All)
        {
            _currentIndex = 0;
            return CurrentTrackId;
        }

        return null;
    }

    public int? MovePrevious()
    {
        if (_playOrder.Count == 0) return null;

        if (RepeatMode == RepeatMode.One)
        {
            return CurrentTrackId;
        }

        if (_currentIndex > 0)
        {
            _currentIndex--;
            return CurrentTrackId;
        }

        if (RepeatMode == RepeatMode.All)
        {
            _currentIndex = _playOrder.Count - 1;
            return CurrentTrackId;
        }

        return null;
    }

    public void JumpTo(int index)
    {
        if (index < 0 || index >= _playOrder.Count)
            throw new ArgumentOutOfRangeException(nameof(index),
                $"Index {index} is out of range. Queue has {_playOrder.Count} items.");

        _currentIndex = index;
    }

    public QueueState GetState()
    {
        return new QueueState
        {
            OriginalOrder = new List<int>(_originalOrder),
            PlayOrder = new List<int>(_playOrder),
            CurrentIndex = _currentIndex,
            RepeatMode = RepeatMode,
            IsShuffled = _isShuffled
        };
    }

    public void RestoreState(QueueState state)
    {
        _originalOrder = new List<int>(state.OriginalOrder);
        _playOrder = new List<int>(state.PlayOrder);
        _currentIndex = state.PlayOrder.Count > 0
            ? Math.Clamp(state.CurrentIndex, 0, state.PlayOrder.Count - 1)
            : -1;
        RepeatMode = state.RepeatMode;
        _isShuffled = state.IsShuffled;
    }

    public void ToggleShuffle()
    {
        if (_playOrder.Count == 0) return;

        var currentTrackId = CurrentTrackId;

        if (_isShuffled)
        {
            // Restore original order
            _playOrder = new List<int>(_originalOrder);
            _isShuffled = false;
        }
        else
        {
            // Shuffle: Fisher-Yates, keeping current track at index 0
            _playOrder = new List<int>(_originalOrder);
            _isShuffled = true;

            for (int i = _playOrder.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (_playOrder[i], _playOrder[j]) = (_playOrder[j], _playOrder[i]);
            }

            // Move current track to position 0 so playback continues seamlessly
            if (currentTrackId.HasValue)
            {
                int currentPos = _playOrder.IndexOf(currentTrackId.Value);
                if (currentPos >= 0)
                {
                    (_playOrder[0], _playOrder[currentPos]) = (_playOrder[currentPos], _playOrder[0]);
                }
            }
        }

        // Restore current track position
        if (currentTrackId.HasValue)
        {
            _currentIndex = _playOrder.IndexOf(currentTrackId.Value);
        }
    }
}