# Events README

## Purpose
This folder contains the lightweight event system used to communicate between core systems
without direct references.

Core flow:
1. Define an event type that inherits from `GenericEvent`.
2. Subscribe to it with `Bus.Subscribe<T>()`.
3. Emit it with `Bus.Emit(new T(...))`.

## Event Bus
- `Bus` is a static, in-process event bus.
- Routing is based on the exact event type `T` (no inheritance dispatch).
- Dispatch is synchronous on the caller thread.
- The bus is not thread-safe.

## Event Types
Current events:
- `BattleSceneDataRequest`: request to load the battle scene with a board and enemy team.
- `ExplorationSceneDataRequest`: request to provide exploration scene data.
- `EncounterTriggerEvent`: emitted when an encounter starts, includes enemy team and battle board.
- `PlayerChunkMotion`: emitted when the player enters a new chunk.
- `PlayerMotion`: emitted on player movement, includes world + cell positions.

## Usage (example)
```csharp
// Subscribe
Bus.Subscribe<PlayerMotion>(OnPlayerMotion);

// Emit
Bus.Emit(new PlayerMotion(worldPos, cellPos));

// Unsubscribe
Bus.Unsubscribe<PlayerMotion>(OnPlayerMotion);
```

## Rules and Conventions
- Events are plain data carriers (no logic).
- Avoid long-running work in handlers (dispatch is synchronous).
- If you need polymorphic dispatch, emit multiple events or add a separate abstraction.

## Creating a New Event
Procedure:
1. Create a new class in `Assets/Scripts/Core/Events/` that inherits from `GenericEvent`.
2. Add XML documentation (summary + remarks) describing when it is emitted and what it carries.
3. Add required data as read-only properties and set them in the constructor.
4. Emit it using `Bus.Emit(new YourEvent(...))`.
5. Subscribe with `Bus.Subscribe<YourEvent>(Handler)` and unsubscribe on disable/cleanup.

Minimal example:
```csharp
namespace Erelia.Core.Event
{
    /// <summary>Event emitted when X happens.</summary>
    public sealed class XHappened : GenericEvent
    {
        public int Value { get; }
        public XHappened(int value) => Value = value;
    }
}
```
