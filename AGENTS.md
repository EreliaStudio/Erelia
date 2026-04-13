# AGENTS.md

This document is the shared brief for humans and AI agents working on `Erelia`.
Use it to keep implementation aligned with the intended game, UI layouts, and terminology.

## Source Of Truth

- Core design intent: `GDD.md`
- Current UI wireframes: `main_menu.png`, `Exploration_mode_screen.png`, `BattleHUD.png`, `BattleUIElement.png`, `TeamUIElement.png`, `Creature_UI_Element.png`, `Action_UI_Element.png`, `Passive_UI_Element.png`, `Encounter_Table_UI.png`, `Team_Editor_Window.png`, `Team_Editor_WindowV2.png`
- Live V3 scaffold: `Assets/`, `Packages/`, `ProjectSettings/`, `README.md`
- Archived history: `archive/V1`, `archive/V2`

The live root is now a fresh `V3` baseline. Previous implementations are archived and should be treated as reference material, not as the active base.

If the GDD, code, and mockups disagree, prefer this order:

1. User clarification
2. `GDD.md`
3. Latest wireframe mockup
4. Existing code

## Project Identity

- Working title: `Erelia`
- Genre: single-player, offline creature-collection RPG
- Core blend:
  - Pokemon-like collection and gym progression
  - FF12 / PoE-style board-based creature progression
  - Tactical board battles with stamina-based turn frequency
  - Procedurally generated voxel overworld
- Primary win condition:
  - Defeat 8 gyms in randomized order
  - Defeat the Elite Four

## Prototype Scope

The current target is a prototype, not the full game.

Prototype runtime scope is limited to:

1. `Main Menu`
2. `Exploration Screen`
3. `Battle Screen`

Anything outside those three runtime surfaces is deferred until later unless the user explicitly reprioritizes it.

Prototype assumptions:

- V3 is intentionally sparse and should be rebuilt cleanly from the wireframes.
- The old implementation in `archive/V2` is reference-only.
- Exploration only needs enough HUD and interaction support to move in the world and trigger encounters.
- Battle only needs enough UI to inspect combat state, choose abilities, and end turns.
- Deeper management screens can be postponed even if progression systems already exist in code.
- Editor tooling may continue to carry some setup burden during the prototype phase.

## Terminology Mapping

Several names refer to the same concept depending on whether you are reading the GDD, code, or mockups.

- `Feat Board` in the GDD == `FeatBoard` in code
- `Quest Validation` in the GDD == node requirements / progression tracked on the feat board
- `Action UI` in mockups == `Ability` UI in code
- `Passive UI` in mockups == passive or status detail card
- `PA` in mockups == `AP` in code and GDD
- `PM` in mockups == `MP` in code and GDD

Do not invent a second progression system unless the user explicitly asks for one.

## Non-Negotiable Design Rules

- No creature XP levels as the main progression axis.
- Creature growth comes from battle-earned validation and feat board unlocks.
- Unlocks should depend on what the creature did in battle, not just whether the battle was won.
- Team size is 6 active creatures.
- Battles are tactical, grid-based, and turn-based through a stamina or turn-bar system.
- Lower stamina-turn duration means acting more often.
- AP and MP refill at the start of the creature's turn.
- Movement and abilities are separate resources.
- Most normal battles should be derived from the surrounding overworld when possible.
- Special battles can use handcrafted boards and scripted setups.
- The overworld is procedurally generated per run from a persistent seed.
- Gym order, encounter scaling, and progression gating are badge or unlock-tier driven, not level driven.
- Losing a battle should still preserve meaningful progression.

## Desired Player Experience

- Exploration should feel familiar and readable: roads, towns, gyms, routes, caves, bushes, trainers.
- Battles should feel information-rich but readable, with all critical data visible without opening deep submenus.
- Progression should feel earned through play patterns: range play, shielding, cleansing, positioning, healing, tanking, and similar behaviors.
- Creature building should support branching identity through feat board unlocks, passive choices, and form or evolution choices.
- Replayability should come from different world layouts, gym order, POI placement, and encounter availability.

## Visual And UX Direction

These points are grounded mostly in the current wireframes and should be treated as layout truth.

- UI is desktop-first, panel-based, and rectangular.
- The current wireframes are monochrome, low-fidelity layout specs, not final art direction.
- The prototype should leave most of the world visible; HUD elements should stay near screen edges.
- Favor strong information grouping over minimalism.
- Creature info, actions, and passives should reveal richer detail on hover when possible.
- Main menu layout is simple and readable:
  - title centered near the top
  - background image or background scene behind the menu
  - primary actions stacked on the right
- Battle HUD should keep both team columns visible while centralizing active-turn controls at the bottom.
- Exploration HUD layout is now defined by the wireframe:
  - top-left time or day-night widget
  - top-center current area or zone name
  - top-right contextual icon buttons
  - left-side vertical party list with six creature slots
  - gameplay view remains dominant in the center
- Team presentation should support six clearly visible creature slots.
- Ability presentation should show:
  - icon
  - name
  - AP and MP cost
  - range metadata
  - area-of-effect metadata
  - line-of-sight metadata
  - generated description text
