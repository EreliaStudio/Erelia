# Project Context — Fast-Start Note

**Purpose:** give a future planning session a high-density understanding of Erelia without re-reading the entire repository.
**Authority:** summary only. The GDD and later explicit decisions remain authoritative.
**GDD snapshot:** see SOURCE-BASELINE.md.

## 1. Product identity

Erelia is a persistent online voxel fantasy RPG combining third-person exploration with tactical cell-based combat.

The core loop is: explore a World; discover resources, camps, dungeons, routes and points of interest; gather and refine; contribute to shared infrastructure; craft, enchant and trade items and spells; improve Hero loadouts; clear normal dungeons; prepare for and defeat the Grand Dungeon; permanently unlock the next World portal for the server.

The game targets official persistent multiplayer servers while allowing private servers.

## 2. Core design pillars

- Major infrastructure and World access are shared server progression.
- Heroes are classless; roles emerge from equipment, stats, item Types, item Tags, eight equipped spells, and tactical decisions.
- Finished persistent equipment, spell items, tools, and consumables are intended to be player-produced and traded.
- Geography and distance from civilization matter.
- Tactical combat is derived from the same voxel terrain used during exploration.
- Cloud, telemetry, networking, and voxel architecture support gameplay rather than dictate it.

## 3. Voxel and visual rules already stated by the GDD

- Terrain, Heroes, creatures, equipment, and props share one multi-scale voxel visual language and fundamental voxel-volume representation.
- Terrain Chunk size is 16×16×16 cells at one world unit per cell.
- Model grids are runtime-sized and use a smaller configurable uniform cell size.
- Terrain and models share voxel Definitions, normalized Shapes, and material concepts.
- Cubes, slabs, slopes, and stairs may exist at either scale.
- Animated characters are rigid voxel-volume parts connected by transforms.
- Animation normally transforms parts rather than rewriting cells or remeshing them.
- Visible equipment is a separately attached voxel model.
- Base permanent voxel terrain is immutable during ordinary gameplay.
- Fine visual cells do not automatically become gameplay traversal nodes, collision primitives, or combat cells.

## 4. Worlds and shared progression

Each server owns its progression state, economy, town upgrades, active outposts, World portals, resource state, and dynamic overworld content.

A root ServerSeed deterministically derives World seeds by WorldID. Equivalent seeds may reproduce equivalent generated geometry on separate servers while runtime/progression state remains separate.

Worlds contain terrain, biomes, enemies, resources, points of interest, dungeons, portals, and possibly permanent towns and outpost crystal sites. Not every World requires a town.

Defeating a World's Grand Dungeon for the first time permanently activates the portal to the next World for the entire server.

Higher Worlds primarily represent vertical power and economic progression. Exact World count, cadence, tiering, town spacing, and playtime remain open content questions.

## 5. Permanent towns and temporary outposts

Permanent towns are shared social/economic anchors.

There is no single Town Level. Buildings upgrade independently through predefined server-wide resource contributions. Current roles include Guild Center, crafting facilities, Mage Tower, Enchantment Building, Alchemist, and tool production.

Dormant Crystal Sites are predefined by generation/authored content. Players do not freely place outposts. Community activation creates a predefined public outpost. Possible services include teleportation toward lower permanent civilization, repairs, marketplace, global bank, and Guild-Center-like roster management.

Outposts consume resources over time. When upkeep reaches zero, services disappear or become unavailable and the site returns to dormancy.

## 6. Heroes, roster, equipment, and spells

Players may create/manage an unlimited roster of blank Heroes at a Guild Center. The active adventuring squad is exactly three Heroes per player.

Multiplayer parties combine each player's squad rather than replacing per-player Hero slots.

There are no classes, Feat trees, character levels, profession levels, weapon mastery gates, or World-completion equipment gates in the baseline.

Equipment slots include Main Hand, Off Hand, Head, Chest, Legs, Boots, and Accessories. The accessory count is still tunable. Items specify hand occupancy.

Every equipment item has exactly one Type and zero or more Tags. Tags have no intrinsic effect; spells query them for availability, scaling, or conditional behavior.

Crafted items can roll recipe-defined stat ranges. Persistent equipment has durability and ultimately breaks and disappears. Repair may delay breakage, but exact durability/repair rules remain open.

Spells are crafted, tradable, storable, equippable objects. Each Hero has exactly eight equipped spell/ability slots.

