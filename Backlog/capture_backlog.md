# Taming System — Backend Backlog

## Overview

Wild creatures are tamed by fulfilling a species-specific list of **Taming Conditions** during the fight.
There is no capture action, no HP threshold, and no success roll.
When all conditions are met the creature is **impressed**: it leaves the board on its own and joins the player after the battle ends.

## Backlog Fields

### Priority
Same scale as the main backlog: 200 = prototype-critical, descending from there.

### Pts
Agile story-point estimate. Max 3 per ticket by convention.

---

## Domain & Data Model

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 200 | 2 | **[TAM-01] Create `TamingCondition` abstract base class** | Mirrors the existing `FeatRequirement` base. Needs: `IsCompleted(TamingProgress) → bool` and `GetProgress(TamingProgress) → float` (for UI). Place in a new `Taming/` folder under `Assets/Scripts/`. |
| 200 | 2 | **[TAM-02] Create `TamingProfile` ScriptableObject** | Holds `List<TamingCondition>` (serialized via `[SerializeReference]` like FeatRequirement). Assigned as a field on `CreatureSpecies`. A null or empty profile means the species cannot be tamed. |
| 195 | 1 | **[TAM-03] Add `TamingProfile` field to `CreatureSpecies`** | Single optional field. The field being null is a valid "untameable" state (trainers, bosses, etc.). No other changes to `CreatureSpecies`. |
| 185 | 2 | **[TAM-04] Create `TamingProgress` runtime class** | Per–wild-unit tracking object. Stores the source `TamingProfile` and a progress value or flag per condition. Instantiated by `TamingTracker` when the battle starts for each wild unit. Immutable profile, mutable progress. |
| 180 | 1 | **[TAM-05] Add `IsWildBattle` flag to `BattleContext`** | Boolean set during `SetupPhase`. All taming logic gates on this flag. Trainer and gym battles must leave it false. |
| 175 | 1 | **[TAM-06] Author `TamedResult` enum** | Values: `Tamed`, `NotYetTamed`, `ConditionFailed` (unit was defeated before taming). Used by `TamingTracker` to report per-unit state to `EndPhaseController`. |

---

## Condition Subclasses

Each subclass tracks one category of in-battle events. They follow the same `[SerializeReference]` polymorphism pattern already used by `FeatRequirement`.

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 175 | 2 | **[TAM-07] `DamageDealtCondition`** | Fulfilled when the player's team deals at least X damage of a given type (physical / magical / either) in a single hit or cumulatively, configurable via a `scope` field (PerHit / PerFight). |
| 170 | 2 | **[TAM-08] `AbilityUsedCondition`** | Fulfilled when a specific Ability (or any Ability of a given tag/type) is used at least N times during the fight by any player unit. |
| 165 | 2 | **[TAM-09] `AlliesAliveCondition`** | Fulfilled when the fight ends (or a trigger point is reached) with at least N player units still alive and not defeated. |
| 165 | 2 | **[TAM-10] `NoDamageReceivedCondition`** | Fulfilled when the player team avoids receiving a specific damage type for N consecutive turns. Requires tracking a rolling counter that resets on violation. |
| 160 | 2 | **[TAM-11] `PositionCondition`** | Fulfilled when a player unit occupies a cell within N tiles of the wild creature's position at any point during the fight. |
| 155 | 2 | **[TAM-12] `StatusAppliedCondition`** | Fulfilled when a specific status (or any status) is applied to any target (ally or enemy, configurable) at least N times during the fight. |
| 145 | 2 | **[TAM-13] `TurnsSurvivedCondition`** | Fulfilled when the fight reaches at least N total turns without the player losing a unit. |

---

