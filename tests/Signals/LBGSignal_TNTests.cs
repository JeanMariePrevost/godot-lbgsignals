using LBG.GodotTools.Signal;
using Xunit;

namespace LBG.Tests.GodotTools.Signal;
public class LBGSignal_TNTests {

    [Fact]
    public void LBGSignal_T0_Everything_ShouldWork() {
        // Arrange
        var signal = new LBGSignal();

        var list = new List<string>();

        void normal() => list.Add("N");
        void once() => list.Add("O");
        void limited() => list.Add("L");
        void toRemove() => list.Add("FAIL");

        signal.Add(normal).WithPriority(1);
        signal.Add(once).Once();
        signal.Add(limited).CallLimit(2);
        signal.Add(toRemove);
        signal.Remove(toRemove);

        // Act
        signal.Emit();
        signal.Emit();
        signal.Clear();
        signal.Emit();

        // Assert
        var expected = new List<string> { "N", "O", "L", "N", "L" };

        Assert.Equal(expected, list);
    }

    [Fact]
    public void LBGSignal_T1_Everything_ShouldWork() {
        // Arrange
        var signal = new LBGSignal<int>();

        var list = new List<string>();

        void normal(int i) => list.Add($"N:{i}");
        void once(int i) => list.Add($"O:{i}");
        void limited(int i) => list.Add($"L:{i}");
        void toRemove(int i) => list.Add("FAIL");

        signal.Add(normal).WithPriority(1);
        signal.Add(once).Once();
        signal.Add(limited).CallLimit(2);
        signal.Add(toRemove);
        signal.Remove(toRemove);

        // Act
        signal.Emit(1);
        signal.Emit(2);
        signal.Clear();
        signal.Emit(3);

        // Assert
        var expected = new List<string>
        { "N:1", "O:1", "L:1", "N:2", "L:2" };

        Assert.Equal(expected, list);
    }

    [Fact]
    public void LBGSignal_T2_Everything_ShouldWork() {
        // Arrange
        var signal = new LBGSignal<int, string>();

        var list = new List<string>();

        void normal(int i, string s) => list.Add($"N:{i}:{s}");
        void once(int i, string s) => list.Add($"O:{i}:{s}");
        void limited(int i, string s) => list.Add($"L:{i}:{s}");
        void toRemove(int i, string s) => list.Add("FAIL");

        signal.Add(normal).WithPriority(1);
        signal.Add(once).Once();
        signal.Add(limited).CallLimit(2);
        signal.Add(toRemove);
        signal.Remove(toRemove);

        // Act
        signal.Emit(1, "a");
        signal.Emit(2, "b");
        signal.Clear();
        signal.Emit(3, "c");

        // Assert
        var expected = new List<string>
        { "N:1:a", "O:1:a", "L:1:a", "N:2:b", "L:2:b" };

        Assert.Equal(expected, list);
    }

    [Fact]
    public void LBGSignal_T3_Everything_ShouldWork() {
        // Arrange
        var signal = new LBGSignal<int, string, bool>();

        var log = new List<string>();

        Action<int, string, bool> normal = (a, b, c) => log.Add($"N:{a}:{b}:{c}");
        Action<int, string, bool> once = (a, b, c) => log.Add($"O:{a}:{b}:{c}");
        Action<int, string, bool> limited = (a, b, c) => log.Add($"L:{a}:{b}:{c}");
        Action<int, string, bool> toRemove = (a, b, c) => log.Add("FAIL");

        signal.Add(normal).WithPriority(0);
        signal.Add(once).Once();
        signal.Add(limited).CallLimit(2);
        signal.Add(toRemove);
        signal.Remove(toRemove);

        // Act
        signal.Emit(1, "a", true);
        signal.Emit(2, "b", false);
        signal.Clear();
        signal.Emit(3, "c", true); // nothing should run after clear

        // Assert
        var expected = new List<string>
        {
        "N:1:a:True", "O:1:a:True", "L:1:a:True",
        "N:2:b:False", "L:2:b:False"
    };

        Assert.Equal(expected, log);
    }

}
