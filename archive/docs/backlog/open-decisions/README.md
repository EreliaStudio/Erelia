# Open Decisions

Decision records are persistent architecture/design history. Resolve a decision in place; never delete it after resolution.

| ID | Status | Decision |
|---|---|---|
| [OD-001](OD-001-exact-hero-combat-statistic-set-and-defaults.md) | Open | Exact Hero/combat statistic set and defaults. |
| [OD-002](OD-002-equipment-stat-budget-and-generated-item-distribution.md) | Open | Equipment stat-budget and generated-item distribution. |
| [OD-003](OD-003-spell-instance-variable-properties.md) | Open | Spell-instance variable properties. |
| [OD-004](OD-004-durability-damage-repair-and-maximum-durability.md) | Open | Durability damage, repair, and maximum durability. |
| [OD-005](OD-005-defense-and-critical-formulas.md) | Open | Defense and critical formulas. |
| [OD-006](OD-006-elastic-combat-area-and-flee-tuning.md) | Open | Elastic combat area and flee tuning. |
| [OD-007](OD-007-building-catalog-costs-and-service-mapping.md) | Open | Building catalog, costs, and service mapping. |
| [OD-008](OD-008-world-count-cadence-and-content-progression.md) | Open | World count, cadence, and content progression. |
| [OD-009](OD-009-marketplace-fair-price-and-outlier-procurement-policy.md) | Open | Marketplace fair-price and outlier procurement policy. |
| [OD-010](OD-010-pvp-design-outside-the-baseline.md) | Open | PvP design outside the baseline. |
| [OD-011](OD-011-c-ownership-and-view-design-for-voxelvolume.md) | Resolved | Concrete vector-owning VoxelVolume base with checked reads, one-notification batched editing/versioning, and Sparkle diagnostics. |
| [OD-012](OD-012-first-external-voxel-authoring-tool-and-interchange-format.md) | Open | First external voxel authoring tool and interchange format. |
| [OD-013](OD-013-authoritative-collision-primitive-for-articulated-entities.md) | Open | Authoritative collision primitive for articulated entities. |
| [OD-014](OD-014-fixed-simulation-tick-rates.md) | Open | Fixed simulation tick rates. |
| [OD-015](OD-015-persistence-backend-and-cloud-topology.md) | Open | Persistence backend and cloud topology. |
| [OD-016](OD-016-network-transport-and-library.md) | Open | Network transport and library. |
| [OD-017](OD-017-rich-shape-mapping-metadata-for-imported-assets.md) | Open | Rich Shape mapping metadata for imported assets. |
| [OD-018](OD-018-first-material-effects-beyond-palette-color.md) | Open | First Material effects beyond palette color. |
| [OD-019](OD-019-palette-variation-algorithm-and-sampling-declaration.md) | Open | Palette variation algorithm and sampling declaration. |
| [OD-020](OD-020-mesher-merging-indexing-and-hard-normal-policy.md) | Resolved | No polygon merging; exact position/normal/UV compatible-vertex reuse with deterministic first-seen indices and hard flat normals. |
| [OD-021](OD-021-serialized-derivative-mesh-cache-versus-rebuild-on-load.md) | Open | Serialized derivative mesh cache versus rebuild on load. |
| [OD-022](OD-022-assembly-anchor-and-animation-authoring-source.md) | Open | Assembly anchor and animation authoring source. |
| [OD-023](OD-023-whether-cell-orientation-and-flip-must-be-expanded.md) | Resolved | Preserve the existing packed Cell orientation and vertical flip until evidence requires a separate expansion decision. |
| [OD-024](OD-024-current-chunk-golden-fixtures-and-runner.md) | Resolved | Current Chunk golden fixtures, capture size, and canonical runner. |
| [OD-025](OD-025-voxelmesher-chunk-specialization-and-occlusion-cache.md) | Resolved | One generic VoxelMesher with cached occlusion and a Chunk::Mesher external-neighbor override. |
| [OD-026](OD-026-initial-palette-element-resolver-and-command-lifetime.md) | Resolved | RGBA Palette data, SSBO composition/view, explicit validation, full resolver context, and value-copy command lifetime. |
| [OD-027](OD-027-render-command-palette-trust-boundary.md) | Resolved | Render commands accept a Palette reference and trust prepared mesh/Palette compatibility without rescanning vertices. |