## Tracking System

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 195 | 3 | **[TAM-14] Create `TamingTracker` service** | Instantiated once per wild battle. Holds a `Dictionary<BattleUnit, TamingProgress>` for every wild unit that has a non-null `TamingProfile`. Exposes `NotifyEvent(BattleFeatEvent)` (reuses the existing feat-event structs) and `GetState(BattleUnit) → TamedResult`. |
| 190 | 2 | **[TAM-15] Initialize `TamingTracker` in `SetupPhase`** | During `SetupPhaseController.Execute`, if `IsWildBattle` is true, instantiate `TamingTracker` and store it on `BattleContext`. Only wild-side units with a non-null `TamingProfile` get a `TamingProgress` entry. |
| 190 | 2 | **[TAM-16] Feed battle events into `TamingTracker` from `BattleActionResolver`** | After each `AbilityAction` or `MoveAction` resolves, the resolver already calls `BattleUnit.RecordFeatEvent`. Extend the same resolution path to also call `BattleContext.TamingTracker?.NotifyEvent(...)` with the same event struct. No new event type needed — reuse existing feat-event structs. |
| 185 | 2 | **[TAM-17] Feed status-application events into `TamingTracker`** | `BattleStatusRules.ApplyHook` already fires hook points. After any `TurnStart` / `TurnEnd` / `TakeDamage` hook that applies or removes a status, call `TamingTracker?.NotifyEvent(...)` with a status event struct. |
| 185 | 2 | **[TAM-18] Check taming completion after every event in `TamingTracker`** | After `NotifyEvent`, `TamingTracker` iterates the affected unit's `TamingProgress` and calls `IsCompleted` on each condition. If all conditions pass, mark the unit as `Tamed` and raise a `UnitTamed(BattleUnit)` callback or `EventCenter` event. |

---

## Impressed Unit — Board Removal

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 185 | 2 | **[TAM-19] Add `ImpressUnit(BattleUnit)` to `BattleContext`** | Parallel to `DefeatUnit`. Removes the unit from `BoardRuntimeRegistry`, marks it as impressed (new boolean flag on `BattleUnit`), and does **not** set `IsDefeated`. The unit is gone from the board but is tracked separately for end-phase collection. |
| 185 | 1 | **[TAM-20] Add `IsImpressed` flag to `BattleUnit`** | Boolean property. Mutually exclusive with `IsDefeated` — a unit cannot be both. Used by `BattleOutcomeRules` and `EndPhaseController` to distinguish tamed units from defeated ones. |
| 180 | 2 | **[TAM-21] Call `ImpressUnit` when `TamingTracker` signals taming complete** | Subscribe to the `UnitTamed` event from TAM-18 in `ResolutionPhase` or `IdlePhase`. On receipt, call `BattleContext.ImpressUnit(unit)` and trigger any board-removal visuals. The battle then continues normally. |
| 175 | 1 | **[TAM-22] Ensure impressed units are skipped by turn-bar advancement** | `BattleTurnRules.AdvanceTurnBars` must skip units where `IsImpressed` is true, the same way stunned units are skipped. Prevents an impressed unit from ever taking a turn after it leaves. |
| 170 | 1 | **[TAM-23] Ensure `BattleOutcomeRules` ignores impressed units when evaluating defeat** | The victory check "all enemies defeated" should treat impressed units as absent, not as alive. Verify that `BattleOutcomeRules.Evaluate` iterates only units that are neither defeated nor impressed. |

---

## Battle Outcome

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 175 | 1 | **[TAM-24] No new `BattleOutcome` variant required** | Taming does not change the win/loss outcome of the battle. The fight ends on standard `Victory` or `Defeat`. The list of impressed units is collected separately by `EndPhaseController`. This is a deliberate simplification — confirm and document. |
| 170 | 1 | **[TAM-25] Expose impressed-unit list from `BattleContext`** | Add `IReadOnlyList<BattleUnit> ImpressedUnits` property to `BattleContext`, populated by each `ImpressUnit` call. `EndPhaseController` reads this list to know which creatures to add to the player's roster. |

---

