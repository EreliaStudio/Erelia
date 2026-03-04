# Battle.Player README

## Purpose
Player contains battle player controls, hover/selection logic, and placement input.
It binds input actions to board interaction and movement.

## Contents
- `Presenter`: moves the player transform relative to the battle camera.
- `Model`: placeholder for battle player state.
- `View`: exposes the linked camera transform.
- `BattlePlayerController`: placement input, hover masks, and creature placement.
- `MouseBoardCellSelection`: hover-to-selection mask helper.
- `Camera`: battle camera orbit/zoom and board cursor utilities.

## Adding Or Extending
1. Add new input-driven behaviors as new components in this folder.
2. Use `BattlePlayerController` or `MouseBoardCellSelection` as reference for mask updates.
3. Keep camera-related logic in `Player/Camera` to avoid circular dependencies.
