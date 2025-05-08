namespace LBG.LBGTools.Signal;

/// <summary>
/// A type-safe signal that takes 3 arguments.
/// </summary>
/// <typeparam name="T1">The type of the first argument passed to the signal's listeners.</typeparam>
/// <typeparam name="T2">The type of the second argument passed to the signal's listeners.</typeparam>
/// <typeparam name="T3">The type of the third argument passed to the signal's listeners.</typeparam>
public sealed class LBGSignal<T1, T2, T3> : AbstractLBGSignal<Action<T1, T2, T3>, ValueTuple<T1, T2, T3>> {

    /// <summary>
    /// Converts a user-supplied Action<T1, T2, T3> into an Action<(T1, T2, T3)> used internally.
    /// </summary>
    protected override Action<(T1, T2, T3)> WrapActionArgsToTuple(Action<T1, T2, T3> callback) => t => callback(t.Item1, t.Item2, t.Item3);

    /// <summary>
    /// Emits the signal with three arguments.
    /// </summary>
    public void Emit(T1 arg1, T2 arg2, T3 arg3) => Emit(new ValueTuple<T1, T2, T3>(arg1, arg2, arg3));
}