## Player Data & PC Box

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 180 | 2 | **[TAM-26] Add PC box storage to `PlayerData`** | `PlayerData` currently holds only the 6-slot active team. Add `List<CreatureUnit> PCBox`. Max size is a named constant (e.g. `MaxPCSlots = 240`). This is required before any tamed creature can be stored. |
| 175 | 2 | **[TAM-27] Implement `TryAddCreature` on `PlayerData`** | Inserts a `CreatureUnit` into the active team if a slot is free, otherwise into `PCBox`. Returns `AddedToTeam`, `SentToPC`, or `StorageFull`. `StorageFull` must be a non-silent failure — the caller must decide what to do (log, surface to player, etc.). |
| 170 | 2 | **[TAM-28] Build `CreatureUnit` from tamed `BattleUnit` in `EndPhaseController`** | Only runs when the battle outcome is `Victory`. For each `BattleUnit` in `ImpressedUnits`, construct a fresh `CreatureUnit` from the unit's `CreatureSpecies` (base stats, default abilities, blank feat board). Then call `PlayerData.TryAddCreature`. If the outcome is `Defeat`, skip this step entirely — impressed creatures are forfeit. The wild unit's in-battle HP or status state does not transfer. |
| 160 | 1 | **[TAM-29] Persist tamed creatures to `GameSaveData` via `EndPhase`** | After all `TryAddCreature` calls, write the updated `PlayerData` (team + PC box) back to `GameSaveData`. Trigger or call the save pipeline from `EndPhaseController`. |
| 155 | 1 | **[TAM-30] Emit `CreatureTamed` event on `EventCenter` for each tamed unit** | After `TryAddCreature` succeeds, emit `EventCenter.EmitCreatureTamed(CreatureUnit, AddResult)`. Allows HUD, audio, and future systems to react without coupling to `EndPhaseController`. |

---

## Feat Events for Taming Behavior

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 150 | 2 | **[TAM-31] Record taming-attempt feat event on the player's acting units** | When `TamingTracker.NotifyEvent` processes an event that advances a taming condition, also call `SourceUnit.RecordFeatEvent(...)` with a "contributed to taming" event struct. Lets feat nodes reward players who actively work toward taming. |
| 145 | 2 | **[TAM-32] Record taming-success feat event on all player units that contributed** | When a unit is impressed, iterate the events that completed the final conditions and record a "taming succeeded" event struct on the contributing player units. Allows feat requirements like "tame X creatures". |
| 140 | 2 | **[TAM-33] Add `TamingContributionRequirement` and `TamingSuccessRequirement` feat requirement subclasses** | Two new `FeatRequirement` subclasses evaluating the event structs from TAM-31/32. `TamingContributionRequirement` counts events that advanced any taming condition; `TamingSuccessRequirement` counts successful tames. |

---

## HUD Backend Hooks (Signal Layer Only)

> Full visual implementation belongs in a separate HUD backlog. These tickets cover the observable/event layer the UI will bind to.

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 165 | 2 | **[TAM-34] Expose per-wild-unit taming progress as observable from `BattleContext`** | `BattleContext` should surface `TamingTracker` progress in a UI-readable form: for each wild unit, a list of `(conditionLabel, currentValue, targetValue)` tuples. This is the data the UI reads to show taming progress bars or icons. |
| 160 | 1 | **[TAM-35] Broadcast `TamingProgressChanged` event when any condition advances** | `TamingTracker` should raise a lightweight event (or update an `ObservableValue`) whenever any condition's progress changes. The HUD subscribes to update taming-condition indicators without polling. |
| 155 | 1 | **[TAM-36] Broadcast `UnitImpressed` event for board-removal visual hook** | When `ImpressUnit` is called, emit an `EventCenter` event carrying the `BattleUnit`. The view layer (animation, VFX) subscribes to play the "creature leaves" animation before the unit disappears from the board. |
| 145 | 1 | **[TAM-37] Broadcast end-of-battle taming summary for result screen** | In `EndPhaseController`, after all `TryAddCreature` calls, emit a summary event listing each tamed `CreatureUnit` and whether it went to team or PC. The battle result screen subscribes to display the taming section. |

