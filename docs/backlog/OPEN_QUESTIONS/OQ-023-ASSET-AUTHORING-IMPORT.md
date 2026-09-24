# OQ-023 — What is the first asset authoring / import format?

**Status:** Open
**Decision records:** None.
**Affected areas:** Voxel models, content pipeline, future editor

## Question

What is the first asset authoring / import format?

## Problem / context

Voxel models need dimensions, cells, Definitions, pivots/attachments and potentially animation metadata. The GDD does not select an external interchange format.

## Known constraints

- The future content editor is not first-version scope.
- The chosen format should not force graphical dependencies into headless voxel data.

## Possible solutions

1. Use a project-owned JSON/binary voxel format.
2. Import a common external voxel format plus Erelia sidecar metadata.
3. Use an editor-native format and export into a separate runtime format.

## Chosen solution

None yet. The first authoring/import format remains open.
