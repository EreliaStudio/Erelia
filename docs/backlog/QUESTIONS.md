# Architecture Question Register

These questions are intentionally unresolved. They are not decisions.

The first conversation should focus on the highest-impact questions that constrain many later systems. Independent planning can continue around questions that do not block the same area.

## A. Repository and product boundaries — highest priority

### Q-001 — Are Core / Server / Client deliberate long-term product boundaries?

**Status:** Resolved — see DECISIONS/DR-001-PRODUCT-BOUNDARIES.md.

The current repository scaffold has Core, Server, and Client. Should this remain the intended high-level product split, or is it only a bootstrap layout that may change?

Consequence: ownership rules, allowed dependencies, test placement, and public contracts.

### Q-002 — What is Core allowed to own?

**Status:** Resolved — see DECISIONS/DR-001-PRODUCT-BOUNDARIES.md.

Candidate interpretations include pure deterministic/shared game-domain rules; domain rules plus shared data/serialization; or broader engine-facing gameplay services.

Which responsibilities must explicitly remain out of Core?

### Q-003 — What may Core depend on?

**Status:** Resolved — see DECISIONS/DR-002-SPARKLE-CORE-DEPENDENCY.md.

Core currently links Sparkle Core. Is that an approved long-term dependency, or should the game-domain portion be independent from Sparkle except through adapters?

### Q-004 — What is the Server's ownership boundary?

**Status:** Resolved — see DECISIONS/DR-001-PRODUCT-BOUNDARIES.md and ARCHITECTURE/ARCH-001-PRODUCT-BOUNDARIES.md.

Which behaviors must exist only on the authoritative Server versus reusable deterministic domain code in Core?

### Q-005 — What is the Client's ownership boundary?

**Status:** Resolved — see DECISIONS/DR-001-PRODUCT-BOUNDARIES.md and ARCHITECTURE/ARCH-001-PRODUCT-BOUNDARIES.md.

Should Client own only input/presentation/rendering, or may it run predictive/speculative copies of domain simulation? If prediction is allowed, which systems may predict?

## B. Authority and semantic protocol — highest priority

### Q-006 — What is the first authoritative runtime topology?

**Status:** Resolved — dedicated Server process from the first playable; see DECISIONS/DR-003-DEDICATED-SERVER-FIRST.md.

For the first implementation, should we target dedicated server from the start, a local authoritative host with the same semantic command boundary, or both from the first milestone?

The GDD permits local authoritative hosting as an implementation stage but does not select the first target.

### Q-007 — What are the first command/response/event semantics?

**Status:** Resolved at the semantic level — see DECISIONS/DR-004-COMMAND-AUTHORITY-SEMANTICS.md and ARCHITECTURE/ARCH-002-AUTHORITATIVE-PROTOCOL.md.

Before choosing a networking library, which player intentions must cross the trust boundary as Commands, and which results return as Responses, Rejections, Events, or Snapshots?

Examples needing decisions: exploration movement, inventory changes, roster/loadout edits, crafting, marketplace operations, encounter actions, targeting, and Flee.

### Q-008 — What may the Client assert versus request?

**Status:** Resolved — Client sends intent, Server derives authoritative result; see DECISIONS/DR-004-COMMAND-AUTHORITY-SEMANTICS.md.

For authoritative systems, should the Client send intent only and let the Server derive all resulting state, or are some client-computed values permitted when the Server validates them?

### Q-009 — What local prediction/reconciliation is required for third-person movement?

**Status:** Direction resolved — local movement prediction is required; exact reconciliation mechanics are deferred to the future exploration-movement Epic. See DECISIONS/DR-005-CLIENT-MOVEMENT-PREDICTION.md.

The GDD requires responsive continuous exploration and server authority but does not define prediction, rewind, reconciliation, or tolerated divergence.

## C. Identity, ownership, and lifetime — highest priority

