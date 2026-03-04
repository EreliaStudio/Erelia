# Battle.Core README

## Purpose
Core contains the battle phase state machine and the registry that resolves phase instances.
`BattleManager` owns the active phase and applies transitions safely.

## Contents
- `BattleManager`: drives the current phase, handles transitions, and ticks the flow.
- `BattlePhase`: base class for phases with `Enter`, `Exit`, and `Tick`.
- `BattlePhaseId`: enum listing all phase identifiers.
- `BattlePhaseRegistry`: serializable mapping from ids to phase instances.

## Adding Or Extending
1. Add a new id to `BattlePhaseId`.
2. Create a new phase in `Assets/Scripts/Battle/Phases`.
3. Register the new phase in `BattlePhaseRegistry`.
4. Request the new phase from existing phases or `BattleManager` as needed.