---

## Edge Cases & Guards

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 170 | 1 | **[TAM-38] Guard all taming logic behind `IsWildBattle`** | `TamingTracker` must not be instantiated for trainer or gym battles. Add an early-return guard in `SetupPhaseController` and a null-check wherever `BattleContext.TamingTracker` is accessed. |
| 165 | 1 | **[TAM-39] Unit defeated before taming completes — mark `ConditionFailed`** | When `BattleContext.DefeatUnit` is called on a wild unit that has a `TamingProgress` entry, `TamingTracker` must mark that entry as `ConditionFailed`. No further events should be processed for that unit. |
| 155 | 2 | **[TAM-40] Multiple wild units with overlapping conditions — no cross-contamination** | `TamingTracker` holds one `TamingProgress` per wild unit. Verify that a battle event that advances conditions for unit A does not erroneously advance conditions for unit B, even if both have identical condition types. Cover with a unit test. |
| 150 | 2 | **[TAM-41] Handle PC box full gracefully** | If `TryAddCreature` returns `StorageFull`, do not silently discard the creature. Log an explicit warning during prototype. Define and document the intended player-facing resolution (e.g. the creature is released, the player is prompted) even if the UI for it is deferred. |
| 160 | 1 | **[TAM-43b] Forfeit all impressed creatures on player defeat** | In `EndPhaseController`, check `BattleOutcome` before processing `ImpressedUnits`. If the outcome is `Defeat`, skip tamed-creature collection entirely — do not call `TryAddCreature` for any impressed unit. `ImpressedUnits` should be cleared or ignored. No partial reward is granted. |
| 140 | 1 | **[TAM-42] Taming progress does not persist between encounters** | `TamingTracker` is created fresh each battle and discarded at `EndPhase`. Confirm no `TamingProgress` state leaks into `GameSaveData` or `PlayerData`. A taming attempt that fails one encounter cannot be resumed in a later encounter against a different instance of the same species. |

---

## Infrastructure & Tests

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 165 | 2 | **[TAM-43] Set `IsWildBattle` in `SetupPhaseController` from encounter data** | Read the encounter type from the encounter data passed to `SetupPhase` and set `BattleContext.IsWildBattle` accordingly. Wild encounters → true; trainer / gym / scripted → false. |
| 155 | 3 | **[TAM-44] Unit-test `TamingTracker` core loop** | Tests: (1) all conditions fulfilled → unit becomes `Tamed`; (2) unit defeated mid-fight → `ConditionFailed`; (3) partial progress → `NotYetTamed`; (4) non-wild battle → tracker is null, no errors. Use NUnit EditMode assembly. |
| 145 | 2 | **[TAM-45] Unit-test each `TamingCondition` subclass in isolation** | For each condition from TAM-07 to TAM-13, write at minimum: one test where the threshold is met, one where it is not. Mock `TamingProgress` to isolate condition logic from the tracker. |
| 135 | 2 | **[TAM-46] Playtest scenario: author one `TamingProfile` on an existing species** | Create a minimal `TamingProfile` asset for one `CreatureSpecies` with two conditions (one simple, one positional). Wire it up end-to-end in a test scene to validate the full pipeline: tracking → impressed → end-phase collection → team/PC storage. |

---

---

## Editor Tooling — CreatureSpecies Inspector

