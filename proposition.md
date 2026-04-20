# Battle Phase System — Architecture Proposition

## Context

The battle loop needs to orchestrate a sequence of distinct phases (placement, player turn, enemy turn, resolution, etc.) while keeping each phase's logic isolated and the transition rules centralized. The V1 archive had a working skeleton but mixed input handling into the orchestrator and leaked decision logic into the wrong layer.

---

## Core Principles

1. **Each phase is a self-contained state** — it knows how to enter, tick, and exit itself.  
2. **The orchestrator is a dumb state machine** — it holds the current phase and drives Enter/Tick/Exit. It never decides *which* phase comes next.  
3. **The coordinator owns battle logic** — it decides phase transitions based on game state. It is the only place that reads `BattleContext` to make decisions.  
4. **Input is handled per-phase** — `PlayerTurnPhase` binds/unbinds `BattlePlayerController` in its own Enter/Exit. The orchestrator never touches input.

---

## Diagrams

| File | Content |
|------|---------|
| [diagrams/proposition/01_classes.puml](diagrams/proposition/01_classes.puml) | Full class diagram — phases, orchestrator, coordinator, actions |
| [diagrams/proposition/02_sequence_full_battle.puml](diagrams/proposition/02_sequence_full_battle.puml) | End-to-end sequence: placement → idle → turn → resolve → result |
| [diagrams/proposition/03_sequence_transition.puml](diagrams/proposition/03_sequence_transition.puml) | Zoom-in on a single orchestrator phase transition |
| [diagrams/proposition/04_state_machine.puml](diagrams/proposition/04_state_machine.puml) | State machine view of all phases and their transition triggers |

---

## Key Design Decisions

### Why a separate Coordinator?

The orchestrator only knows "current phase" and how to swap phases. The coordinator knows *battle rules* — who goes next, what constitutes victory, whether a unit is player- or enemy-side. Keeping them separate means you can unit-test the state machine without a full `BattleContext`, and you can change transition rules without touching the phase Enter/Exit machinery.

### Why do phases emit events instead of returning the next phase?

If a phase returned the next `BattlePhaseId` directly it would need to know about victory conditions, whose turn it is, etc. Events keep phases ignorant of what follows them. `PlayerTurnPhase` fires `OnActionChosen` — it has no idea whether that triggers a resolve phase or an animation phase in the future.

### Where does BattlePlayerController live?

`PlayerTurnPhase` holds a reference to it and calls `Bind()`/`Unbind()` in Enter/Exit. The controller itself stays a MonoBehaviour on the scene so it can receive Unity input events. The phase simply activates and deactivates it.

### Phase instantiation

The coordinator creates all phase instances up front (or lazily on first transition) and passes them the dependencies they need (`BattleContext`, `BattlePlayerController`, etc.). The orchestrator receives pre-built phase objects — it never constructs anything.

### BattleAction ownership

Before transitioning to `ResolveActionPhase`, the coordinator sets the pending `BattleAction` on the phase (via a property or constructor argument). The phase executes it and fires `OnResolved`. This keeps execution isolated in one place regardless of who chose the action (player or AI).