Spell availability can depend on equipped Types and Tags. Spell effects are constrained data-driven formulas, not unrestricted first-version scripting.

## 7. Exploration and traversal

Exploration uses third-person direct control with continuous collision-based movement.

The other active Heroes are physical followers. Baseline follower modes are Follow and Stay, with direct-control switching between Heroes. Followers can trigger vision, traps, encounters, and separation.

First-version enemy detection is vision only. Detection causes a real-time chase before tactical engagement.

Jumping, climbing, and similar special traversal are explicit Exploration Actions. They transition temporarily to top-down destination targeting. The World Clock continues while the player targets.

Walking navigation is derived from authoritative voxel surface information. Shape-specific cardinal/center heights and orientation determine walking connectivity. Multiple traversable surfaces can exist in one horizontal column.

The walking graph supports enemy pathfinding, followers, combat-cell derivation, and tactical movement; it does not replace continuous player control.

## 8. Time and encounter model

Active regions run continuously on a World/Region Clock.

Each tactical battle owns an independent Encounter Clock. Several encounters may run in one region.

When a player-controlled combat unit becomes ready and needs input, only that Encounter Clock pauses. The World and other encounters continue.

Initial encounter participants synchronize before handoff: uncommitted selection is cancelled; committed actions finish; units freeze; final positions are snapshotted; combat cells are resolved; participants transfer to the Encounter Clock.

Late eligible arrivals are Reinforcements and do not restart the initial synchronization barrier. Reinforcements enter with zero readiness progress / a fresh full Turn Interval.

Surprise is symmetric. If one side initiates hostility while the other is unaware, the surprising side's initial units begin Ready.

## 9. Tactical combat

A CombatCell represents one traversable terrain surface with a 1×1 world-unit horizontal footprint and a 3D standing position.

The Combat Area Builder is separate from combat rules. Baseline extraction is radius-based around participants. The effective area is elastic: the union of valid cells around current combatants using one configurable global radius.

Each combat unit has a TurnInterval measured in Encounter Clock seconds. Lower means more frequent activation. Readiness tracks progress to the next activation.

AP pays for spells/combat actions. MP pays for tactical movement. Heroes share common base AP/MP values that equipment may modify. There is no baseline Mana/Energy/Rage resource.

Core combat stats are Physical Power, Physical Defense, Magic Power, and Magic Defense, plus required derived/survival values such as HP, critical stats, Range, AP, MP, and Turn Interval modifiers.

Spells compute raw physical or magical effects through data-driven formulas. Defense applies percentage reduction with diminishing returns. The exact defense formula and balancing constants remain open.

Critical hits exist. Spells can opt in/out of critical eligibility.

Targeting is cell-based with minimum/maximum range, optional Range modifiers, line of sight, linear/diagonal restrictions, target categories, and authored AoE shapes. Usage constraints include AP cost, cooldown, uses per turn, uses per target per turn, optional encounter limits, and conditional restrictions.

## 10. Statuses, temporary entities, fleeing, and AI

Statuses are event-driven: persistent state + effects + hooks/triggers + expiration rules. Hooks can include turn start/end, encounter seconds, AP/MP spending, spell cast, movement, cell entry, damage, criticals, death, and other explicit events.

Spells and statuses should reuse a bounded effect vocabulary such as damage, healing, stat modification, TurnInterval/readiness manipulation, Push/Pull, Teleport/Swap, status application/removal, and SpawnEntity.

Temporary walls, traps, zones, totems, portals, turrets, and summons are Encounter-owned entities and disappear when the encounter ends.

Summons initially reuse the normal CombatUnit model and may receive normal controllable turns.

Flee is an explicit per-Hero action. Baseline eligibility requires no hostile within a configurable threat range. A fled Hero waits outside combat until all of that player's participating Heroes have fled or been defeated. A fully escaped player may later re-enter as a Reinforcement.

Defeat is normally temporary. Defeated Heroes recover at 1 HP after battle. Full team defeat respawns the player at the last activated Respawn Point with no default gold, durability, or experience penalty.

Enemy AI is an ordered Condition → Target → Reaction Gambit list. It evaluates top to bottom and executes the first satisfiable rule.

## 11. Gathering, crafting, enchantment, and economy

Gathering has no profession level. The required tool and tool tier gate access.

Resource nodes are shared server entities: gathering removes them for everyone until respawn. Permanent terrain is unchanged.