These tickets extend `CreatureSpeciesEditor` (the existing `[CustomEditor(typeof(CreatureSpecies))]` in `Assets/Scripts/Creatures/Editor/CreatureSpeciesEditor.cs`) to let designers author `TamingProfile` and its `TamingCondition` list directly in the Inspector, following the same patterns already used for `FeatRequirement` and `AbilityEffect` drawers.

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 195 | 1 | **[TAM-ED-01] Add `TamingProfile` serialized field to `CreatureSpecies`** | Prerequisite for all editor work. The field must be `[SerializeField]` and typed as `TamingProfile` (the SO from TAM-02). A null value means the species is untameable — the inspector must make this state obvious (e.g. a "No Taming Profile" label). |
| 190 | 2 | **[TAM-ED-02] Draw `TamingProfile` slot in `CreatureSpeciesEditor`** | After the existing "Open Feat Board" button, add a new section header "Taming Profile". Draw an `ObjectField` for the `TamingProfile` SO reference. If the field is null, show a help box: *"This species cannot be tamed. Assign a Taming Profile to enable taming."* |
| 185 | 2 | **[TAM-ED-03] Create `TamingProfileEditor` custom editor for the `TamingProfile` SO** | `[CustomEditor(typeof(TamingProfile))]`. Draws the list of `TamingCondition` entries using `[SerializeReference]` inline. Each entry shows: condition type (dropdown via `ManagedReferenceTypePicker`), its fields, and remove button. Follows the same pattern as `AbilityEffectDrawer`. |
| 180 | 2 | **[TAM-ED-04] `TamingCondition` type picker using `ManagedReferenceTypePicker`** | Inside `TamingProfileEditor`, the "Add Condition" button opens a dropdown populated by `ManagedReferenceTypePicker.GetConcreteTypes(typeof(TamingCondition))`. Selecting a type calls `ManagedReferenceTypePicker.CreateInstance` and appends the new entry to the list. Uses `NicifyTypeName` with suffix trim `"Condition"` for readable labels. |
| 170 | 2 | **[TAM-ED-05] Inline property drawer per `TamingCondition` subclass** | Each concrete `TamingCondition` subclass (TAM-07 to TAM-13) needs its fields exposed. Default `PropertyField` is acceptable for a first pass. Add a `[CustomPropertyDrawer]` only if a subclass has fields that benefit from a compact single-line layout (e.g. `DamageDealtCondition` with type + threshold side-by-side). |
| 160 | 1 | **[TAM-ED-06] Foldout header for the Taming section in `CreatureSpeciesEditor`** | Wrap the Taming Profile section in a `EditorGUILayout.BeginFoldoutHeaderGroup` (matching the existing "Attributes" foldout style). Persist the expanded state via a private bool field on the editor class. Keeps the inspector uncluttered when taming is not the current focus. |
| 150 | 1 | **[TAM-ED-07] "Create new Taming Profile" shortcut button in `CreatureSpeciesEditor`** | When the `TamingProfile` field is null, show a small "Create" button next to the object field. Clicking it calls `ScriptableObject.CreateInstance<TamingProfile>()`, saves it as an asset next to the `CreatureSpecies` asset using `AssetDatabase`, and assigns it to the field. Mirrors the workflow used for other SO assets in the project. |
| 140 | 1 | **[TAM-ED-08] Read-only condition progress preview in editor (stretch)** | In Play Mode, `CreatureSpeciesEditor` could show the live `TamingProgress` state for that species if a matching wild unit exists in the current `BattleContext`. Low priority — useful for debugging condition authoring but not required for shipping the feature. |

---

## Creature Info Window — Taming Conditions Panel

