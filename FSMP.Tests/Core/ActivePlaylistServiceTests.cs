using FluentAssertions;
using FSMP.Core;

namespace FSMP.Tests.Core;

public class ActivePlaylistServiceTests
{
    private ActivePlaylistService CreateService(int seed = 42)
        => new ActivePlaylistService(new Random(seed));

    // --- Initial State ---

    [Fact]
    public void NewService_ShouldHaveEmptyState()
    {
        // Arrange & Act
        var svc = CreateService();

        // Assert
        svc.Count.Should().Be(0);
        svc.CurrentIndex.Should().Be(-1);
        svc.CurrentTrackId.Should().BeNull();
        svc.IsShuffled.Should().BeFalse();
        svc.RepeatMode.Should().Be(RepeatMode.None);
        svc.PlayOrder.Should().BeEmpty();
        svc.HasNext.Should().BeFalse();
        svc.HasPrevious.Should().BeFalse();
    }

    // --- SetQueue ---

    [Fact]
    public void SetQueue_ShouldLoadTracksAndSetIndexToZero()
    {
        // Arrange
        var svc = CreateService();
        var trackIds = new List<int> { 10, 20, 30 };

        // Act
        svc.SetQueue(trackIds);

        // Assert
        svc.Count.Should().Be(3);
        svc.CurrentIndex.Should().Be(0);
        svc.CurrentTrackId.Should().Be(10);
        svc.PlayOrder.Should().BeEquivalentTo(new[] { 10, 20, 30 }, o => o.WithStrictOrdering());
    }

    [Fact]
    public void SetQueue_WithEmptyList_ShouldSetIndexToNegativeOne()
    {
        // Arrange
        var svc = CreateService();

        // Act
        svc.SetQueue(new List<int>());

        // Assert
        svc.Count.Should().Be(0);
        svc.CurrentIndex.Should().Be(-1);
        svc.CurrentTrackId.Should().BeNull();
    }

