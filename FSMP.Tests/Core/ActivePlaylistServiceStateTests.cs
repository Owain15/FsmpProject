using FluentAssertions;
using FSMP.Core;
using FSMP.Core.Models;

namespace FSMP.Tests.Core;

public class ActivePlaylistServiceStateTests
{
    private ActivePlaylistService CreateService(int seed = 42)
        => new ActivePlaylistService(new Random(seed));

    [Fact]
    public void GetState_EmptyService_ReturnsEmptyState()
    {
        var svc = CreateService();
        var state = svc.GetState();

        state.OriginalOrder.Should().BeEmpty();
        state.PlayOrder.Should().BeEmpty();
        state.CurrentIndex.Should().Be(-1);
        state.RepeatMode.Should().Be(RepeatMode.None);
        state.IsShuffled.Should().BeFalse();
    }

    [Fact]
    public void GetState_PopulatedService_CapturesAllFields()
    {
        var svc = CreateService();
        svc.SetQueue(new[] { 10, 20, 30 });
        svc.MoveNext(); // index 1
        svc.RepeatMode = RepeatMode.All;

        var state = svc.GetState();

        state.OriginalOrder.Should().Equal(10, 20, 30);
        state.PlayOrder.Should().Equal(10, 20, 30);
        state.CurrentIndex.Should().Be(1);
        state.RepeatMode.Should().Be(RepeatMode.All);
        state.IsShuffled.Should().BeFalse();
    }

    [Fact]
    public void GetState_ShuffledService_CapturesShuffleState()
    {
        var svc = CreateService();
        svc.SetQueue(new[] { 1, 2, 3, 4, 5 });
        svc.ToggleShuffle();

        var state = svc.GetState();

        state.IsShuffled.Should().BeTrue();
        state.OriginalOrder.Should().Equal(1, 2, 3, 4, 5);
        state.PlayOrder.Should().NotEqual(state.OriginalOrder);
    }

    [Fact]
    public void RestoreState_RoundTrip_PreservesAllFields()
    {
        var svc = CreateService();
        svc.SetQueue(new[] { 10, 20, 30 });
        svc.MoveNext();
        svc.RepeatMode = RepeatMode.One;
        var state = svc.GetState();

        var svc2 = CreateService();
        svc2.RestoreState(state);

        svc2.Count.Should().Be(3);
        svc2.CurrentIndex.Should().Be(1);
        svc2.CurrentTrackId.Should().Be(20);
        svc2.RepeatMode.Should().Be(RepeatMode.One);
        svc2.IsShuffled.Should().BeFalse();
        svc2.PlayOrder.Should().Equal(10, 20, 30);
    }

    [Fact]
    public void RestoreState_EmptyState_ResultsInEmptyService()
    {
        var svc = CreateService();
        svc.SetQueue(new[] { 1, 2, 3 });

        svc.RestoreState(new QueueState());

        svc.Count.Should().Be(0);
        svc.CurrentIndex.Should().Be(-1);
        svc.CurrentTrackId.Should().BeNull();
    }

    [Fact]
    public void RestoreState_IndexOutOfBounds_ClampedToLastItem()
    {
        var svc = CreateService();
        var state = new QueueState
        {
            OriginalOrder = new List<int> { 1, 2, 3 },
            PlayOrder = new List<int> { 1, 2, 3 },
            CurrentIndex = 99
        };

        svc.RestoreState(state);

        svc.CurrentIndex.Should().Be(2);
    }

    [Fact]
    public void RestoreState_ShuffledState_PreservesShuffleFlag()
    {
        var svc = CreateService();
        var state = new QueueState
        {
            OriginalOrder = new List<int> { 1, 2, 3 },
            PlayOrder = new List<int> { 3, 1, 2 },
            CurrentIndex = 0,
            IsShuffled = true,
            RepeatMode = RepeatMode.All
        };

        svc.RestoreState(state);

        svc.IsShuffled.Should().BeTrue();
        svc.RepeatMode.Should().Be(RepeatMode.All);
        svc.PlayOrder.Should().Equal(3, 1, 2);
    }

    [Fact]
    public void GetState_ReturnsCopy_ModifyingStateDoesNotAffectService()
    {
        var svc = CreateService();
        svc.SetQueue(new[] { 1, 2, 3 });
        var state = svc.GetState();

        state.PlayOrder.Add(99);

        svc.Count.Should().Be(3);
    }
}