### Q-010 — Which concepts need stable persistent identifiers?

Candidates include player/account, Hero, crafted item, spell item, World, permanent building, outpost site, resource node, camp site, dungeon definition, and personal access unlock.

Which identities must survive server restarts and serialization?

### Q-011 — Which concepts need runtime-only identifiers?

Candidates include active entity, encounter, dungeon instance, temporary combat entity, status instance, marketplace transaction, and command/request.

Do runtime IDs need to be globally unique, unique per server, or scoped to an owning aggregate?

### Q-012 — What is the lifetime/invalidation rule for references?

When a persistent item breaks, a temporary entity disappears, an encounter ends, a dungeon instance resets, or a resource node cycles, what references may remain and how must stale references fail?

## D. Persistence and transactional state — highest priority

### Q-013 — What exact server state must survive restart?

The GDD says the server owns progression/economy/world state, but explicit restart semantics are needed for players/inventory/bank/rosters/loadouts; buildings/contributions; portals; outposts/upkeep; resource depletion/respawn; camps; marketplace; dungeon rotation/runtime instances; active encounters/readiness; cooldowns/statuses; and telemetry delivery state.

### Q-014 — Do active encounters survive a Server restart?

If yes, what exact snapshot/time semantics apply? If no, what recovery result is required for participants?

### Q-015 — Which economy operations require atomic transactions?

Crafting, enchantment, marketplace purchase, marketplace-funded reward procurement, building contribution, durability breakage, bank/inventory transfers, and outpost upkeep can cross several pieces of state. Which must commit atomically?

### Q-016 — What are retry/idempotency requirements for persistent commands?

For example, if a client retries a marketplace purchase or crafting request after a timeout, how is duplicate execution prevented?

## E. Time, determinism, and concurrency — high priority

### Q-017 — What time representation is authoritative?

**Status:** Partially resolved — Encounter, World, and Real Time are separate domains; exact units/storage representations remain open. See DECISIONS/DR-006-THREE-TIME-DOMAINS.md.

We need deliberate units/types for World time, Encounter time, cooldowns, outpost upkeep, resource respawns, dungeon rotation, and persistent timestamps.

### Q-018 — Fixed-step or variable-step simulation?

**Status:** Partially resolved — World Time advances in discrete ticks; exact tick frequency and Encounter advancement mechanics remain open. See DECISIONS/DR-006-THREE-TIME-DOMAINS.md.

Do World/Region and Encounter simulation use fixed ticks, variable delta time, or different models? Does an Encounter Clock advance only on discrete simulation steps?

### Q-019 — What determinism is required across platforms?

**Status:** Resolved — semantic determinism rather than universal bit-for-bit runtime determinism; see DECISIONS/DR-007-SEMANTIC-DETERMINISM.md.

Should identical inputs and seeds produce bit-for-bit/semantic-equivalent simulation on Windows and Linux, or is deterministic content generation sufficient while runtime simulation can vary within controlled tolerances?

### Q-020 — How are concurrent authoritative operations ordered?

**Status:** Resolved at the architectural level — Server serializes conflicting authoritative mutations; exact subsystem concurrency primitives remain local implementation choices. See DECISIONS/DR-008-AUTHORITATIVE-OPERATION-ORDERING.md.

Examples: two players gather the same node; two buyers select the same marketplace listing; an outpost upkeep tick races a deposit; a reinforcement joins while an Encounter pauses.

Do we require a single deterministic serialized order per owning aggregate/region, or another concurrency model?

## F. World, coordinates, and voxel gameplay boundaries — high priority

### Q-021 — What coordinate conventions become architectural contracts?

**Status:** Resolved — see DECISIONS/DR-011-VOXEL-COORDINATES.md.

We need explicit axis orientation, handedness, unit scale, integer voxel coordinates, Chunk origin convention, World/Region coordinates, model-local coordinates, and conversion rules.