- Final palette, typography, iconography style, and environmental mood are still under-specified.

## What Exists In The Repository Today

The repo already contains useful anchors. Do not assume it is empty.

- The repo root now contains a clean `V3` Unity scaffold.
- `Assets/Scripts/Bootstrap/PrototypeSceneNames.cs` defines the intended scene names.
- `Assets/Scripts/Bootstrap/PrototypeSceneLoader.cs` provides basic scene-load helpers for menu buttons and simple flow.
- `Assets/Scripts/UI/CreatureSlotView.cs` is the new minimal replacement direction for creature presentation.
- `Assets/Scripts/Exploration/ExplorationHudView.cs` and `Assets/Scripts/Battle/BattleHudView.cs` define a small, clean UI baseline for the prototype.
- `Assets/Scenes/README.md` lists the scene creation order for V3.
- `archive/V2` contains the previous live implementation, including the old runtime UI and scenes.
- `archive/V1` contains the older imported archive.
- Root-level PNGs are user-authored wireframes and should be treated as intentional design references.

## Working Rules For Future AI Contributors

- Preserve the no-leveling progression model.
- Preserve the six-slot team structure unless the user changes it.
- Preserve the eight-ability action bar expectation unless the user changes it.
- Do not replace the tactical board battle with real-time combat.
- Do not compress the UI into a mobile-first layout without approval.
- Build on V3, not on archived V2 debug composition, unless the user explicitly asks to restore something from it.
- Keep prototype views simple and dumb; avoid reintroducing hover-expanded all-in-one creature cards as the baseline.
- When in doubt, keep terminology aligned with the existing code:
  - `CreatureSpecies`
  - `CreatureUnit`
  - `FeatBoard`
  - `FeatNode`
  - `EncounterTable`
  - `EncounterTier`
- Treat `archive/` as reference, not live implementation.

## Current Screen And Window Inventory

### Runtime Screens

- `Main Menu / Load Screen`
  - Status: wireframed, not yet rebuilt in V3
  - Source: `main_menu.png`
  - Note: current intended layout is title on top, background behind menu, and right-side action stack

- `Battle HUD`
  - Status: wireframed, V3 baseline scripts created
  - Source: `BattleHUD.png`, `BattleUIElement.png`, `TeamUIElement.png`, `Creature_UI_Element.png`

- `Exploration Screen / HUD`
  - Status: required for prototype, wireframed, V3 baseline scripts created
  - Source: `Exploration_mode_screen.png`
  - Note: current layout includes a time widget, zone label, contextual icons, left-side party strip, and a largely unobstructed world view

- `Creature Card States`
  - Status: wireframed, old V2 version archived, V3 direction is to replace it with simpler slot views first
  - Source: `Creature_UI_Element.png`

### Editor Screens

- `Feat Board Editor`
  - Status: archived in V2
  - Source: `archive/V2/Assets/Scripts/Feats/Editor/FeatBoardEditorWindow.cs`
  - Purpose: edit species feat board layout, links, rewards, and requirements

- `Encounter Team Editor`
  - Status: archived in V2
  - Source: `archive/V2/Assets/Scripts/Encounters/Editor/EncounterTeamEditorWindow.cs`
  - Purpose: edit a six-creature encounter team with top tabs, board preview, and right inspector

- `Encounter Table Tier Window`
  - Status: wireframed, related implementation archived in V2
  - Source: `Encounter_Table_UI.png`, `archive/V2/Assets/Scripts/Encounters/Editor/*`

- `Team Editor Window`
  - Status: wireframed
  - Source: `Team_Editor_WindowV2.png`
  - Note: `Team_Editor_Window.png` is an earlier version; V2 should be treated as the preferred direction unless the user says otherwise

## ASCII Wireframes

These ASCII layouts are simplified summaries of the current PNG mockups.

### Main Menu
Source: `main_menu.png`

```text
                    +------------------------------+
                    |         Game Title           |
                    +------------------------------+

   [Background image or animated background scene]

                                      +----------------------+
                                      |      New Game        |
                                      +----------------------+
                                      +----------------------+
                                      |       Load           |
                                      +----------------------+
                                      +----------------------+
                                      |       Quit           |
                                      +----------------------+
```

### Exploration Screen
Source: `Exploration_mode_screen.png`

```text
+------------------+   +--------------------------------------+     [I] [I] [I]
| Time / Day-Night |   | Current Area / Zone Name            |     Context icons
+------------------+   +--------------------------------------+

+---------------------+
| [Party Slot 1]      |
+---------------------+
| [Party Slot 2]      |
+---------------------+
| [Party Slot 3]      |
+---------------------+
| [Party Slot 4]      |
+---------------------+
| [Party Slot 5]      |
+---------------------+
| [Party Slot 6]      |
+---------------------+

                [World map / terrain view with player centered]
```

### Battle HUD
Source: `BattleHUD.png`

```text
+----------------------+                             +----------------------+
| Player Team          |                             | Enemy Team           |
| [6 creature cards]   |                             | [6 creature cards]   |
+----------------------+                             +----------------------+

                +-----------------------------------------------+
                | Battle Action Bar / Active Creature Controls  |
                +-----------------------------------------------+
```

