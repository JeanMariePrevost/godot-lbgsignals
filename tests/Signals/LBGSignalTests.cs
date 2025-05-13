using LBG.GodotTools.Signal;
using Xunit;

namespace LBG.Tests.GodotTools.Signal;

public class LBGSignalTests {
    [Fact]
    public void Add_Callback_ShouldInvoke() {
        // Arrange
        var signal = new LBGSignal();
        var wasCalled = false;

        signal.Add(() => wasCalled = true);

        // Act
        signal.Emit();

        // Assert
        Assert.True(wasCalled);
    }

    [Fact]
    public void AddOnce_Callback_ShouldBeCalledOnce() {
        // Arrange
        var signal = new LBGSignal();
        var callCount = 0;

        signal.Add(() => callCount++).Once();

        // Act
        signal.Emit();
        signal.Emit();

        // Assert
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Limited_Callback_ShouldBeRemovedAfterCall() {
        // Arrange
        var signal = new LBGSignal();
        var callCount = 0;

        signal.Add(() => callCount++).Once();
        signal.Add(() => callCount++).CallLimit(1);
        signal.Add(() => callCount++).CallLimit(2);
        signal.Add(() => callCount++).CallLimit(3);
        signal.Add(() => callCount++);

        // Act / Assert
        Assert.Equal(5, signal.Count);
        signal.Emit();
        Assert.Equal(5, callCount);
        Assert.Equal(3, signal.Count);
        signal.Emit();
        Assert.Equal(8, callCount);
        Assert.Equal(2, signal.Count);
        signal.Emit();
        Assert.Equal(10, callCount);
        Assert.Equal(1, signal.Count);
        signal.Emit();
        Assert.Equal(11, callCount);
        Assert.Equal(1, signal.Count);
    }

    [Fact]
    public void AddLimited_Callback_ShouldBeCalledLimitedTimes() {
        // Arrange
        var signal = new LBGSignal();
        var callCount = 0;

        signal.Add(() => callCount++).CallLimit(2);

        // Act
        signal.Emit();
        signal.Emit();
        signal.Emit();
        signal.Emit();

        // Assert
        Assert.Equal(2, callCount);
    }

    [Fact]
    public void Remove_Callback_ShouldNotBeCalledAfterRemoval() {
        // Arrange
        var signal = new LBGSignal();
        var wasCalled = false;
        Action callback = () => wasCalled = true;

        signal.Add(callback);
        signal.Remove(callback);

        // Act
        signal.Emit();

        // Assert
        Assert.False(wasCalled);
    }

    [Fact]
    public void Remove_NonExistentCallback_ShouldNotThrow() {
        // Arrange
        var signal = new LBGSignal();
        var wasCalled = false;
        void callback() => wasCalled = true;

        signal.Add(() => wasCalled = true);
        signal.Remove(callback); // Attempt to remove a non‑existent callback

        // Act
        signal.Emit();

        // Assert
        Assert.True(wasCalled);
    }

    [Fact]
    public void Clear_ShouldRemoveAllCallbacks() {
        // Arrange
        var signal = new LBGSignal();
        var wasCalled = false;

        signal.Add(() => wasCalled = true);
        signal.Clear();

        // Act
        signal.Emit();

        // Assert
        Assert.Equal(0, signal.Count);
        Assert.False(wasCalled);
    }

    [Fact]
    public void Emit_ShouldCallCallbacksInPriorityOrder() {
        // Arrange
        var signal = new LBGSignal();
        var callOrder = new List<int>();

        signal.Add(() => callOrder.Add(1)).WithPriority(-101);
        signal.Add(() => callOrder.Add(2)).WithPriority(1);
        signal.Add(() => callOrder.Add(3)).WithPriority(2);
        signal.Add(() => callOrder.Add(4)).WithPriority(0);
        signal.Add(() => callOrder.Add(5)).WithPriority(-100);

        // Act
        signal.Emit();

        // Assert
        Assert.Equal(new List<int> { 3, 2, 4, 5, 1 }, callOrder);
    }
}
