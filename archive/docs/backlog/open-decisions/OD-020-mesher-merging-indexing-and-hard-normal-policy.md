# OD-020 — Mesher merging, indexing, and hard-normal policy

## Status

Resolved

## Problem

The project requires an explicit choice for mesher merging, indexing, and hard-normal policy. Leaving it implicit would force an implementation ticket to invent design, balance, content, or technology policy.

## Affected epics/tickets

[EP-032](../epics/EP-032-unified-voxel-mesher/EP-032-unified-voxel-mesher.md) and [ST-032-04](../epics/EP-032-unified-voxel-mesher/tickets/ST-032-04-optimization-and-indexed-output.md).

## Known constraints

- Do not contradict the current GDD or the fixed contracts stated in affected tickets.
- Work independent of this choice may proceed behind configuration or an interface.
- No arbitrary value or technology is selected merely to unblock implementation.

## Options considered

- Preserve emitted polygons and add deterministic compatible-vertex reuse only.
- Merge adjacent coplanar faces immediately, including a greedy-meshing variant.
- Keep one vertex per authored polygon corner.
- Share vertices by position alone and recalculate or smooth their normals.
- Use approximate or quantized vertex compatibility.

## Decision

- Do not merge adjacent polygons. Preserve every polygon currently emitted by `VoxelMesher`. Face or greedy merging may be reconsidered only if later profiling identifies polygon count or mesh submission as a meaningful bottleneck; this decision defines no arbitrary performance threshold.
- Reuse a vertex only when every attribute currently stored in `spk::Texture3DVertex` is exactly equal: position, normal, and atlas UV. Material/UV seams and different normals therefore produce distinct vertices.
- When palette-element indices are introduced by ST-003-02, they also participate in compatibility. OD-020 does not introduce Palette data or change the current textured representation.
- Use exact stored-value equality. Do not use an epsilon, quantization, or position-only key.
- Preserve deterministic traversal and first-seen indexing: cells, polygons, and polygon vertices keep their established order; a compatible vertex receives its index on its first encounter; lookup-container iteration never determines output order.
- Preserve the stored hard, flat normal of every authored or occlusion-clipped Shape polygon. Do not average, interpolate, recalculate, or smooth normals across polygon boundaries. Polygons share a vertex only when their stored normals are already exactly equal.

## Rationale

Compatible-vertex reuse reduces redundant vertex storage without changing polygon topology, winding, UV seams, or the hard-edged voxel appearance. Deferring polygon merging keeps this checkpoint structural and evidence-driven: a more invasive optimization is not justified until profiling identifies a relevant bottleneck. Exact equality makes compatibility auditable and prevents an approximation policy from silently changing rendering semantics.

## Consequences

- ST-032-04 may add one deterministic vertex lookup to the shared `VoxelMesher` output path used by generic volumes/models and `Chunk::Mesher`.
- Index counts, triangle order, winding, Shape transforms, visibility, clipping, atlas selection, and rendered output remain unchanged. Only compatible duplicate vertex records and their referenced index values may change.
- Hard-normal and UV/material boundaries remain explicit in the vertex buffer.
- No face-merging implementation or threshold is introduced. Later profiling may motivate a separate project-owner decision and ticket.
- ST-003-02 remains responsible for Palette resources, palette-element data, binding, validation, and shader contracts.

## Validation / evidence

- Focused GoogleTests prove exact position/normal/UV compatibility, hard-normal separation, UV-seam separation, deterministic first-seen output, shared generic/Chunk behavior, and rejection recovery.
- Existing ST-032-01 semantic snapshots and ST-032-02 scale, transform, clipping, cache, and cross-Chunk fixtures remain the semantic baseline. Vertex-count assertions may change only where exact compatible reuse is demonstrated; index counts remain unchanged.
- All 20 original Chunk references and all 12 consolidated-mesher references remain unchanged under OD-024's `640 × 480` Windows/OpenGL comparison.
- Before/after vertex and index counts are recorded in ST-032-04 completion evidence. Timing evidence is recorded only where the environment permits reliable measurement; no unstable timing assertion or performance claim follows from vertex-count reduction alone.

## Resolution provenance

Resolved by the project owner in the ST-032-04 implementation instruction on 20 September 2026. The approved policy is: no polygon merging, exact position/normal/UV compatible-vertex reuse with deterministic first-seen indices, and preservation of authored/clipped hard flat normals. Implementation and CI links are recorded by ST-032-04.
