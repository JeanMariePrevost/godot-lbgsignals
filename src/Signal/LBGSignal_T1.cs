namespace LBG.LBGTools.Signal;

/// <summary>
/// A type-safe signal that takes one argument.
/// </summary>
/// <typeparam name="T">The type of the argument passed to the signal's listeners.</typeparam>
public sealed class LBGSignal<T>
	: AbstractLBGSignal<Action<T>, ValueTuple<T>> {
	public LBGSignal() { }

	/// <summary>
	/// Converts a user-supplied Action<T1> into an Action<(T1)> used internally.
	/// </summary>
	protected override Action<ValueTuple<T>> WrapActionArgsToTuple(Action<T> callback) => t => callback(t.Item1);

	/// <summary>
	/// Emits the signal with one argument.
	/// </summary>
	public void Emit(T arg) => Emit(new ValueTuple<T>(arg));
}