Raw resources are refined before many recipes. Higher tiers require increasingly expensive raw-to-refined conversion.

Any player may craft when the server has unlocked the required facility/recipe, the player has resources, and required fees can be paid.

The baseline aims for finished persistent items to originate from player production rather than ordinary monster generation.

Enchantments have a known intended result and may have an RNG success chance. On baseline failure, costs remain spent while the item remains otherwise unchanged. Enchantments can add/remove/replace Tags, transform Type, or apply deterministic stat tradeoffs.

There is one universal gold-like currency. Sources include monsters, chests, quests, dungeons, and game procurement of player-made marketplace items. Sinks include taxes, healing, crafting/refining, enchantment, repairs, and services.

The first marketplace is server-wide.

## 12. Inventory and bank

Each player has one fixed-size expedition inventory shared by the three active Heroes. It uses slots/stacks rather than weight simulation in the first design.

Permanent towns share one player/account global bank. Active outposts with storage access connect to the same bank.

## 13. Dungeons, rewards, camps, and progression

Normal dungeons provide specialized resources, training for the Grand Dungeon, and optional World access opportunities. Completing every normal dungeon is not a global prerequisite by default.

A dungeon may have several entrances leading to different starts within one instance. NPC quests are conventional guidance/access mechanisms, but independent discovery should generally remain valid.

Dungeons are instanced per party/group. Exploration inside still uses a regional clock; fights create Encounter Clocks.

Geometry is assembled from handcrafted voxel rooms/features through WFC or a similar constraint-based modular algorithm.

Dungeon layouts rotate on a configurable server schedule. Seed concept: ServerSeed + DungeonID + RotationIndex. Re-entering within one rotation reproduces structure while resetting runtime state; a new rotation changes structure.

Normal dungeons have explicit completion objectives and vault rewards.

PvE may reward finished player-crafted items by purchasing eligible marketplace listings against a reward budget. The exact fair-pricing, eligibility, outlier, and manipulation-protection algorithm remains open.

The Grand Dungeon follows normal dungeon rules and additionally unlocks the next World portal on first server completion.

Overworld enemy camps occupy predefined candidate sites, with only a subset active. Clearing one can activate another predefined site and may use a smaller marketplace-backed reward budget.

## 14. Multiplayer, telemetry, and future tools

The GDD expects the authoritative server to own important shared/persistent state including accounts/inventory, Hero roster/loadouts, World progression, towns, outposts, shared resources, camps, marketplace, dungeon instances, encounters, and economy/rewards.

Voxel geometry may be deterministic from seeds while dynamic entities synchronize separately.

Gameplay simulation determines movement, collision, combat occupancy, and effect timing. Animation and visual geometry do not authoritatively determine damage or other outcomes.

A local authoritative host may be used for early validation if it preserves the same command boundary as the later dedicated server.

Telemetry should be authoritative-server driven where practical and event-oriented.

A future content editor is not a core first-version requirement. Official runtime data formats should be reusable by future content tooling where practical.

## 15. Explicit GDD open questions

- Complete Hero stat sheet and defaults.
- Equipment stat-generation/budget/quality model.
- Which spell-item properties vary per crafted copy.
- Durability loss and repair semantics.
- Defense formula, critical baseline, AP/MP defaults, TurnInterval defaults, combat radius, flee threat radius, reinforcement details.
- Permanent-town building catalog, tiers, recipes, and costs.
- World count, cadence, tier structure, town spacing, and expected playtime.
- Marketplace procurement pricing and anti-manipulation algorithm.
- PvP is not part of the established design.

## 16. First playable direction in the GDD

The GDD recommends visual validation before broad gameplay/content production, then a focused vertical slice: one generated World, one town, limited building/resource tiers, gathering/refining/crafting/enchantment, three-Hero flow, exploration/followers/detection, voxel-derived combat, World/Encounter clocks, readiness/AP/MP, eight spells, formula effects, Gambit AI, one normal dungeon, one Grand Dungeon, one outpost crystal, one marketplace-backed reward chest, and one next-World activation event.

Whether this recommendation becomes the actual implementation roadmap is still a planning decision.

## 17. Current implementation reality

The active Erelia repository currently contains only a fresh Core/Server/Client scaffold, build configuration, smoke/status functions, per-layer GoogleTest suites, CI, and Sparkle dependency wiring.

Do not infer future architecture from those placeholder status functions or archived code.
