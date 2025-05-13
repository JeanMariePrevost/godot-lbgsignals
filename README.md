# LBGSignals – Lightweight, type-safe event signaling

[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE) [![Godot 4.2+](https://img.shields.io/badge/Godot-4.2%2B-blue.svg)](https://godotengine.org/) [![.NET 8](https://img.shields.io/badge/.NET-8-blueviolet.svg)](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8/overview)

**LBGSignals** is a lightweight C# utility that provides a simple, type-safe way to define and emit signals without relying on string names or reflection. It takes direct inspiration from Robert Penner's [AS3 Signals](https://github.com/robertpenner/as3-signals)

It aims to provide a middle ground between C# events and Godot Signals while reducing boilerplate and providing a type-safe, uniform and intuitive API.

Here's a side-by-side comparision of a hypothetical player collecting 15 gold pieces:

```csharp
// --- Standard C# Events ---
public event EventHandler<(string, int)>? ItemCollected;    // Declaration with tuple payload
ItemCollected += OnItemCollected;                           // Subscription
ItemCollected?.Invoke(this, ("Gold", 15));                  // Emission with null check
ItemCollected -= OnItemCollected;                           // Unsubscription

// --- Godot Signals (in C#) ---
[Signal] public delegate void ItemCollectedHandler(string item, int amount); // Declaration with attribute and delegate
sender.Connect("ItemCollected", Callable.From(OnItemCollected));             // Subscription using signal name
EmitSignal("ItemCollected", "Gold", 15);                                     // Emission using signal name and arguments
sender.Disconnect("ItemCollected", Callable.From(OnItemCollected));         // Unsubscription using signal name

// --- LBGSignals ---
public LBGSignal<string, int> ItemCollected = new();        // Declaration with typed arguments
ItemCollected.Add(OnItemCollected);                         // Subscription
ItemCollected.Emit("Gold", 15);                             // Emission with arguments
ItemCollected.Remove(OnItemCollected);                      // Unsubscription
```

---

## Features

- Simple drop-in usage — just add the contents of `.src/` to your project under ".LBG/GodotTools/Signals/"
- Simplified collection-like API (Add, Remove, Emit)
- QoL features such as subscriber priority or limited callbacks (e.g. register for a _single_ emit)
- Supports generic arity (`Signal`, `Signal<T>`, `Signal<T1, T2>` and `Signal<T1, T2, T3>`)
- Fluent-style API (e.g. `MySignal.Add(OnSignalEmitted).WithPriority(2).Once()`)

---

## Quick Start

With the files somewhere in your project, you can simply do:

```csharp
using LBG.GodotTools.Signals;

[...]

// Declare a new signal for <string, int> payloads
public LBGSignal<string, int> MyStringSignal = new(); 

// Subscribe with different options
MyStringSignal.Add(OnBasic);                               // Regular subscriber
MyStringSignal.Add(OnHighPriority).WithPriority(5);        // Higher priority — called earlier
MyStringSignal.Add(OnOnce).Once();                         // Auto-unsubscribed after first emit
MyStringSignal.Add(OnLimited).WithPriority(-2).CallLimit(10); // Lower priority, max 10 calls

// Emit the signal, e.g., 12 points of damage
MyStringSignal.Emit("damage", 42);

// Unsubscribe manually
MyStringSignal.Remove(OnBasic);

// Clear all subscribers
MyStringSignal.RemoveAll();

// Example handler method (matches the signal's <string, int> signature)
public void OnBasic(string messageType, int value){
    Console.WriteLine($"OnBasic received {messageType} with value {value}");
}

```

---

## Installation

Copy or clone the contents of `.src/` into your project under `.LBG/GodotTools/Signals/`.

Then, in files where you use signals, add:

```csharp
using LBG.GodotTools.Signals;
```

No extra steps required. This is a source-level dependency.

Requires .NET 8+ with C# 12 support (compatible with Godot 4.2 and later)

---

## Notes and Considerations

- Subscribers are identified by exact delegate instance. Different lambdas will not be treated as the same subscription.
  - For example, even if two lambdas point to the same method, you cannot add one and remove the other.
- Current implementation is not thread-safe.
- Modifying the subscriber list during an ongoing `Emit()` call is not safe.
- More performant than Godot's signal system, but less integrated with the engine.
- Slightly less performant than raw C# events.
- Signals do not include `sender` context or other metadata.

---

## Planned Features

- Safe emit-time listener modifications
- Weak reference subscriptions
- Re-entrancy safeguards (e.g. blocking nested Emit calls)
- Awaitable signals (`await MySignal`)
- Watchdog signals (automatic emit on value change)
- Error handling hooks (e.g. global or per-signal error callbacks)

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## Author

Jean-Marie Prévost

https://github.com/JeanMariePrevost