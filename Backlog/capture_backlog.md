# Taming System — Backlog

## Overview

Wild creatures are tamed by fulfilling a species-specific list of **Taming Conditions** during the fight.
There is no capture action, no HP threshold, and no success roll.
When all conditions are met the creature is **impressed**: it leaves the board on its own and joins the player after the battle ends.

**Core backend is complete.** `TamingProfile`, `TamingProgress`, `TamingRules`, `TamingProgressService`, and `WildBattleUnit` are implemented and tested. Conditions reuse `FeatRequirement` subclasses. Impressed units are removed from the board and converted to `CreatureUnit` on victory via `TamingProgressService.AwardWonBattleTamingRewards`.

## Backlog Fields

### Priority
Same scale as the main backlog: 200 = prototype-critical, descending from there.

### Pts
Agile story-point estimate. Max 3 per ticket by convention.

---

## Persistence & Events

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 160 | 1 | **[TAM-29] Persist tamed creatures to `GameSaveData` via `EndPhase`** | After all `TryAddCreature` calls, write the updated `PlayerData` (team + PC box) back to `GameSaveData`. Trigger or call the save pipeline from `EndPhaseController`. Depends on TI-06 (world seed / save wiring). |
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
| 165 | 2 | **[TAM-34] Expose per-wild-unit taming progress as observable from `BattleContext`** | `BattleContext` should surface taming progress in a UI-readable form: for each wild unit, a list of `(conditionLabel, currentValue, targetValue)` tuples. This is the data the UI reads to show taming progress bars or icons. |
| 160 | 1 | **[TAM-35] Broadcast `TamingProgressChanged` event when any condition advances** | `TamingTracker` should raise a lightweight event (or update an `ObservableValue`) whenever any condition's progress changes. The HUD subscribes to update taming-condition indicators without polling. |
| 155 | 1 | **[TAM-36] Broadcast `UnitImpressed` event for board-removal visual hook** | When `ImpressUnit` is called, emit an `EventCenter` event carrying the `BattleUnit`. The view layer (animation, VFX) subscribes to play the "creature leaves" animation before the unit disappears from the board. |
| 145 | 1 | **[TAM-37] Broadcast end-of-battle taming summary for result screen** | In `EndPhaseController`, after all `TryAddCreature` calls, emit a summary event listing each tamed `CreatureUnit` and whether it went to team or PC. The battle result screen subscribes to display the taming section. |

---

## Edge Cases & Guards

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 160 | 1 | **[TAM-43b] Forfeit all impressed creatures on player defeat** | In `EndPhaseController`, check `BattleOutcome` before processing `ImpressedUnits`. If the outcome is `Defeat`, skip tamed-creature collection entirely — do not call `TryAddCreature` for any impressed unit. No partial reward is granted. |
| 150 | 2 | **[TAM-41] Handle PC box full gracefully** | If `TryAddCreature` returns `StorageFull`, do not silently discard the creature. Log an explicit warning during prototype. Define and document the intended player-facing resolution (e.g. the creature is released, the player is prompted) even if the UI for it is deferred. |

---

## Playtest Scenario

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 135 | 2 | **[TAM-46] Playtest scenario: author one `TamingProfile` on an existing species** | Create a minimal `TamingProfile` asset for one `CreatureSpecies` with two conditions (one simple, one positional). Wire it up end-to-end in a test scene to validate the full pipeline: tracking → impressed → end-phase collection → team/PC storage. |

---

## Editor Tooling — CreatureSpecies Inspector