### Q-022 — What is the authoritative gameplay collision representation for articulated voxel entities?

The GDD says visual voxel parts do not automatically determine gameplay collision. What primitive/model owns collision for Heroes, enemies, props, summons, and temporary barriers?

### Q-023 — What is the first asset authoring/import format?

The GDD leaves external tooling and interchange format open. This blocks precise model/pivot/attachment/animation asset contracts.

### Q-024 — What is runtime-editability of voxel/model resources?

Which imported/generated resources are immutable after load, and which can be modified? This affects ownership, caching, invalidation, and tests.

## G. Content schemas and formulas — medium/high priority

### Q-025 — How strict should data-driven content schemas be in the first playable?

Which content is data-authored versus compiled code: items, recipes, spells, statuses, Gambits, dungeon rooms, buildings, resources, enemies, effects, and reward budgets?

### Q-026 — What expression model powers constrained spell formulas?

Do we want an AST/expression language, declarative operation graph, pre-approved formula primitives, or another bounded representation? What invalid-formula behavior is required?

### Q-027 — Are official and future-editor content formats identical from the first implementation?

The GDD prefers shared formats where practical, but the moderation/publishing editor is not first-version scope.

## H. Testing and visual validation — high priority

### Q-028 — Is the GDD's visual-validation prerequisite the actual first milestone?

**Status:** Resolved — first implementation milestone is the end-to-end voxel terrain pipeline; see DECISIONS/DR-009-FIRST-VOXEL-TERRAIN-MILESTONE.md and EPICS/EP-001-voxel-terrain-foundation/.

The GDD recommends unified voxel visual validation before broad gameplay production. Do we adopt that ordering, or is there an earlier architecture/test milestone?

### Q-029 — What is the golden-image platform policy?

Which OS/GPU/software-renderer configuration produces approved references? Do references need to be stable across Windows/Linux, or only on a dedicated deterministic renderer environment?

### Q-030 — What image comparison policy is acceptable?

Exact pixel equality, thresholded per-pixel comparison, perceptual metric, masked regions, or different policies per renderer/test type?

### Q-031 — What performance evidence should the first visual milestone capture?

The GDD asks for performance measurements but does not define acceptable structural metrics or benchmark methodology.

## I. Backlog taxonomy and workflow

### Q-032 — Do you want Story as a separate backlog level?

**Status:** Resolved — no mandatory Story layer; Epic directly owns ST-XXX-YY implementation tickets. See DECISIONS/DR-010-BACKLOG-GRANULARITY.md.

The supplied planning rules define Epics and detailed ST-XXX-YY implementation tickets. Should Story be synonymous with an implementation ticket, a capability slice between Epic and implementation Ticket, or not used as a distinct level?

No separate Story hierarchy is active until this is answered.

### Q-033 — What branch will receive the completed backlog?

The repository currently has master and no main branch. Should the eventual backlog PR target master, or do you intend to create/rename to main first?

### Q-034 — How much long-term roadmap should be materialized initially?

**Status:** Resolved — keep planning near-term and just-in-time; do not detail far-future Epics prematurely. See DECISIONS/DR-010-BACKLOG-GRANULARITY.md.

Should we create high-level capability/roadmap coverage for the whole GDD but detail only near-term Epics; only the first-playable roadmap initially; or another horizon model?


## J. EP-001 voxel terrain foundation — immediate blockers

### Q-035 — What is the first terrain voxel/cell representation?

**Status:** Direction resolved — packed 32-bit `Voxel::Cell` plus generic `Voxel::Volume`; exact canonical-empty/storage/editor/wire details remain open. See DECISIONS/DR-012-PACKED-CELL-AND-VOLUME-DIRECTION.md.

EP-001 needs an exact shared contract for one terrain cell and one 16×16×16 Chunk.

Questions include:

