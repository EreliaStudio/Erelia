# Battle.Player README

## Purpose
Player contains battle player controls, hover/selection logic, and input routing.
It binds input actions to board interaction and forwards confirm/cancel to the active phase controller.

## Contents
- `Presenter`: moves the player transform relative to the battle camera.
- `Model`: placeholder for battle player state.
- `View`: exposes the linked camera transform.
- `BattlePlayerController`: hover masks and confirm/cancel input routing for the active phase.
- `MouseBoardCellSelection`: hover-to-selection mask helper.
- `Camera`: battle camera orbit/zoom and board cursor utilities.

## Adding Or Extending
1. Add new input-driven behaviors as new components in this folder.
2. Use `BattlePlayerController` or `MouseBoardCellSelection` as reference for mask updates.
3. Keep camera-related logic in `Player/Camera` to avoid circular dependencies.
