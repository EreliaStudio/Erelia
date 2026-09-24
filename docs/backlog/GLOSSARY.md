# Glossary and Capability Navigation

Terms below reflect the current GDD plus approved greenfield decisions where implementation vocabulary has already been fixed.

Entries are sorted alphabetically by term. The Backlog owner column points to an approved Epic when ownership is already established; otherwise it remains TBD.

| Term | Meaning in current design | GDD / decision source | Backlog owner |
| --- | --- | --- | --- |
| Active squad | Exactly three adventuring Heroes per player | §7.2 | TBD |
| AP | Combat resource used for spells/actions | §17.3 | TBD |
| Combat Area Builder | Service that extracts relevant combat cells from authoritative traversal/world data | §16.2 | TBD |
| CombatCell | One traversable terrain surface with 1×1 horizontal footprint and 3D standing position | §16.1 | TBD |
| Dormant Crystal Site | Predefined location that can become a temporary public outpost | §6 | TBD |
| Dungeon RotationIndex | Server-time-derived index used in deterministic rotating dungeon seeds | §32.3 | TBD |
| Elastic combat area | Union of valid cells around current combatants; may grow/shrink as participants move | §16.3 | TBD |
| Enchantment | Known item transformation with optional RNG success/failure | §27 | TBD |
| Encounter Clock | Independent tactical battle time domain | §13.2; DR-006 | TBD |
| Enemy camp site | Predefined possible overworld camp location; only a subset are active | §36 | TBD |
| Expedition inventory | Fixed-size per-player inventory shared by the three active Heroes | §29.1 | TBD |
| Exploration Action | Explicit special traversal action such as Jump/Climb using destination targeting | §11 | TBD |
| Flee | Explicit per-Hero combat action to leave an encounter under defined conditions | §22 | TBD |
| Gambit | Ordered Condition → Target → Reaction tactical AI rule | §24 | TBD |
| Global bank | Per-player/account bank shared across permanent towns and eligible outposts | §29.2 | TBD |
| Grand Dungeon | Repeatable high-level dungeon whose first server completion unlocks the next World | §35 | TBD |
| Hero | Classless player-controlled character whose role emerges from loadout | §7 | TBD |
| Item Tag | Zero or more semantic labels with no intrinsic gameplay effect | §8.4 | TBD |
| Item Type | Exactly one semantic equipment Type used by spell requirements/formulas | §8.3 | TBD |
| Marketplace-funded PvE reward | Finished item reward procured from player marketplace listings against a reward budget | §34 | TBD |
| MP | Combat resource used for tactical movement | §17.3 | TBD |
| Outpost | Public predefined frontier services kept active by resource upkeep | §6 | TBD |
| Permanent Town | Shared social/economic settlement with independently upgraded buildings | §5 | TBD |
| Readiness | Progress toward the next combat activation | §17.2 | TBD |
| Reinforcement | Eligible late arrival after the initial participant set is locked | §14.2 | TBD |
| Resource node | Shared server gathering entity with depletion and respawn | §25 | TBD |
| ServerSeed | Root deterministic generation seed owned by a server | §3.2 | TBD |
| Spell item | Crafted/tradable/storable/equippable spell object | §9 | TBD |
| Status | Event-driven combat state with effects, hooks, and expiration rules | §20 | TBD |
| Surprise | Symmetric encounter-start advantage for an aware initiating side against an unaware opponent | §15 | TBD |
| Temporary combat entity | Encounter-owned object removed when the encounter ends | §21 | TBD |
| Terrain Chunk | Fixed 16×16×16 terrain voxel volume at one world unit per cell; addressed in integer Chunk coordinates and generated authoritatively by the Server | §1.2; DR-011; EP-001 | EP-001 |
| TurnInterval | Encounter-clock duration of a unit's readiness cycle; lower means more frequent activation | §17.1 | TBD |
| Voxel | A cell in Erelia's shared multi-scale voxel-volume representation; terrain uses world-scale cells while models may use finer uniformly sized cells | §1.2 | EP-001 |
| Voxel::Cell | Compact shared voxel-cell value whose approved representation packs a runtime Definition ID, horizontal Orientation, and vertical Flip into one 32-bit value; Definition ID 0 denotes empty | §1.2; DR-012 | EP-001 |
| Voxel Definition | Shared semantic definition referenced by voxel cells and used with normalized Shapes/material concepts by both terrain and models | §1.2; DR-012 | EP-001 |
| Voxel Shape | Normalized voxel geometry definition shared across terrain and models; cubes, slabs, slopes, and stairs are explicitly supported Shape families in the current design | §1.2 | EP-001 |
| Voxel::Volume | Shared owning voxel container with runtime dimensions, uniform voxel size, and contiguous `Voxel::Cell` storage; terrain Chunks are one semantic use of this generic volume concept | §1.2; DR-012 | EP-001 |
| Walking graph | Authoritative voxel-surface connectivity for AI/followers/combat derivation | §12 | TBD |
| World | Persistent voxel region with its own terrain/content/progression tier | §4 | TBD |
| World portal | Server-wide access link; next-World portal permanently activates after first Grand Dungeon victory | §4.2, §35 | TBD |
| World/Region Clock | Continuously running exploration/regional simulation time domain | §13.1; DR-006 | TBD |
