# VS-001 — First Playable Vertical Slice

## Goal

Prove Erelia's distinctive gameplay loop with one World and deliberately narrow content breadth, building on the visual/technical direction accepted at VS-000.

## Visual/runtime prerequisite

- Unified VoxelVolume/VoxelMesher path is the supported path for world and imported voxel models.
- Current terrain regression suite remains green.
- At least one external voxel authoring/import workflow is documented and repeatable.
- Hero visual is a rigid articulated assembly of cached VoxelModel parts; transform animation does not remesh.
- Equipment attachment uses independent VoxelModel assets.
- Base palette Materials are usable; advanced effects are optional.

## Gameplay slice

One World, one town, a small building set, 1–2 resource tiers, gather/refine/craft, Type/Tag/stat generation, one Enchantment Building, three-Hero flow, exploration/followers, vision/chase, voxel combat cells, clock split, readiness/AP/MP combat, eight spell slots, formulas, Gambit enemy, one normal modular dungeon, one Grand Dungeon, one dormant outpost, one marketplace-backed chest and next-World portal activation.

## System acceptance cases

1. **VS-001-01 Fresh seeded server/world bootstrap:** same seed/content/generation version produces the same immutable base World.
2. **VS-001-02 Roster to expedition:** create Heroes, select active three, instantiate visual assemblies and enter exploration.
3. **VS-001-03 Gather to craft:** deplete a shared node, refine resources and craft a persistent item.
4. **VS-001-04 Enchant success/failure:** both paths consume correct costs and preserve item invariants.
5. **VS-001-05 Detection to encounter:** vision → chase → handoff → tactical encounter from actual positions.
6. **VS-001-06 Independent clocks:** pausing one encounter leaves Region Clock and another encounter advancing.
7. **VS-001-07 Tactical turn:** readiness, AP/MP, movement, spell targeting/damage and end turn execute through authority.
8. **VS-001-08 Reinforcement:** later participant joins at zero readiness without encounter restart.
9. **VS-001-09 Flee/defeat:** per-Hero flee and defeat recovery return valid world state.
10. **VS-001-10 Dungeon rotation:** same rotation recreates structural layout while runtime state resets.
11. **VS-001-11 Reward procurement:** funded reward source buys a real eligible player listing and pays seller.
12. **VS-001-12 Grand Dungeon progression:** first success unlocks next-World portal exactly once.
13. **VS-001-13 Save/restart:** persistent state reloads without item/progression/world corruption.
14. **VS-001-14 Headless parity:** same authoritative command stream with and without presentation reaches same state hash.
15. **VS-001-15 Model mesh reuse:** representative Heroes/equipment reuse cached voxel meshes across transforms/poses.
16. **VS-001-16 Material/animation independence:** transform animation and palette/effect changes do not invoke VoxelMesher.
