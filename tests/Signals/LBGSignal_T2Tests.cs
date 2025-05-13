using LBG.GodotTools.Signal;
using Xunit;

namespace LBG.Tests.GodotTools.Signal;
public class LBGSignal_T2Tests {
    [Fact]
    public void Add_Callback_ShouldPassValue() {
        // Arrange
        var signal = new LBGSignal<int, string>();
        var value1 = 1;
        var value2 = "Hello";

        signal.Add((arg1, arg2) => {
            value1 += arg1;
            value2 += arg2;
        });

        // Act
        signal.Emit(3, " World");

        // Assert
        Assert.Equal(4, value1);
        Assert.Equal("Hello World", value2);
    }

    [Fact]
    public void Add_Callback_ShouldPassNull() {
        // Arrange
        var signal = new LBGSignal<int?, string?>();
        int? value1 = 1;
        string? value2 = "Hello";

        signal.Add((arg1, arg2) => {
            value1 = arg1;
            value2 = arg2;
        });

        // Act
        signal.Emit(null, null);

        // Assert
        Assert.Null(value1);
        Assert.Null(value2);
    }

    [Fact]
    public void AddOnce_Callback_ShouldBeCalledOnce() {
        // Arrange
        var signal = new LBGSignal<int, int>();
        var callCount = 0;

        signal.Add((arg1, arg2) => callCount++).Once();

        // Act
        signal.Emit(42, 43);
        signal.Emit(42, 43);
        signal.Emit(42, 43);

        // Assert
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void AddLimited_Callback_ShouldBeCalledLimitedTimes() {
        // Arrange
        var signal = new LBGSignal<int, int>();
        var callCount = 0;

        signal.Add((arg1, arg2) => callCount++).CallLimit(2);

        // Act
        signal.Emit(42, 43);
        signal.Emit(42, 43);
        signal.Emit(42, 43);
        signal.Emit(42, 43);

        // Assert
        Assert.Equal(2, callCount);
    }

    [Fact]
    public void Remove_Callback_ShouldNotBeCalledAfterRemoval() {
        // Arrange
        var signal = new LBGSignal<int, int>();
        var wasCalled = false;
        Action<int, int> callback = (arg1, arg2) => wasCalled = true;

        signal.Add(callback);
        signal.Remove(callback);

        // Act
        signal.Emit(42, 43);

        // Assert
        Assert.False(wasCalled);
    }

    [Fact]
    public void ComplexObjects_ShouldBePassedCorrectly() {
        // Arrange
        var signal1 = new LBGSignal<(int, string, float)?, List<string>>();
        (int, string, float)? testObject1 = (123, "Hello", 456.789f);
        List<string> testObject2 = ["A", "B", "C"];
        List<string> receivedObject2 = [];
        (int, string, float)? receivedObject1 = null;

        signal1.Add((arg1, arg2) => {
            receivedObject1 = arg1;
            receivedObject2 = arg2;
        });

        // Act
        signal1.Emit(testObject1, testObject2);

        // Assert
        Assert.Equal(testObject1, receivedObject1);
        Assert.NotNull(receivedObject2);
        Assert.Equal(testObject2, receivedObject2);
        testObject2.Remove("A");
        Assert.Equal(["B", "C"], receivedObject2);
        Assert.Equal(testObject2, receivedObject2);
    }

    [Fact]
    public void Remove_NonExistentCallback_ShouldNotThrow() {
        // Arrange
        var signal = new LBGSignal<int, int>();
        var wasCalled = false;
        var wasNotCalled = false;
        void callback(int arg1, int arg2) => wasNotCalled = true;

        signal.Add((arg1, arg2) => wasCalled = true);
        signal.Remove(callback); // Attempt to remove a non-existent callback

        // Act
        signal.Emit(42, 43);

        // Assert
        Assert.True(wasCalled);
        Assert.False(wasNotCalled);
    }

    [Fact]
    public void Clear_ShouldRemoveAllCallbacks() {
        // Arrange
        var signal = new LBGSignal<int, int>();
        var wasCalled = false;

        signal.Add((arg1, arg2) => wasCalled = true);
        signal.Clear();

        // Act
        signal.Emit(42, 43);

        // Assert
        Assert.Equal(0, signal.Count);
        Assert.False(wasCalled);
    }

    [Fact]
    public void Emit_ShouldCallCallbacksInPriorityOrder() {
        // Arrange
        var signal = new LBGSignal<int, int>();
        var callOrder = new List<int>();

        signal.Add((arg1, arg2) => callOrder.Add(1)).WithPriority(-101);
        signal.Add((arg1, arg2) => callOrder.Add(2)).WithPriority(1);
        signal.Add((arg1, arg2) => callOrder.Add(3)).WithPriority(2);
        signal.Add((arg1, arg2) => callOrder.Add(4)).WithPriority(0);
        signal.Add((arg1, arg2) => callOrder.Add(5)).WithPriority(-100);

        // Act
        signal.Emit(42, 43);

        // Assert
        Assert.Equal(new List<int> { 3, 2, 4, 5, 1 }, callOrder);
    }
}
