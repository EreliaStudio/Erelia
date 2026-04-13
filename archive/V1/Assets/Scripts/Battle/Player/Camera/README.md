# Battle.Player.Camera README

## Purpose
Camera contains the battle camera orbit/zoom controls and mouse cursor raycasting
for selecting board cells.

## Contents
- `Presenter`: applies orbit and zoom input each frame.
- `Model`: serialized tuning values for orbit and zoom.
- `View`: exposes the camera transform.
- `MouseBoardCellCursor`: raycasts the board and emits hover events.

## Adding Or Extending
1. Add new camera tuning fields to `Model` and apply them in `Presenter`.
2. Extend `MouseBoardCellCursor` if you need different hover rules or tags.
3. Keep raycast filters and board tags in this folder to localize input concerns.
