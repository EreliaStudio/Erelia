# OD-027 — Render-command Palette trust boundary

## Status

Resolved

## Problem

The initial ST-003-02 implementation made `VoxelRenderCommand` scan every mesh vertex during construction to verify that its `paletteElementIndex` fit the supplied Palette. That repeated compatibility work at the draw-command boundary, where render inputs are expected to be complete and valid.

## Affected epics/tickets

[EP-003](../epics/EP-003-runtime-rendering-mesh-cache/EP-003-runtime-rendering-mesh-cache.md), [ST-003-02](../epics/EP-003-runtime-rendering-mesh-cache/tickets/ST-003-02-palette-resource-binding-and-voxel-shader-contract.md), [ARCH-007](../architecture/ARCH-007-VOXEL-MESH-PALETTE-ASSEMBLY.md), and the render-command portion of [OD-026](OD-026-initial-palette-element-resolver-and-command-lifetime.md).

## Known constraints

- `VoxelRenderCommand` must retain a Palette value copy so the shared GPU resource outlives deferred execution.
- One cached mesh may be drawn with multiple compatible Palettes, so `VoxelMesher` does not own or receive the selected Palette.
- The shader contract currently receives Palette element data but no Palette length.
- Missing Palette input can be excluded by the C++ type system without a runtime check.

## Options considered

- Keep scanning every mesh vertex in `VoxelRenderCommand`.
- Couple `VoxelMesher` to a specific Palette and validate resolver output while building the mesh.
- Pass Palette length to the shader and define shader-side out-of-range behavior.
- Perform no compatibility validation in the mesher, shader, or render command; require valid prepared render inputs.

## Decision

- `VoxelRenderCommand` accepts `const Palette&`, copies it for deferred lifetime, and performs no empty-Palette or mesh-index compatibility validation.
- A mesh/Palette pair whose referenced indices do not exist is outside the render-command contract. Preparing a compatible pair is the caller's responsibility.
- `VoxelMesher` remains independent of Palette selection and size. It does not validate the resolver's element index against a Palette.
- The shader receives no Palette length and adds no fallback or clamping behavior.
- `Palette::data(index)` retains its own checked CPU editing boundary; this decision removes only cross-resource validation from `VoxelRenderCommand`.

## Rationale

Render commands are the final submission boundary and should consume already-valid data without rescanning immutable mesh bytes. A reference makes the required Palette binding explicit, while the retained value copy preserves the previously selected deferred-lifetime behavior. Mesher-side validation would incorrectly bind reusable geometry to one Palette, and shader-side fallback would add rendering semantics that were not requested.

## Consequences

- Constructing a command is constant with respect to mesh vertex count.
- Invalid Palette indices can produce an invalid GPU SSBO access; callers must not submit incompatible mesh/Palette pairs.
- No render-command rejection or fallback test exists for missing, empty, or out-of-range Palette data.
- The prior OD-026 pointer-and-validation decision is superseded; its RGBA payload, resolver context, SSBO composition/View, explicit upload, value-copy ownership, and lifetime decisions remain active.

## Validation / evidence

- The `_checkedPalette` mesh scan and its rejection tests are removed.
- All call sites pass a Palette reference.
- The deferred-lifetime test still proves that the command's Palette value copy retains the shared GPU resource.
- Palette golden tests exercise compatible Human, Orc, and WorldPalette bindings through the simplified command.
- [CI run 35572632309](https://github.com/EreliaStudio/Playground/actions/runs/35572632309) passes the CPU/headless and Windows/OpenGL golden-test lanes with this trust boundary.

## Resolution provenance

Resolved by the project owner on 21 September 2026: remove render-command compatibility verification entirely and treat valid Palette bindings and element indices as a caller precondition.
