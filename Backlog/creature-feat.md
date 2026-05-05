# Creature & Feat System

See [global.md](global.md) for the epic summary.

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 200 | 1 | **[CF-01] Author first `CreatureSpecies` asset — base stats** | Create the SO asset with all `CreatureAttributes` filled in (HP, AP, MP, Attack, Magic, Armor, Resistance, Recovery, etc.). Stats should make the creature viable in a short test battle. |
| 200 | 2 | **[CF-02] Author 2–3 default abilities for the first species** | At minimum: one physical damage ability, one magical damage ability, one utility ability (heal, buff, or debuff). Wire them to `CreatureSpecies.DefaultAbilities`. Depends on CO-04 to CO-09. |
| 200 | 2 | **[CF-03] Author a minimal `FeatBoard` for the first species** | 5–8 nodes: root (auto-complete), 2–3 stat-bonus nodes, 1 ability-unlock node, 1 evolution node. Enough to validate the feat progression pipeline end-to-end. |
| 135 | 3 | **[CF-04] `FeatBoardRuntimeView` — canvas panel with node graph layout** | Runtime in-game equivalent of `FeatBoardEditorWindow`. Renders `FeatNode` positions as UI elements on a scrollable canvas. Node positions come from the same layout data used by the editor window. |
| 135 | 2 | **[CF-05] Node state rendering — locked / unlocked / completed** | Each node button has three visual states: locked (grey, no interaction), unlocked (highlighted, requirements shown on hover), completed (filled icon). Driven by `FeatBoardProgress` on the bound `CreatureUnit`. |
| 135 | 2 | **[CF-06] Node detail popup on click — requirements and rewards** | Clicking an unlocked or completed node opens a small popup listing: all `FeatRequirement` descriptions with current progress, and all `FeatReward` descriptions. Popup is closable and non-modal. |
| 135 | 1 | **[CF-07] Open `FeatBoardRuntimeView` from exploration HUD** | Wire a button (or contextual icon) in the exploration HUD to open the feat board for the selected creature. The view is bound to the chosen `CreatureUnit` from `PlayerData.Team`. |
| 55 | 3 | **[CF-08] `FeatBoardRespecService` — reset non-root nodes, refund rewards** | Traverses all completed non-root nodes on a `FeatBoard`, calls `FeatReward.Revert` on each, and resets `FeatBoardProgress` to root-only state. Must recompute attributes after respec via `FeatProgressionService`. |
| 55 | 1 | **[CF-09] Respec confirmation dialog** | Simple modal dialog: "This will undo all feat progress for [name]. Continue?" with confirm / cancel. Shown before calling `FeatBoardRespecService`. Prevent accidental respec. |