These tickets extend `CreatureSpeciesEditor` (the existing `[CustomEditor(typeof(CreatureSpecies))]` in `Assets/Scripts/Creatures/Editor/CreatureSpeciesEditor.cs`) to let designers author `TamingProfile` and its condition list directly in the Inspector.

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 195 | 1 | **[TAM-ED-01] Add `TamingProfile` serialized field to `CreatureSpecies`** | Prerequisite for all editor work. The field must be `[SerializeField]`. A null value means the species is untameable — the inspector must make this state obvious (e.g. a "No Taming Profile" label). |
| 190 | 2 | **[TAM-ED-02] Draw `TamingProfile` slot in `CreatureSpeciesEditor`** | After the existing "Open Feat Board" button, add a new section header "Taming Profile". Draw an `ObjectField` for the `TamingProfile` SO reference. If the field is null, show a help box: *"This species cannot be tamed. Assign a Taming Profile to enable taming."* |
| 185 | 2 | **[TAM-ED-03] Create `TamingProfileEditor` custom editor for the `TamingProfile` SO** | `[CustomEditor(typeof(TamingProfile))]`. Draws the list of condition entries using `[SerializeReference]` inline. Each entry shows: condition type (dropdown via `ManagedReferenceTypePicker`), its fields, and remove button. Follows the same pattern as `AbilityEffectDrawer`. |
| 180 | 2 | **[TAM-ED-04] Condition type picker using `ManagedReferenceTypePicker`** | Inside `TamingProfileEditor`, the "Add Condition" button opens a dropdown populated by `ManagedReferenceTypePicker.GetConcreteTypes(typeof(FeatRequirement))`. Uses `NicifyTypeName` for readable labels. |
| 170 | 2 | **[TAM-ED-05] Inline property drawer per condition subclass** | Each concrete condition subclass needs its fields exposed. Default `PropertyField` is acceptable for a first pass. Add `[CustomPropertyDrawer]` only if a subclass has fields that benefit from a compact single-line layout. |
| 160 | 1 | **[TAM-ED-06] Foldout header for the Taming section in `CreatureSpeciesEditor`** | Wrap the Taming Profile section in a `EditorGUILayout.BeginFoldoutHeaderGroup` matching the existing "Attributes" foldout style. Persist the expanded state via a private bool field on the editor class. |
| 150 | 1 | **[TAM-ED-07] "Create new Taming Profile" shortcut button in `CreatureSpeciesEditor`** | When the `TamingProfile` field is null, show a small "Create" button. Clicking it creates the SO asset next to the `CreatureSpecies` asset using `AssetDatabase` and assigns it to the field. |

---

## Creature Info Window — Taming Conditions Panel

Assumes the creature info window infrastructure (open/close/drag, stats panel) is complete.

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 104 | 2 | **[TAM-UI-01] `TamingConditionsPanel` MonoBehaviour** | Self-contained UI component that takes a `TamingProgress` reference and renders one row per condition. Rows show: condition label, a `ProgressBarView` (current / target), and a "completed" checkmark icon. Subscribes to `TamingProgressChanged` (TAM-35) to update live. |
| 102 | 1 | **[TAM-UI-02] Show/hide the taming panel based on unit type** | The creature info window controller checks `BattleContext.IsWildBattle` and whether the bound `BattleUnit`'s species has a non-null `TamingProfile`. If both are true, show `TamingConditionsPanel`; otherwise hide it entirely. |
| 100 | 1 | **[TAM-UI-03] Bind `TamingProgress` to the panel on window open** | When the window opens on a wild unit, retrieve the unit's `TamingProgress` from `BattleContext` and pass it to `TamingConditionsPanel.Bind(TamingProgress)`. If null (untameable or already impressed/defeated), hide the panel. |
| 98 | 1 | **[TAM-UI-04] Live progress update via `TamingProgressChanged` event** | `TamingConditionsPanel` subscribes to `TamingProgressChanged` (TAM-35) while the window is open. On receipt, re-evaluate each condition row without rebuilding the full list. Unsubscribe when the window closes. |
| 92 | 1 | **[TAM-UI-05] "Impressed" state — lock the panel when taming completes** | When the bound unit becomes impressed mid-fight (`UnitImpressed` event, TAM-36), update all rows to "completed" and display a banner: *"Impressed — will join after victory"*. |
| 85 | 2 | **[TAM-UI-06] `TamingConditionRow` sub-component** | Reusable child component for a single condition row. Fields: label `TextMeshProUGUI`, `ProgressBarView`, completed icon `Image`. The panel instantiates one row per condition from a prefab. |
| 75 | 1 | **[TAM-UI-07] "Unit defeated before taming" state** | If the bound unit is defeated while the window is open, mark all incomplete rows with a "failed" style and show *"Escaped — taming failed"*. Triggered by subscribing to `EventCenter` defeat events. |
