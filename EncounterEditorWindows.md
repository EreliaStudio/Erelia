# Encounter Editor Windows

These are the two editor windows now intended for biome encounter authoring.

## Encounter Table Window

```text
+------------------------------------------------------------------------------------------------------+
| Biome [BiomeDefinition]  [Use Selection]  [Add Trigger Tag]                                         |
+------------------------------------------------------------------------------------------------------+
| Trigger Tags               | Selected Rule                                                           |
| +-----------------------+  | Trigger Tag [bush______________] [Apply Tag Rename]                    |
| | bush                X |  | Base Chance Per Step [-----|----] 0.20                                 |
| | water               X |  |                                                                        |
| | cave                X |  | +----------------+ +----------------+ +----------------+ +-----------+ |
| | road                X |  | | No Badge       | | 1 Badge        | | 2 Badges       | | 3 Badges  | |
| +-----------------------+  | | X [name] [w] E | | X [name] [w] E | | X [name] [w] E | | ...       | |
| [Add Trigger Tag]         | | X [name] [w] E | | X [name] [w] E | | X [name] [w] E | |           | |
|                            | |       [+ Team] | |       [+ Team] | |       [+ Team] | |           | |
|                            | +----------------+ +----------------+ +----------------+ +-----------+ |
|                            | +----------------+ +----------------+ +----------------+ +-----------+ |
|                            | | 4 Badges       | | 5 Badges       | | 6 Badges       | | 7 Badges  | |
|                            | |                | |                | |                | |           | |
|                            | |       [+ Team] | |       [+ Team] | |       [+ Team] | | [+ Team]  | |
|                            | +----------------+ +----------------+ +----------------+ +-----------+ |
|                            | +----------------+ +----------------+                                            |
|                            | | 8 Badges       | | Post Game      |                                            |
|                            | |                | |                |                                            |
|                            | |       [+ Team] | |       [+ Team] |                                            |
|                            | +----------------+ +----------------+                                            |
+------------------------------------------------------------------------------------------------------+
```

## Encounter Team Window

```text
+------------------------------------------------------------------------------------------------------+
| Biome [BiomeDefinition]  Trigger [bush]  Tier [No Badge]  Team Name [forest_pack_______]           |
+------------------------------------------------------------------------------------------------------+
| [Icon] Unit 1  [Icon] Unit 2  [Icon] Unit 3  [Icon] Unit 4  [Icon] Unit 5  [Icon] Unit 6          |
+------------------------------------------------------------------------------------------------------+
|                                                                 |                                    |
|   Feat board / progression graph for selected unit              | Team                               |
|   - click node to inspect                                       | Display Name [forest_pack______]   |
|   - complete / reset nodes from the inspector                   |                                    |
|   - same species board as the main feat editor                  | Species [CreatureSpecies] [Edit]   |
|                                                                 | Slot 1                             |
|                                                                 | [Icon] Name                        |
|                                                                 | Current Form: base                 |
|                                                                 |                                    |
|                                                                 | Stats                              |
|                                                                 | Health ....                        |
|                                                                 | AP ........                        |
|                                                                 | Movement ..                        |
|                                                                 | ...                                |
|                                                                 |                                    |
|                                                                 | Abilities                          |
|                                                                 | - Ability A                        |
|                                                                 | - Ability B                        |
|                                                                 |                                    |
|                                                                 | Passives                           |
|                                                                 | - Passive A                        |
|                                                                 |                                    |
|                                                                 | Selected Node                      |
|                                                                 | [Complete Once]                    |
|                                                                 | [Reset Node]                       |
|                                                                 | [Clear All Progress]               |
|                                                                 |                                    |
|                                                                 | [Edit AI Behaviour]                |
|                                                                 | (disabled for now)                 |
+------------------------------------------------------------------------------------------------------+
```
