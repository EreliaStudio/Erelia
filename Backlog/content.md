# Content

See [global.md](global.md) for the epic summary.

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 185 | 2 | **[CO-01] Author `CreatureSpecies` asset — Species 1 (full spec)** | Stats, 3 default abilities, `FeatBoard` reference, `TamingProfile` reference. Must be self-sufficient for a short battle test. Target: a balanced all-rounder. |
| 185 | 2 | **[CO-02] Author `CreatureSpecies` asset — Species 2 (full spec)** | Same structure as CO-01. Target: a fast, fragile attacker (low Recovery, high Attack). Distinct taming conditions from species 1. |
| 185 | 2 | **[CO-03] Author `CreatureSpecies` asset — Species 3 (full spec)** | Target: a slow, tanky support (high HP and Armor, lower Attack). Distinct taming conditions and feat board path. |
| 175 | 1 | **[CO-04] Author physical direct damage `Ability` asset** | `Ability` SO: costs AP, targets `Enemy`, single-cell range, physical damage `Effect`. No status. Assign SFX stub and animation name. |
| 175 | 1 | **[CO-05] Author magical direct damage `Ability` asset** | Same structure as CO-04 but magical damage type and slightly longer range. |
| 175 | 1 | **[CO-06] Author DoT `Ability` asset (poison or burn)** | Applies the Poison or Burn status SO from BB-08/BB-09 to the target. Physical or magical per design choice. |
| 175 | 1 | **[CO-07] Author heal / HoT `Ability` asset** | Targets `Ally`, restores HP (direct or over-time via status). Costs AP, no line-of-sight requirement. |
| 175 | 1 | **[CO-08] Author movement debuff `Ability` asset** | Reduces target's MP for one turn (via a short-duration debuff status) or applies a root effect. |
| 175 | 1 | **[CO-09] Author buff `Ability` asset** | Targets `Ally`, increases Attack or Magic via a buff status for a configurable duration. |
| 160 | 1 | **[CO-10] Ground `VoxelDefinition` asset** | Walkable, no terrain cost modifier. Base tile for roads, town floors, etc. |
| 160 | 1 | **[CO-11] Wall `VoxelDefinition` asset** | Non-walkable, blocks line of sight. Used for building walls, cliffs, and obstacles. |
| 160 | 1 | **[CO-12] Slope `VoxelDefinition` asset** | Walkable, connects different height levels. Movement cost modifier: 2 MP per cell. |
| 160 | 1 | **[CO-13] Water `VoxelDefinition` asset** | Non-walkable by default (or high movement cost if wading is desired). Used for rivers, ponds. |
| 160 | 1 | **[CO-14] Grass / bush `VoxelDefinition` asset** | Walkable. Marked as an encounter trigger surface. Used to delineate wild encounter zones. |
| 80 | 1 | **[CO-15] Placeholder prefab with `AnimationRig` — Species 1** | Cube primitive + `AnimationRig` MonoBehaviour with `WholeRig` mapped to the cube's `Transform`. Portrait sprite optional. |
| 80 | 1 | **[CO-16] Placeholder prefab with `AnimationRig` — Species 2** | Same as CO-15, different color material to distinguish the species in test play. |
| 80 | 1 | **[CO-17] Placeholder prefab with `AnimationRig` — Species 3** | Same as CO-15/CO-16, third distinct color. |
| 70 | 2 | **[CO-18] Gym building voxel template** | Pre-built voxel cell grid representing a gym exterior. Includes entrance trigger volume and a trainer actor placeholder. Reusable across procedurally placed gym POIs. |
| 70 | 1 | **[CO-19] House voxel template** | Simple house exterior. Includes an entrance teleport trigger (EW-19). Used as filler in procedurally generated towns. |
| 70 | 1 | **[CO-20] POI / landmark voxel template** | Generic landmark template (well, signpost, statue) that marks points of interest on the overworld without interior access. |