- does each occupied cell reference a voxel Definition ID, or directly store Shape/material properties;
- how is empty/air represented;
- how is Shape orientation represented;
- what information must be sufficient for Client meshing without Server-only state;
- which representation belongs in Core and crosses the network unchanged versus through a serialized DTO.

This must be resolved before the shared Chunk representation ticket can become Ready.

### Q-036 — Does the Client own all terrain meshing, and what neighbor data may meshing require?

**Status:** Partially resolved — Server never emits terrain meshes and Client owns meshing/rendering; missing-neighbor/remesh policy remains open. See DECISIONS/DR-013-CLIENT-TERRAIN-MESHING.md.

The current Epic assumes Server sends canonical voxel/Chunk data and Client produces render meshes.

Confirm whether:

- Server never sends render meshes for ordinary terrain;
- Client mesher may require neighboring Chunk/cell data to correctly remove faces or resolve shapes at boundaries;
- a Chunk may be rendered temporarily before all neighbors arrive, or must wait for required neighbor information.

### Q-037 — What networking transport and serialization/framing should EP-001 use?

**Status:** Mostly resolved — use Sparkle Version-0.1.3 networking and friend `spk::Message` insertion/extraction operators declared directly on `Voxel::Volume`, giving `message << volume` / `message >> volume` syntax. The operators serialize Volume logical state, not the C++ object representation. See DECISIONS/DR-016-SPARKLE-NETWORK-NODE-ROUTER.md and DR-017-VOXEL-VOLUME-MESSAGE-SERIALIZATION.md.

One portability detail remains open: whether Erelia requires a fixed endian/platform-independent scalar wire representation, or whether Sparkle/native representation compatibility across the project's supported Client/Server platforms is sufficient for the first milestone.

### Q-038 — What are the first Chunk request/streaming semantics?

**Status:** Partially resolved — requests are batched lists of Chunk coordinates; Client alone owns its view/loading radius policy. Duplicate outstanding request, cache/eviction, and partial-response details remain open. See DECISIONS/DR-014-BATCHED-CHUNK-PROTOCOL-DIRECTION.md.

For the inspection milestone, define:

- how the Client selects Chunks around its inspection position;
- initial request radius/shape or whether it is configurable;
- whether requests are one-Chunk-at-a-time or batched;
- whether duplicate outstanding requests are suppressed;
- Client cache/eviction expectations for the milestone;
- response behavior for unavailable/invalid coordinates.

### Q-039 — What exact basic terrain generator should be the first deterministic fixture?

**Status:** Direction resolved — flat baseline + X=0/Z=0 walls + elevated stairs/slabs/slopes across orientations/flips; exact fixture coordinates/Definitions remain open. See DECISIONS/DR-015-FIRST-TERRAIN-VALIDATION-SCENE.md.

The first generator should be intentionally simple, but tests need exact expected terrain.

Candidate examples include:

- flat plane at a fixed height;
- flat layers with one or more materials;
- deterministic height field from a seed;
- another deliberately small fixture.

The chosen generator and exact fixture values must be explicit before its implementation ticket becomes Ready.


### Q-040 — Should EP-001 use NodeRouter from the first Server implementation?

**Status:** Resolved — router-first from EP-001, with one terrain `LocalNode`; see DECISIONS/DR-016-SPARKLE-NETWORK-NODE-ROUTER.md and ARCHITECTURE/ARCH-004-SERVER-NODE-ROUTING.md.

Long-term direction is resolved: the dedicated Server is planned as a router to logical nodes responsible for coherent subsections/families of the game, using Sparkle networking.

EP-001 uses `spk::NodeRouter` immediately and routes Chunk request message types to one in-process terrain `spk::LocalNode`. Later capability families may add more LocalNodes, and selected nodes may become `RemoteNode` endpoints when justified.

Important limitation: Sparkle NodeRouter currently routes by `Message::Type`, not by World/Region/instance key. Capability-family routing fits directly; future sharding within one message family would require dispatch inside the owning node or a later routing extension.
