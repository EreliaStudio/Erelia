# Erelia — Global Backlog

## Priority Scale

| Range | Meaning |
|-------|---------|
| 200 | Absolutely mandatory for a playable end-to-end prototype |
| 150–199 | Core loop / prototype-critical gameplay systems |
| 100–149 | Important game-completeness systems |
| 50–99 | Polish, feedback, iteration speed, quality-of-life |
| 0–49 | Optional, cosmetic, or low-impact improvements |

Each section below lists its top-priority epics only. Full ticket breakdowns (1–3 pts each) live in the section files linked in the headings.

---

## [Battle System — Backend](battle-backend.md)

| Priority | Epic | Detail file |
|----------|------|-------------|
| 155 | **AI turn evaluation** — `EnemyTurnPhaseController` must evaluate `AIBehaviour` rules and submit a legal `BattleAction` | [battle-backend.md](battle-backend.md) |
| 140 | **Status SO assets authored** — poison, burn, shield, DoT, HoT, stun assets needed; code infrastructure is ready | [battle-backend.md](battle-backend.md) |
| 125 | **Enemy placement strategy** — strategy interface + fixed / by-line alternatives beyond random | [battle-backend.md](battle-backend.md) |

---

## [Battle System — Controllers & HUD](battle-hud.md)

| Priority | Epic | Detail file |
|----------|------|-------------|
| 145 | **Battle result screen** — no victory / defeat screen exists | [battle-hud.md](battle-hud.md) |
| 130 | **End-of-battle feat progression summary** — show which nodes progressed after `EndPhase` | [battle-hud.md](battle-hud.md) |
| 110 | **Creature info window — infrastructure** — floating, draggable, closable panel on right-click of a creature card | [battle-hud.md](battle-hud.md) |
| 104 | **Creature info window — taming conditions section** — visible only on wild enemies with a `TamingProfile` | [battle-hud.md](battle-hud.md) |
| 85 | **Damage / heal floating text** — numeric popup on hits and heals | [battle-hud.md](battle-hud.md) |

---

## [Battle System — Board & Camera](battle-board-camera.md)

| Priority | Epic | Detail file |
|----------|------|-------------|
| 190 | **Board generation from world voxels** — `BoardDataBuilder` pipeline not yet connected to battle entry | [battle-board-camera.md](battle-board-camera.md) |
| 175 | **Battle camera rig** — no dedicated battle camera; exploration camera is the only one | [battle-board-camera.md](battle-board-camera.md) |
| 120 | **Camera transition exploration ↔ battle** — smooth blend/cut between rigs | [battle-board-camera.md](battle-board-camera.md) |
| 65 | **Camera shake on impact** — short shake coroutine tied to hit events | [battle-board-camera.md](battle-board-camera.md) |

---

## [Exploration & World](exploration-world.md)

| Priority | Epic | Detail file |
|----------|------|-------------|
| 200 | **Battle ↔ Exploration transition wiring** — encounter detected → board load → `BattleMode` not connected | [exploration-world.md](exploration-world.md) |
| 190 | **Encounter emitter wired to player movement** — `EncounterEmitter` not subscribed to `EventCenter.EmitPlayerMoved` | [exploration-world.md](exploration-world.md) |
| 170 | **Trainer line-of-sight trigger** — no `TrainerActor` or sight-line check exists | [exploration-world.md](exploration-world.md) |
| 145 | **Biome → encounter table selection** — pipeline biome → table → roll needs verification | [exploration-world.md](exploration-world.md) |
| 135 | **Procedural biome-based world generation** — `GenerateChunkMeta` returns `defaultBiome` for every chunk | [exploration-world.md](exploration-world.md) |

---

## [Creature & Feat System](creature-feat.md)

| Priority | Epic | Detail file |
|----------|------|-------------|
| 200 | **At least one authored `CreatureSpecies` + `FeatBoard`** — nothing playable without real creature data | [creature-feat.md](creature-feat.md) |
| 135 | **Feat Board runtime UI** — editor-only window exists; a runtime in-game equivalent is needed | [creature-feat.md](creature-feat.md) |
| 55 | **Feat Board respec** — GDD mentions it; not a main feature | [creature-feat.md](creature-feat.md) |