### Team UI Column
Source: `TeamUIElement.png`

```text
+---------------------------+
| Creature UI element #1    |
| Creature UI element #2    |
| Creature UI element #3    |
| Creature UI element #4    |
| Creature UI element #5    |
| Creature UI element #6    |
+---------------------------+
```

### Creature Card States
Source: `Creature_UI_Element.png`

```text
Compact / outside battle or placement
+----------------------------------------------------+
| [Icon]  Name                                       |
+----------------------------------------------------+

Battle version
+----------------------------------------------------+
| [Icon]  Name                                       |
| HP / Max HP bar                                    |
| Stamina / turn bar with countdown                  |
+----------------------------------------------------+

Expanded hover version
+----------------------------------------------------+
| [Icon]  Name                                       |
| [A1][A2][A3][A4][A5][A6][A7][A8]                   |
| HP / AP / MP / attack / magic / armor / resist     |
| Passive and status list                            |
+----------------------------------------------------+
```

### Battle Action Bar
Source: `BattleUIElement.png`

```text
     +-----------+     +-----------+     +-----------+
     | MP / Max  |     | HP / Max  |     | AP / Max  |
     +-----------+     +-----------+     +-----------+

+---------------------------------------------------------------+
| [1] 8 ability slots for the active creature               [8] |
| Shortcut labels sit on top of each slot                      |
+---------------------------------------------------------------+

                    +-------------------+
                    |     End Turn      |
                    +-------------------+
```

### Ability Detail Card
Source: `Action_UI_Element.png`

```text
+----------------------------------------------------+
| [Icon]  Ability Name                               |
| Cost AP/MP | Range | Area | Line of Sight          |
|                                                    |
| Generated rules text from the underlying effects   |
| Example: deal X magic damage, heal Y HP, etc.      |
+----------------------------------------------------+
```

### Passive Or Status Detail Card
Source: `Passive_UI_Element.png`

```text
+----------------------------------------------------+
| [Icon]  Passive Name                               |
|                                                    |
| Generated rules text from the underlying effects   |
| Example: damage bonus, healing bonus, immunity     |
+----------------------------------------------------+
```

### Encounter Table Tier Editor
Source: `Encounter_Table_UI.png`

```text
+------------------------------------------------------------+
| Table                                                      |
| > No Badge                                                 |
|   | Weight | ... | [Edit Team Composition] |               |
|   | Weight | ... | [Edit Team Composition] |    [-] [+]    |
|   | Weight | ... | [Edit Team Composition] |               |
| < One Badge                                                |
| < Two Badges                                               |
| < Three Badges                                             |
| < Four Badges                                              |
| < Five Badges                                              |
| < Six Badges                                               |
| < Seven Badges                                             |
| < Eight Badges                                             |
| < Post Game                                                |
+------------------------------------------------------------+
```

### Team Editor Window V2
Source: `Team_Editor_WindowV2.png`

```text
+----------------------------------------------------------------------------------+
| [Slot1] [Slot2] [Slot3] [Slot4] [Slot5] [Slot6]                                  |
+--------------------------------------------------------------+-------------------+
|                                                              | Selected creature |
| Large feat or node graph canvas                              | read-only summary |
| - unlock or lock nodes                                       | - stats           |
| - unlock abilities                                           | - abilities       |
| - unlock passives                                            | - passives        |
| - unlock forms or stat boosts                                |                   |
|                                                              | [Edit AI Behaviour]|
+--------------------------------------------------------------+-------------------+
```

## Prototype-Critical Open Questions

These are the design gaps that still matter right now for the prototype.

- `Main menu background`
  - Should the menu background be a static illustration, a rendered 3D scene, or a live camera view?

- `Exploration contextual icons`
  - Which actions live in the top-right icons during the prototype:
    - settings
    - save
    - quit
    - map
    - something else

- `Exploration HUD extras`
  - Beyond the current wireframe, do you want any of these in the prototype:
    - interaction prompt
    - objective text
    - minimap

- `Battle placement phase screen`
  - Should placement use a dedicated pre-combat layout or reuse the battle HUD with an alternate mode?

- `Battle targeting and preview overlays`
  - How much path, range, area, and line-of-sight preview should be shown before confirming an ability?

## Deferred Runtime Screens

These screens are intentionally out of prototype scope for now:

- Party management screen
- PC or creature storage screen
- Runtime creature detail screen
- Runtime feat board screen
- Capture-specific screen
- Battle result screen
- Dialogue-heavy interaction screens
- Pause, options, accessibility
- World map and fast travel

## Prototype Screen Set

The current prototype screen map is:

1. Main menu
2. Exploration screen
3. Battle screen

## Owner Questions To Answer Next

When updating this file later, prioritize answers to these:

1. Should the main menu background be static art, a 3D scene, or a live camera view?
2. Which actions belong in the top-right exploration icons?
3. Do you want an interaction prompt and objective text during exploration?
4. Do you want a minimap for the prototype, or no minimap?
5. Should battle placement be its own screen state, or just a battle sub-state?
6. How much battle targeting preview should be visible before confirming an action?
