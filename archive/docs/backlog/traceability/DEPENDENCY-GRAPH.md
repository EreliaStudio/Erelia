# Dependency Graph

## Unified voxel / visual foundation

```text
EP-000 Test Harness
   ↓
EP-001 Unified VoxelVolume/Cell/Scale contract
   ├──────────────→ EP-032 Unified VoxelMesher + OcclusionResolver
   │                       ├──────────→ EP-004 Chunk world adaptation
   │                       └──────────→ EP-003 Mesh runtime/materials
   │
   ├──────────────→ EP-029 Content contracts
   │                       ↓
   └────────────────→ EP-030 External voxel import
                               ↓
                         EP-002 VoxelModel/assemblies/cache semantics
                               ↓
                         EP-031 Articulated characters

EP-003 + EP-004 + EP-030 + EP-031 + EP-032
                     ↓
        EP-033 / VS-000 visual validation
```

## World / gameplay foundation

```text
EP-001 + EP-032
      ↓
    EP-004 World runtime → EP-005 Traversal
      ↓                     ↓
    EP-008 Exploration → EP-009 Followers/detection
                              ↓
                           EP-010 Encounter lifecycle

EP-006 Clocks → EP-010 → EP-011 → EP-012
                            ↓       ↓
                         EP-015 ← EP-014/013
                            ↓
                         EP-016 → EP-017
```

## Persistence / frontier / economy

```text
EP-024 Authority/Persistence
  ├→ inventory/economy/shared world state
  └→ EP-028 networking later

EP-018 → EP-019 → EP-020 → EP-021
EP-021 → EP-022
EP-023 + EP-022 → EP-025
EP-018 + EP-024 → EP-026 → EP-025 reward procurement
```

Dependencies are contract relationships, not a mandatory waterfall sequence.