---

## [Player Progression & Meta](player-progression.md)

| Priority | Epic | Detail file |
|----------|------|-------------|
| 195 | **Save / Load system** — serialization to disk and loading on startup not wired | [player-progression.md](player-progression.md) |
| 135 | **Gym defeat tracking + encounter tier scaling** — cleared-gym set and tier feed missing | [player-progression.md](player-progression.md) |
| 125 | **Team management screen** — UI to swap creatures between team and PC box | [player-progression.md](player-progression.md) |
| 115 | **PC storage creature box** — PC box storage and swap UI needed | [player-progression.md](player-progression.md) |
| 105 | **Gym / Elite Four encounter definitions** — `EncounterTable` assets for 8 gyms × 8 tiers + Elite Four | [player-progression.md](player-progression.md) |

---

## [View / Animation System](view-animation.md)

| Priority | Epic | Detail file |
|----------|------|-------------|
| 130 | **Board movement tween for units** — smooth tile-to-tile move when `MoveAction` resolves | [view-animation.md](view-animation.md) |
| 120 | **`View.Animation.Set` ScriptableObject** — named recipe dictionary assigned to `CreatureForm` | [view-animation.md](view-animation.md) |
| 115 | **`View.Animation.Rig` MonoBehaviour** — `LogicalPart` → `Transform` mapping + rest pose | [view-animation.md](view-animation.md) |
| 110 | **`View.Animation.Animator` MonoBehaviour** — main channel + additive overlay execution | [view-animation.md](view-animation.md) |
| 105 | **Ability caster animation hook** — trigger recipe before effect resolution | [view-animation.md](view-animation.md) |

---

## [Audio](audio.md)

| Priority | Epic | Detail file |
|----------|------|-------------|
| 110 | **Audio manager / service** — no audio system exists; need SFX + BGM singleton | [audio.md](audio.md) |
| 85 | **Battle music** — BGM that starts on battle entry and stops on exit | [audio.md](audio.md) |
| 65 | **Ability SFX** — per-ability sounds triggered by `PlaySoundStep` | [audio.md](audio.md) |
| 50 | **UI SFX** — button click / menu navigation sounds | [audio.md](audio.md) |
| 45 | **Footstep SFX** — exploration walking sounds | [audio.md](audio.md) |

---

## [Content](content.md)

| Priority | Epic | Detail file |
|----------|------|-------------|
| 185 | **First creature roster** — minimum 3 fully specced creatures for test play | [content.md](content.md) |
| 175 | **Ability definitions** — physical, magical, DoT, heal, debuff, buff `Ability` assets | [content.md](content.md) |
| 160 | **Voxel tileset** — ground, wall, slope, water, grass `VoxelDefinition` assets | [content.md](content.md) |
| 80 | **Creature 3D models / voxel prefabs** — placeholder cubes with `AnimationRig` | [content.md](content.md) |
| 70 | **Town structures / building voxel templates** — gym, house, POI templates | [content.md](content.md) |

---

## [Technical / Infrastructure](technical-infrastructure.md)

| Priority | Epic | Detail file |
|----------|------|-------------|
| 195 | **Bootstrap / scene wiring end-to-end** — full startup flow needs validation | [technical-infrastructure.md](technical-infrastructure.md) |
| 140 | **Event system completeness** — verify no double-subscriptions across all `EventCenter` listeners | [technical-infrastructure.md](technical-infrastructure.md) |
| 130 | **Random seed determinism** — all `MetaWorldGenerator` random calls must use seeded `System.Random` | [technical-infrastructure.md](technical-infrastructure.md) |
| 90 | **Debug / cheat console** — in-game toggle for forcing encounters, feats, warps | [technical-infrastructure.md](technical-infrastructure.md) |
| 70 | **Voxel mesher performance profiling** — `VoxelMesher` is synchronous; spike risk at high chunk counts | [technical-infrastructure.md](technical-infrastructure.md) |