    [Fact]
    public void SetQueue_ShouldResetShuffleState()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 1, 2, 3 });
        svc.ToggleShuffle();
        svc.IsShuffled.Should().BeTrue();

        // Act
        svc.SetQueue(new List<int> { 10, 20 });

        // Assert
        svc.IsShuffled.Should().BeFalse();
    }

    // --- Clear ---

    [Fact]
    public void Clear_ShouldResetAllState()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 1, 2, 3 });
        svc.MoveNext();
        svc.ToggleShuffle();

        // Act
        svc.Clear();

        // Assert
        svc.Count.Should().Be(0);
        svc.CurrentIndex.Should().Be(-1);
        svc.CurrentTrackId.Should().BeNull();
        svc.IsShuffled.Should().BeFalse();
        svc.PlayOrder.Should().BeEmpty();
    }

    // --- MoveNext ---

    [Fact]
    public void MoveNext_ShouldAdvanceThroughQueue()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10, 20, 30 });

        // Act & Assert
        svc.CurrentTrackId.Should().Be(10);

        svc.MoveNext().Should().Be(20);
        svc.CurrentIndex.Should().Be(1);

        svc.MoveNext().Should().Be(30);
        svc.CurrentIndex.Should().Be(2);
    }

    [Fact]
    public void MoveNext_AtEndWithRepeatNone_ShouldReturnNull()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10, 20 });
        svc.RepeatMode = RepeatMode.None;
        svc.MoveNext(); // move to 20

        // Act
        var result = svc.MoveNext();

        // Assert
        result.Should().BeNull();
        svc.CurrentIndex.Should().Be(1); // stays at end
    }

    [Fact]
    public void MoveNext_AtEndWithRepeatAll_ShouldWrapToBeginning()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10, 20 });
        svc.RepeatMode = RepeatMode.All;
        svc.MoveNext(); // move to 20

        // Act
        var result = svc.MoveNext();

        // Assert
        result.Should().Be(10);
        svc.CurrentIndex.Should().Be(0);
    }

    [Fact]
    public void MoveNext_WithRepeatOne_ShouldReturnSameTrack()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10, 20, 30 });
        svc.RepeatMode = RepeatMode.One;

        // Act
        var result = svc.MoveNext();

        // Assert
        result.Should().Be(10);
        svc.CurrentIndex.Should().Be(0);
    }

    [Fact]
    public void MoveNext_OnEmptyQueue_ShouldReturnNull()
    {
        // Arrange
        var svc = CreateService();

        // Act
        var result = svc.MoveNext();

        // Assert
        result.Should().BeNull();
    }

    // --- MovePrevious ---

    [Fact]
    public void MovePrevious_ShouldGoBackThroughQueue()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10, 20, 30 });
        svc.MoveNext(); // at 20
        svc.MoveNext(); // at 30

        // Act & Assert
        svc.MovePrevious().Should().Be(20);
        svc.CurrentIndex.Should().Be(1);

        svc.MovePrevious().Should().Be(10);
        svc.CurrentIndex.Should().Be(0);
    }

    [Fact]
    public void MovePrevious_AtStartWithRepeatNone_ShouldReturnNull()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10, 20 });
        svc.RepeatMode = RepeatMode.None;

        // Act
        var result = svc.MovePrevious();

        // Assert
        result.Should().BeNull();
        svc.CurrentIndex.Should().Be(0);
    }

    [Fact]
    public void MovePrevious_AtStartWithRepeatAll_ShouldWrapToEnd()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10, 20, 30 });
        svc.RepeatMode = RepeatMode.All;

        // Act
        var result = svc.MovePrevious();

        // Assert
        result.Should().Be(30);
        svc.CurrentIndex.Should().Be(2);
    }

    [Fact]
    public void MovePrevious_WithRepeatOne_ShouldReturnSameTrack()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10, 20, 30 });
        svc.MoveNext(); // at 20
        svc.RepeatMode = RepeatMode.One;

        // Act
        var result = svc.MovePrevious();

        // Assert
        result.Should().Be(20);
        svc.CurrentIndex.Should().Be(1);
    }

    [Fact]
    public void MovePrevious_OnEmptyQueue_ShouldReturnNull()
    {
        // Arrange
        var svc = CreateService();

        // Act
        var result = svc.MovePrevious();

        // Assert
        result.Should().BeNull();
    }

    // --- HasNext / HasPrevious ---

    [Fact]
    public void HasNext_AtMiddle_ShouldBeTrue()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10, 20, 30 });

        // Assert
        svc.HasNext.Should().BeTrue();
    }

    [Fact]
    public void HasNext_AtEndWithRepeatNone_ShouldBeFalse()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10, 20 });
        svc.MoveNext(); // at end
        svc.RepeatMode = RepeatMode.None;

        // Assert
        svc.HasNext.Should().BeFalse();
    }

    [Fact]
    public void HasNext_AtEndWithRepeatAll_ShouldBeTrue()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10, 20 });
        svc.MoveNext(); // at end
        svc.RepeatMode = RepeatMode.All;

        // Assert
        svc.HasNext.Should().BeTrue();
    }

    [Fact]
    public void HasPrevious_AtStartWithRepeatNone_ShouldBeFalse()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10, 20 });
        svc.RepeatMode = RepeatMode.None;

        // Assert
        svc.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void HasPrevious_AtStartWithRepeatAll_ShouldBeTrue()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10, 20 });
        svc.RepeatMode = RepeatMode.All;

        // Assert
        svc.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public void HasNext_WithRepeatOne_ShouldBeTrue()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10 });
        svc.RepeatMode = RepeatMode.One;

        // Assert
        svc.HasNext.Should().BeTrue();
        svc.HasPrevious.Should().BeTrue();
    }

    // --- JumpTo ---

    [Fact]
    public void JumpTo_ValidIndex_ShouldSetCurrentIndex()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10, 20, 30, 40 });

        // Act
        svc.JumpTo(2);

        // Assert
        svc.CurrentIndex.Should().Be(2);
        svc.CurrentTrackId.Should().Be(30);
    }

    [Fact]
    public void JumpTo_NegativeIndex_ShouldThrow()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10, 20 });

        // Act
        var act = () => svc.JumpTo(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("index");
    }

    [Fact]
    public void JumpTo_IndexBeyondEnd_ShouldThrow()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10, 20 });

        // Act
        var act = () => svc.JumpTo(2);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("index");
    }

    // --- ToggleShuffle ---

    [Fact]
    public void ToggleShuffle_ShouldShufflePlayOrder()
    {
        // Arrange
        var svc = CreateService(seed: 42);
        var originalIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        svc.SetQueue(originalIds);

        // Act
        svc.ToggleShuffle();

        // Assert
        svc.IsShuffled.Should().BeTrue();
        svc.Count.Should().Be(10);
        // Shuffled order should contain same elements
        svc.PlayOrder.Should().BeEquivalentTo(originalIds);
        // With 10 items and a fixed seed, it's extremely unlikely to be identical
        svc.PlayOrder.Should().NotEqual(originalIds);
    }

    [Fact]
    public void ToggleShuffle_ShouldPreserveCurrentTrack()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10, 20, 30, 40, 50 });
        svc.MoveNext(); // at 20

        // Act
        svc.ToggleShuffle();

        // Assert
        svc.CurrentTrackId.Should().Be(20);
    }

    [Fact]
    public void ToggleShuffle_Twice_ShouldRestoreOriginalOrder()
    {
        // Arrange
        var svc = CreateService();
        var originalIds = new List<int> { 10, 20, 30, 40, 50 };
        svc.SetQueue(originalIds);

        // Act
        svc.ToggleShuffle();
        svc.ToggleShuffle();

        // Assert
        svc.IsShuffled.Should().BeFalse();
        svc.PlayOrder.Should().BeEquivalentTo(originalIds, o => o.WithStrictOrdering());
    }

    [Fact]
    public void ToggleShuffle_Twice_ShouldPreserveCurrentTrack()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 10, 20, 30, 40, 50 });
        svc.MoveNext(); // at 20
        svc.MoveNext(); // at 30

        // Act
        svc.ToggleShuffle(); // shuffle
        svc.ToggleShuffle(); // unshuffle

        // Assert
        svc.CurrentTrackId.Should().Be(30);
    }

    [Fact]
    public void ToggleShuffle_OnEmptyQueue_ShouldDoNothing()
    {
        // Arrange
        var svc = CreateService();

        // Act
        svc.ToggleShuffle();

        // Assert
        svc.IsShuffled.Should().BeFalse();
        svc.Count.Should().Be(0);
    }

    // --- Shuffle + Navigation ---

    [Fact]
    public void Shuffle_ShouldAllowNavigationThroughShuffledOrder()
    {
        // Arrange
        var svc = CreateService(seed: 42);
        svc.SetQueue(new List<int> { 1, 2, 3, 4, 5 });
        svc.ToggleShuffle();
        var visited = new List<int>();

        // Act - walk through entire shuffled queue
        visited.Add(svc.CurrentTrackId!.Value);
        while (svc.MoveNext() is int nextId && !visited.Contains(nextId))
        {
            visited.Add(nextId);
        }

        // Assert - should visit all tracks
        visited.Should().HaveCount(5);
        visited.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
    }

    // --- RepeatMode property ---

    [Fact]
    public void RepeatMode_ShouldDefaultToNone()
    {
        // Arrange & Act
        var svc = CreateService();

        // Assert
        svc.RepeatMode.Should().Be(RepeatMode.None);
    }

    [Theory]
    [InlineData(RepeatMode.None)]
    [InlineData(RepeatMode.One)]
    [InlineData(RepeatMode.All)]
    public void RepeatMode_ShouldBeSettable(RepeatMode mode)
    {
        // Arrange
        var svc = CreateService();

        // Act
        svc.RepeatMode = mode;

        // Assert
        svc.RepeatMode.Should().Be(mode);
    }

    // --- Single-track edge cases ---

    [Fact]
    public void SingleTrack_MoveNext_WithRepeatAll_ShouldReturnSameTrack()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 42 });
        svc.RepeatMode = RepeatMode.All;

        // Act
        var result = svc.MoveNext();

        // Assert
        result.Should().Be(42);
        svc.CurrentIndex.Should().Be(0);
    }

    [Fact]
    public void SingleTrack_MoveNext_WithRepeatNone_ShouldReturnNull()
    {
        // Arrange
        var svc = CreateService();
        svc.SetQueue(new List<int> { 42 });
        svc.RepeatMode = RepeatMode.None;

        // Act
        var result = svc.MoveNext();

        // Assert
        result.Should().BeNull();
    }

    // --- Default constructor ---

    [Fact]
    public void DefaultConstructor_ShouldWork()
    {
        // Arrange & Act
        var svc = new ActivePlaylistService();

        // Assert
        svc.Count.Should().Be(0);
        svc.RepeatMode.Should().Be(RepeatMode.None);
    }
}
