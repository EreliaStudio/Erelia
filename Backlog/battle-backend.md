# Battle System — Backend

See [global.md](global.md) for the epic summary.

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 155 | 2 | **[BB-03] Make a list of all AICondition subclass** | Need a full list of all concrete `AICondition`. Creating a proposition in format of a file in .md. |
| 155 | 2 | **[BB-04] Implement all AICondition subclass** | Compose all the `AICondition` and TUs |
| 155 | 3 | **[BB-07] Wire `EnemyTurnPhaseController` to evaluate `AIBehaviour` rule list** | Iterate `AIBehaviour.Rules` top-down. For each `AIRule`, evaluate all `AICondition`s (AND logic); on first full match, execute the `AIDecision` to build and submit a `BattleAction`. Fall through to `EndTurnAction` if no rule matches. |
| 125 | 1 | **[BB-13] Define `IPlacementStrategy` interface** | Single method: `PlaceUnits(BattleContext, BattleSide, IReadOnlyList<BattleUnit>)`. All placement strategies implement this. |
| 125 | 1 | **[BB-14] Implement `RandomPlacementStrategy`** | Extracts the existing `TryAutoPlaceUnitsRandomly` logic into a class implementing `IPlacementStrategy`. No behavior change — just refactored behind the interface. |
| 125 | 2 | **[BB-15] Implement `FixedPlacementStrategy`** | Places units at designer-specified cell offsets relative to the side's deployment zone origin. Per-encounter authoring via a list of `Vector3Int` offsets on the encounter data. |
| 125 | 2 | **[BB-16] Implement `ByLinePlacementStrategy`** | Fills cells row by row from the side's front line inward. Respects the unit count and skips occupied cells. |
| 125 | 1 | **[BB-17] Wire strategy selection to encounter data** | `EncounterData` (or `EncounterTeam`) gains a `PlacementStrategyType` enum field. `SetupPhaseController` reads it and instantiates the correct `IPlacementStrategy`. |
| 115 | 2 | **[BB-18] Emit feat events from `BattleStatusRules` hook points** | After each hook fires (TurnStart, TurnEnd, TakeDamage, etc.), call `affectedUnit.RecordFeatEvent(...)` with a typed status-hook event struct. Covers "took damage while shielded", "applied stun", etc. |
| 115 | 2 | **[BB-19] Add status-hook event types to matching `FeatRequirement` subclasses** | New `FeatRequirement` subclasses: `StatusAppliedRequirement`, `DamageWhileShieldedRequirement`. Evaluate against the event structs from BB-18. |
