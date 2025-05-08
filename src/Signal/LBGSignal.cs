
using System;

namespace LBG.LBGTools.Signal;

public readonly struct Unit { }   // empty struct, just a placeholder for "void" type

// your non-generic signal
public sealed class LBGSignal
  : AbstractLBGSignal<Action, Unit> {
    // wrap the void Action into an Action<Unit> that ignores its parameter
    protected override Action<Unit> WrapActionArgsToTuple(Action callback)
        => _ => callback();

    // expose the clean no-arg Emit() to users
    public void Emit()
        => Emit(default);
}
