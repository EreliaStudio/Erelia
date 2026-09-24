# OD-026 — Initial Palette element, resolver context, and render-command lifetime

## Status

Resolved

## Supersession note

[OD-027](OD-027-render-command-palette-trust-boundary.md) supersedes only this record's Palette-pointer and render-command compatibility-validation decisions. The RGBA payload, resolver context, SSBO composition/View, explicit upload, Palette value-copy ownership, and deferred-lifetime decisions remain active.

## Problem

ST-003-02 fixed the per-Palette SSBO and flat integer mesh/shader contract, but three public details remained unspecified: the initial Palette element payload, the information available to `MaterialResolver`, and how a deferred render command preserves its Palette resource.

## Affected epics/tickets

[EP-003](../epics/EP-003-runtime-rendering-mesh-cache/EP-003-runtime-rendering-mesh-cache.md), [ST-003-02](../epics/EP-003-runtime-rendering-mesh-cache/tickets/ST-003-02-palette-resource-binding-and-voxel-shader-contract.md), and [ARCH-007](../architecture/ARCH-007-VOXEL-MESH-PALETTE-ASSEMBLY.md).

## Known constraints

- OD-018 leaves effects beyond base Palette color open.
- Shape-to-WorldPalette precedence remains outside ST-003-02.
- `MaterialResolver` is an independent mesher input and must not select or own a Palette.
- Sparkle GPU resources use value-copy handles with shared underlying resource ownership.
- Render snapshots execute after their commands are constructed, so non-owning resource lifetime cannot be assumed.

## Options considered

- Store only an RGBA base color in each initial Palette element, or introduce a broader Material object immediately.
- Give `MaterialResolver` only `materialSlot` and `outerSide`, or a complete immutable emission context.
- Let a command retain a strong Palette value copy, or keep a weak/non-owning reference and fail if the owner releases it.

## Decision

- The initial public payload is `Palette::Data { spk::Color color; }`. `Palette` owns its SSBO by composition and retains a typed `ShaderStorageBuffer::View<void, Data>` over that resource's CPU-side bytes. No effect field governed by OD-018 is introduced.
- `MaterialResolver` receives an immutable context containing the source volume, Definition, transformed emitted polygon, cell coordinate, and packed Cell. It returns only `paletteElementIndex`; it does not define slot/side precedence or select a Palette.
- `VoxelRenderCommand` accepts a Palette pointer so missing input can be rejected, then stores the validated Palette by value. Copying a Palette uses Sparkle's shared GPU-resource ownership, so the exact SSBO remains alive through deferred command execution. **Superseded by OD-027:** the command now accepts `const Palette&` and retains the value copy without validation.
- Empty Palette data and any mesh element index outside `[0, Palette::size())` are rejected before a draw can be submitted. **Superseded by OD-027:** compatibility is now a caller precondition and is not checked by the command.
- Mutable `Palette::data(index)` access returns a reference into the SSBO view. The caller explicitly invokes `Palette::validate()` after an edit; mutation does not upload automatically.

## Rationale

RGBA is the smallest payload that implements the approved base-color lookup without pre-empting later Material-effect decisions. Full immutable resolver context supports Chunk slot/side logic and later authored model inputs without choosing content precedence. A Palette value copy follows Sparkle's established resource semantics and prevents a valid deferred command from becoming dangling or silently falling back to previously bound state.

## Consequences

- Palette copies identify the same underlying SSBO resource; distinct Palette constructions remain independently bindable resources.
- Later element fields require an explicit OD-018 resolution and compatible GPU-layout migration.
- Later content-schema work supplies concrete resolvers and precedence; ST-003-02 test resolvers are fixture-local and do not freeze production mapping.
- A command cannot observe caller-side Palette destruction after successful construction because its stored copy retains the resource.
- [Sparkle issue #5](https://github.com/EreliaStudio/Sparkle/issues/5) tracks making `ShaderStorageBuffer` inheritable. Palette remains composition-based until that dependency is deliberately implemented and released.

## Validation / evidence

- ST-003-02 Palette, VoxelMesh, mesher, command-validation, and Windows/OpenGL tests pass in [CI run 35538271204](https://github.com/EreliaStudio/Playground/actions/runs/35538271204).
- The command-lifetime fixture constructs a command from a temporary Palette and retains it after the caller's Palette object leaves scope.
- Palette candidate captures exercise two independently constructed model Palettes and one Chunk WorldPalette.
- Existing approved textured references remain unchanged; Palette candidates in artifacts `10614200309` and `10613791231` await owner review.

## Resolution provenance

Resolved by the project owner during ST-003-02 execution on 20 September 2026: RGBA base color, full immutable resolver context, Palette value-copy storage using Sparkle's implicit shared GPU-resource ownership, SSBO composition with a typed `View`, and explicit validation after mutable edits.
