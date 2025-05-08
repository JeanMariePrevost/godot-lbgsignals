namespace LBG.LBGTools.Signal;

/// <summary>
/// Internal record that wraps a user callback and tracks metadata
/// (e.g. priority, remaining calls).
/// Use fluent API to customize, e.g. entry.WithPriority(1).Once()
/// </summary>
public sealed class ListenerEntry {

    /// <summary> The original delegate passed by the user. Can be used for identification.</summary>
    public Delegate OriginalDelegate { get; }

    /// <summary> Defines call order. Higher priority callbacks are executed first. Defaults to 0.</summary>
    /// <remarks>Use the fluent API to set the priority.</remarks>
    public int Priority { get; private set; }

    /// <summary> The number of times the callback can be triggered before being removed automatically.</summary>
    /// <remarks>Defaults to null, meaning unlimited invocations.</remarks>
    public int? RemainingInvocations { get; private set; }

    /// <summary> Whether the callback has expired (either due to call limit or null delegate) and should be removed.</summary>
    /// <remarks>Calling an expired callback has no effect.</remarks>
    public bool IsExpired => RemainingInvocations <= 0 || _wrappedDelegate == null;

    private readonly Action<object?> _wrappedDelegate; // Required internally to allow for typed _and_ void actions.

    /// <summary> Creates a new CallbackEntry from a user-supplied callback that takes no arguments. </summary>
    public static ListenerEntry CreateFrom(Action action) => new(action, _ => action());

    /// <summary> Creates a new CallbackEntry from a user-supplied callback that takes one argument. </summary>
    public static ListenerEntry CreateFrom<T>(Action<T> action) => new(action, o => action((T)o!));

    // Private ctor to hide the messier Action<object?> and Delegate types from the public API.
    private ListenerEntry(Delegate original, Action<object?> wrappedInvoke) {
        OriginalDelegate = original;
        _wrappedDelegate = wrappedInvoke;
    }

    /// <summary> Calls the wrapped delegate with the provided payload. </summary>
    /// <remarks>Does nothing if the callback has expired.</remarks>
    public void Invoke(object? payload = null) {
        if (IsExpired) return; // Expired callbacks don't fire.

        // Call the wrapped delegate with the payload.
        // Action<object?> is used to allow for both typed and void callbacks.
        // The recipient argument-less Action will not receive a "null" payload. I.e. it will behave as _wrappedDelegate().
        // The Action<T> will receive the payload as expected.
        _wrappedDelegate(payload);
        if (RemainingInvocations is int n) RemainingInvocations = n - 1;
    }

    // Simple overload for clearer void callbacks.
    public void Invoke() => Invoke(null);

    // -------------------------------------------------------
    // Fluent modifiers
    // -------------------------------------------------------

    /// <summary> Sets the priority of the callback. Higher priority callbacks are executed first.</summary>
    public ListenerEntry WithPriority(int p = 0) { Priority = p; return this; }

    /// <summary> Sets the ListenerEntry to expire after a certain number of calls.</summary>
    public ListenerEntry CallLimit(int? n = null) { RemainingInvocations = n; return this; }

    /// <summary> Sets the ListenerEntry to expire after one call.</summary>
    public ListenerEntry Once() => CallLimit(1);
}
