using System;
using System.Collections.Generic;
using System.Linq;

namespace LBG.LBGTools.Signal;

/// <summary>
/// Base abstract LBGSignal class that handles the logic by wrapping every callback in a tuple.
/// It also keeps a map of callbacks to their wrapped version, allowing for "finding" the callback by its original signature.
/// This allows it to handle any arity of callbacks for a minimal performance cost, e.g. Action<T1>, Action<T1, T2>, Action<T1, T2, T3>, etc.
/// Note: NOT currently thread-safe.
/// </summary>
///
/// <typeparam name="TExposed">
/// The type used by this adapter layer, exposed to the classes that use/extend it, e.g. Action<T1, T2, T3>.
/// It is the type that gets "exposed" though the public interface of this class, e.g. Add, Remove, etc.
/// Has to be a delegate type (e.g. Action, Func, etc.).
/// </typeparam>
///
/// <typeparam name="TTupled">
/// The type that TExposed gets wrapped into, to be used by LBGSignal<T> under the hood, e.g. (T1, T2, T3).
/// This one is a tuple type, not a delegate.
/// </typeparam>
//
// TODO: **Weak ref listeners** (and make it default?)
// TODO: Implement the "Awaiter Pattern", allowing us to directly await a signal itself, e.g. "await MouseEntered"
// TODO: **Autosignals** that _observe_ a predicate or property or field, and can emit changes, toggles, or "every frame this is true" and the likes.
// TODO: Deboucing and/or throttling as an option?
// TODO: ASync emit worth anything?
// TODO: Make it thread-safe?
// TODO: Conditional listeners and/or conditional emits?
// TODO: Grouped or tagged listeners? (E.g. "EmitFor("enemy", 12.5f)")
// TODO: Pausing or interrupting a dispatch?
// TODO: "Emit delayed"?
// TODO: Hooking into the dispatch of another signal?
// TODO: "Safe listen" that are more like "TryEmit" ? Meaning the call catches any exceptions silently (or allows for another callback)
// TODO: ignal-level error handler?
// TODO: History and/or caching of last payloads? Bad idea? E.g. "Replay last event"? Can a closure give a 100% safe "snapshot"?
// TODO: Bubbling, capturing, filtering?
// TODO: "SkipX" instead of "CallLimit" ? E.g. "SkipFirst(3)" or "Every(2)"
// TODO: Atomic operations possible even for emit?
// TODO: EmitTo() ? Like targeting by delegate, or index, or propertion, or randomly...?
// TODO:
// TODO:
// TODO:
public abstract class AbstractLBGSignal<TExposed, TTupled> where TExposed : Delegate // i.e. it "must be a function"
{
    /// <summary>
    /// A map of the Delegate to DelegateEntry, allowing for the lookup of potentially wrapped callbacks (e.g. lambdas being used)
    /// </summary>
    internal readonly Dictionary<TExposed, ListenerEntry> _map = [];

    /// <summary>
    /// Converts a user-supplied callback (e.g. Action<T1,T2>)
    /// into a callback that takes the internal wrapped payload (e.g. Action<(T1,T2)>).
    /// </summary>
    /// <remarks> This has to be "hard-coded" for each arity </remarks>
    protected abstract Action<TTupled> WrapActionArgsToTuple(TExposed callback);

    /// <summary> The list of callbacks to be called when the event is triggered. </summary>
	protected List<ListenerEntry> Listeners { get; } = [];

    /// <summary> Whether the signal is currently mid-emission. </summary>
    public bool IsEmitting { get; private set; } = false;

    /// <summary> Whether the signal is currently in an emission that has been halted. </summary>
    public bool InterruptRequested { get; private set; } = false;

    /// <summary>
    /// Stops an ongoing emission to remaining listeners.
    /// E.g. for when a UI element wishes to "stop propagation".
    /// Has no effect outside of an ongoing Emit() call.
    /// </summary>
    public void InterruptCurrentEmission() {
        InterruptRequested = true;
    }

    // ------------------------------------------
    // Public interface
    // ------------------------------------------

    /// <summary>
    /// Adds a listener for this signal.
    /// </summary>
    /// <param name="callbackAction">The callback action to be invoked when the signal is triggered.</param>
    /// <remarks>
    /// Use the fluent API to set the priority and times to trigger. E.g.:
    /// <code>signal.Add(callback).WithPriority(1).CallLimit(2);</code>
    /// <code>signal.Add(callback).Once();</code>
    /// </remarks>
    public ListenerEntry Add(TExposed callbackAction) {
        var listenerEntry = GetOrAddNewListener(callbackAction); // This also adds the ListenerEntry if it had to be created.
        return listenerEntry;
    }

    /// <summary> Whether there is a callback registered for this signature. </summary>
    public bool Contains(TExposed callbackAction) => _map.ContainsKey(callbackAction);

    /// <summary> Removes a callback from the signal, by its original signature. </summary>
    public void Remove(TExposed callbackAction) {
        if (_map.TryGetValue(callbackAction, out var wrapped)) {
            // We do have an entry for this callback, so we can remove it.
            Listeners.Remove(wrapped);
            _map.Remove(callbackAction);
        }
    }

    /// <summary> Removes all registered callbacks. </summary>
    public void Clear() {
        Listeners.Clear();
        _map.Clear();
    }

    /// <summary> Returns the number of registered callbacks. </summary>
    public int Count => Listeners.Count;

    /// <summary> Triggers the signal, calling all registered callbacks. </summary>
    /// <param name="arg">The argument to pass to the callbacks.</param>
    public void Emit(TTupled arg) {
        IsEmitting = true;
        InterruptRequested = false;

        // Prepare and sort the list of callbacks to be called.
        var toRun = Listeners
            .OrderByDescending(e => e.Priority) // Sort by priority, highest first.
            .ToList(); // Create a copy of the list to avoid modifying it while iterating.

        // Call each callback in the sorted list.
        foreach (var entry in toRun) {
            if (!InterruptRequested) {
                entry.Invoke(arg);
            } else {
                // A listener requested to halt the emission ("consumed the event"), we stop dispatching.
                break;
            }
        }

        // Remove expired entries from the list of listeners
        Listeners.RemoveAll(cb => cb.IsExpired);

        // Remove expired entries from the map as well.
        foreach (var key in _map.Where(kvp => kvp.Value.IsExpired).Select(kvp => kvp.Key).ToList()) {
            _map.Remove(key);
        }

        IsEmitting = false;
        InterruptRequested = false;
    }

    // ------------------------------------------
    // Helper functions
    // ------------------------------------------

    /// <summary>
    /// Helper function that checks if the callback is already wrapped and stored in the map.
    /// This allows us to use the wrapped version of callbacks just like the original ones.
    /// </summary>
    /// <param name="sourceDelegate"> The target callback function </param>
    /// <returns> A new or existing ListenerEntry for this sourceDelegate. </returns>
    protected ListenerEntry GetOrAddNewListener(TExposed sourceDelegate) {
        if (_map.TryGetValue(sourceDelegate, out var existingEntry)) {
            // We already have a ListenerEntry for this callback, so just return it.
            return existingEntry;
        }

        // We don't have a wrapped version yet, so create one and store it in the map.
        var actionUsingTupledArgs = WrapActionArgsToTuple(sourceDelegate);
        var newEntry = ListenerEntry.CreateFrom(actionUsingTupledArgs);
        _map[sourceDelegate] = newEntry;
        if (!Listeners.Contains(newEntry)) Listeners.Add(newEntry);
        return newEntry;
    }
}
