using LBG.GodotTools.Signal;
using Xunit;

namespace LBG.Tests.GodotTools.Signal;
public class LBGSignal_T1Tests {
    [Fact]
    public void Add_Callback_ShouldPassValue() {
        // Arrange
        var signal = new LBGSignal<int>();
        var value = 1;

        signal.Add(arg => value += arg);

        // Act
        signal.Emit(3);

        // Assert
        Assert.Equal(4, value);
    }

    [Fact]
    public void Add_Callback_ShouldPassNull() {
        // Arrange
        var signal = new LBGSignal<int?>();
        int? value = 1;

        signal.Add(arg => value = arg);

        // Act
        signal.Emit(null);

        // Assert
        Assert.Null(value);
    }

    [Fact]
    public void AddOnce_Callback_ShouldBeCalledOnce() {
        // Arrange
        var signal = new LBGSignal<int>();
        var callCount = 0;

        signal.Add(arg => callCount++).Once();

        // Act
        signal.Emit(42);
        signal.Emit(42);

        // Assert
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Limited_Callback_ShouldBeRemovedAfterCall() {
        // Arrange
        var signal = new LBGSignal<int>();
        var callCount = 0;

        signal.Add(arg => callCount++).Once();
        signal.Add(arg => callCount++).CallLimit(1);
        signal.Add(arg => callCount++).CallLimit(2);
        signal.Add(arg => callCount++).CallLimit(3);
        signal.Add(arg => callCount++);

        // Act / Assert
        Assert.Equal(5, signal.Count);
        signal.Emit(42);
        Assert.Equal(5, callCount);
        Assert.Equal(3, signal.Count);
        signal.Emit(42);
        Assert.Equal(8, callCount);
        Assert.Equal(2, signal.Count);
        signal.Emit(42);
        Assert.Equal(10, callCount);
        Assert.Equal(1, signal.Count);
        signal.Emit(42);
        Assert.Equal(11, callCount);
        Assert.Equal(1, signal.Count);
    }

    [Fact]
    public void AddLimited_Callback_ShouldBeCalledLimitedTimes() {
        // Arrange
        var signal = new LBGSignal<int>();
        var callCount = 0;

        signal.Add(arg => callCount++).CallLimit(2);

        // Act
        signal.Emit(42);
        signal.Emit(42);
        signal.Emit(42);
        signal.Emit(42);

        // Assert
        Assert.Equal(2, callCount);
    }

    [Fact]
    public void Remove_Callback_ShouldNotBeCalledAfterRemoval() {
        // Arrange
        var signal = new LBGSignal<int>();
        var wasCalled = false;
        Action<int> callback = arg => wasCalled = true;

        signal.Add(callback);
        signal.Remove(callback);

        // Act
        signal.Emit(42);

        // Assert
        Assert.False(wasCalled);
    }

    [Fact]
    public void EmittingSignal_ShouldReportCorrectState() {
        // Arrange
        var signal = new LBGSignal<int>();
        var reportsEmitting = false;

        signal.Add(arg => reportsEmitting = signal.IsEmitting);

        // Act
        signal.Emit(42);

        // Assert
        Assert.False(signal.IsEmitting);
        Assert.False(signal.InterruptRequested);
        Assert.True(reportsEmitting);
    }

    [Fact]
    public void InterruptedSignal_ShouldStopEmitting() {
        // Arrange
        var signal = new LBGSignal<int>();
        var callCount = 0;

        signal.Add(arg => callCount++).WithPriority(5);
        signal.Add(_ => Assert.True(signal.IsEmitting)).WithPriority(4);
        signal.Add(_ => Assert.False(signal.InterruptRequested)).WithPriority(4);
        signal.Add(_ => signal.InterruptCurrentEmission()).WithPriority(3);
        signal.Add(_ => Assert.True(signal.IsEmitting)).WithPriority(2);
        signal.Add(_ => Assert.True(signal.InterruptRequested)).WithPriority(2);
        signal.Add(arg => callCount++).WithPriority(1);

        // Act
        signal.Emit(42);

        // Assert
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void ComplexObjects_ShouldBePassedCorrectly() {
        // Arrange
        var signal1 = new LBGSignal<(int, string, float)?>();
        (int, string, float)? testObject1 = (123, "Hello", 456.789f);
        (int, string, float)? receivedObject1 = null;

        var signal2 = new LBGSignal<List<string>>();
        List<string> testObject2 = ["A", "B", "C"];
        List<string> receivedObject2 = [];

        signal1.Add(arg => receivedObject1 = arg);
        signal2.Add(arg => receivedObject2 = arg);

        // Act
        signal1.Emit(testObject1);
        signal2.Emit(testObject2);

        // Assert
        Assert.Equal(testObject1, receivedObject1);
        Assert.NotEqual(testObject2, new List<string>());
        Assert.Equal(testObject2, new List<string> { "A", "B", "C" });
        Assert.Equal(testObject2, receivedObject2);
        testObject2.Remove("A");
        Assert.NotEqual(testObject2, new List<string> { "A", "B", "C" });
        Assert.Equal(testObject2, receivedObject2);
    }

    [Fact]
    public void Remove_NonExistentCallback_ShouldNotThrow() {
        // Arrange
        var signal = new LBGSignal<int>();
        var wasCalled = false;
        void callback(int arg) => wasCalled = true;

        signal.Add(arg => wasCalled = true);
        signal.Remove(callback); // Attempt to remove a non-existent callback

        // Act
        signal.Emit(42);

        // Assert
        Assert.True(wasCalled);
    }

    [Fact]
    public void Clear_ShouldRemoveAllCallbacks() {
        // Arrange
        var signal = new LBGSignal<int>();
        var wasCalled = false;

        signal.Add(arg => wasCalled = true);
        signal.Clear();

        // Act
        signal.Emit(42);

        // Assert
        Assert.Equal(0, signal.Count);
        Assert.False(wasCalled);
    }

    [Fact]
    public void Emit_ShouldCallCallbacksInPriorityOrder() {
        // Arrange
        var signal = new LBGSignal<int>();
        var callOrder = new List<int>();

        signal.Add(arg => callOrder.Add(1)).WithPriority(-101);
        signal.Add(arg => callOrder.Add(2)).WithPriority(1);
        signal.Add(arg => callOrder.Add(3)).WithPriority(2);
        signal.Add(arg => callOrder.Add(4)).WithPriority(0);
        signal.Add(arg => callOrder.Add(5)).WithPriority(-100);

        // Act
        signal.Emit(42);

        // Assert
        Assert.Equal(new List<int> { 3, 2, 4, 5, 1 }, callOrder);
    }
}
