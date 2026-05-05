# Battle System — Controllers & HUD

See [global.md](global.md) for the epic summary.
Taming conditions panel tickets: [CaptureBacklog.md](../CaptureBacklog.md) TAM-UI-*.

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 145 | 2 | **[BH-01] `BattleResultScreen` MonoBehaviour — show/hide on `EndPhase`** | Panel activated by `EndPhaseController` after outcome is determined. Receives `BattleOutcome` and the list of surviving player units. Must block all other input while open. |
| 145 | 2 | **[BH-02] Victory content — surviving creatures + "Continue" button** | Shows each surviving creature's name and portrait. "Continue" button calls `EventCenter.EmitBattleEnded` and returns to exploration. |
| 145 | 1 | **[BH-03] Defeat content — "Return to heal point" button** | Minimal defeat panel: defeat message + button that triggers the respawn flow (teleport player to last heal point). |
| 130 | 3 | **[BH-04] `FeatProgressionSummaryView` — list progressed / completed nodes** | Shown in the battle result screen after `FeatProgressionService.ApplyProgress` runs. Lists each creature with any node that advanced or completed during the fight: node name, before/after state. |
| 130 | 2 | **[BH-05] Wire feat summary to `FeatProgressionService.ApplyProgress` result** | `ApplyProgress` must return (or emit) a list of `(CreatureUnit, FeatNode, ProgressState)` tuples. `BH-04` reads this list; this ticket covers the data contract between the service and the view. |
| 110 | 3 | **[BH-06] `CreatureInfoWindow` MonoBehaviour — spawn, anchor, drag, close** | Floating non-modal panel. Spawned once and rebound. Draggable via `IDragHandler`. Close button sets inactive. Anchors near the clicked card but auto-corrects if off-screen. |
| 110 | 1 | **[BH-07] Subscribe to `CreatureCardView.RightClicked` in `PlayerTurnPhaseController`** | For each card in both `CreatureTeamView` instances, subscribe `RightClicked` to `CreatureInfoWindowController.Open(BattleUnit)`. Unsubscribe on `PlayerTurnPhase.Exit`. |
| 108 | 2 | **[BH-08] Creature info window — stats section** | Name, portrait, HP / AP / MP bars (`ObservableResource` bound), attribute grid (Attack, Magic, Armor, Resistance, Recovery, etc.). Closes automatically if the bound unit leaves the board. |
| 107 | 2 | **[BH-09] Creature info window — ability list section** | One row per ability in the unit's pool: icon, name, AP/MP cost, range label. Hovering a row shows the ability detail card (reuse `AbilityShortcutView` detail pattern). |
| 106 | 1 | **[BH-10] Creature info window — status / passive list section** | One row per active status and passive on the bound unit: icon, name, remaining duration. Reuses passive/status detail card from `Passive_UI_Element.png` wireframe. |
| 104 | — | **Creature info window — taming conditions section** | See [CaptureBacklog.md](../CaptureBacklog.md) TAM-UI-01 to TAM-UI-07. |
| 85 | 2 | **[BH-11] `FloatingTextSpawner` — spawn, animate, fade** | Instantiates a `TextMeshPro` prefab at a world-space position (above the hit unit). Animates upward over ~0.8 s then fades out. Pooled to avoid per-hit allocations. |
| 85 | 1 | **[BH-12] Subscribe `FloatingTextSpawner` to hit / heal resolution events** | `BattleActionResolver` already emits damage and heal values. Subscribe `FloatingTextSpawner` to these via `EventCenter` and convert to screen position via main camera. |
| 80 | 2 | **[BH-13] `WorldSpaceHpBar` MonoBehaviour — world-space canvas above unit model** | Small `Canvas` (World Space) with a `ProgressBarView` child. Positioned above the unit's `Transform`. Follows the model each frame via `LateUpdate`. |
| 80 | 1 | **[BH-14] Bind `WorldSpaceHpBar` to `BattleUnit.HP` observable** | On `Bind(BattleUnit)`, subscribe to `BattleUnit.BattleAttributes.HitPoints.Changed` and update the progress bar. Unsubscribe on unbind or unit defeat. |
| 70 | 1 | **[BH-15] Emit view-layer defeat event from `BattleContext.DefeatUnit`** | After marking the unit defeated, call `EventCenter.EmitUnitDefeated(BattleUnit)`. No view logic in the context — this is the signal only. |
| 70 | 1 | **[BH-16] Subscribe `BattleUnitView` to `UnitDefeated` event to trigger Death animation** | `BattleUnitView` subscribes to `EventCenter.UnitDefeated`. If the event matches the bound unit, trigger the `Death` animation recipe via the unit's `RecipeAnimator`. |