These tickets cover the taming-specific section inside the right-click creature info window. The window infrastructure itself (open/close/drag, stats, abilities, statuses) is tracked in `backlog.md` under "Battle System — Controllers & HUD" at priorities 110–106. The tickets below assume that infrastructure exists and focus only on binding and displaying taming data.

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 104 | 2 | **[TAM-UI-01] `TamingConditionsPanel` MonoBehaviour** | Self-contained UI component that takes a `TamingProgress` reference and renders one row per `TamingCondition`. Rows show: condition label, a `ProgressBarView` (current / target), and a "completed" checkmark icon. Subscribes to the `TamingProgressChanged` event from TAM-35 to update live. |
| 102 | 1 | **[TAM-UI-02] Show/hide the taming panel based on unit type** | The creature info window controller checks `BattleContext.IsWildBattle` and whether the bound `BattleUnit`'s species has a non-null `TamingProfile`. If both are true, show `TamingConditionsPanel`; otherwise hide it entirely. The panel must not appear for player units or untameable wild units. |
| 100 | 1 | **[TAM-UI-03] Bind `TamingProgress` to the panel on window open** | When the window opens on a wild unit, retrieve the unit's `TamingProgress` from `BattleContext.TamingTracker` and pass it to `TamingConditionsPanel.Bind(TamingProgress)`. If the tracker returns null (unit is untameable or already impressed/defeated), hide the panel. |
| 98 | 1 | **[TAM-UI-04] Live progress update via `TamingProgressChanged` event** | `TamingConditionsPanel` subscribes to the `TamingProgressChanged` event (TAM-35) while the window is open. On receipt, re-evaluate each condition row and update the progress bar and checkmark without rebuilding the full list. Unsubscribe when the window closes or is rebound. |
| 92 | 1 | **[TAM-UI-05] "Impressed" state — lock the panel when taming completes** | When the bound unit becomes impressed mid-fight (TAM-36 `UnitImpressed` event), the panel should update all rows to "completed" and display a banner or label such as *"Impressed — will join after victory"*. The window may remain open but should be clearly read-only. |
| 85 | 2 | **[TAM-UI-06] `TamingConditionRow` sub-component** | Reusable child component for a single condition row. Fields: label `TextMeshProUGUI`, `ProgressBarView`, completed icon `Image`. The panel instantiates one row per condition from a prefab. Follows the same instantiate-from-prefab pattern used by `CreatureTeamView` / `CreatureCardView`. |
| 75 | 1 | **[TAM-UI-07] "Unit defeated before taming" state** | If the bound unit is defeated while the window is open, mark all incomplete condition rows with a "failed" style (e.g. greyed out, cross icon) and show a label *"Escaped — taming failed"*. Triggered by subscribing to `EventCenter` defeat events. |

---

## Suggested Implementation Order

1. TAM-05, TAM-43 — wild battle flag infrastructure (unblocks everything)
2. TAM-01, TAM-02, TAM-03, TAM-04, TAM-06 — data model
3. **TAM-ED-01** — add `TamingProfile` field to `CreatureSpecies` (required before any editor or runtime work)
4. TAM-07 to TAM-13 — condition subclasses (can be done in parallel)
5. **TAM-ED-02, TAM-ED-03, TAM-ED-04** — core inspector authoring UI
6. **TAM-ED-05, TAM-ED-06, TAM-ED-07** — inspector polish
7. TAM-14, TAM-15 — tracker creation and setup
8. TAM-16, TAM-17, TAM-18 — event feeding and completion detection
9. TAM-19, TAM-20, TAM-21, TAM-22, TAM-23 — board removal and turn-bar guards
10. TAM-24, TAM-25 — outcome wiring
11. TAM-26, TAM-27, TAM-28, TAM-29, TAM-30 — player data and PC box
12. TAM-34, TAM-35, TAM-36, TAM-37 — HUD signal layer *(TAM-35 required before TAM-UI-04)*
13. TAM-31, TAM-32, TAM-33 — feat events for taming
14. TAM-38 to TAM-43b — edge cases
15. *(creature info window infrastructure from `backlog.md` priority 110–106 must be complete before the next step)*
16. **TAM-UI-01, TAM-UI-02, TAM-UI-03** — taming panel component and binding
17. **TAM-UI-04, TAM-UI-05, TAM-UI-06, TAM-UI-07** — live updates, impressed/defeated states, row sub-component
18. TAM-44, TAM-45, TAM-46 — tests and playtest scenario
19. **TAM-ED-08** — play-mode debug preview (stretch)

---
