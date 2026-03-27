# Battle README

## Purpose
Battle contains the runtime systems for the tactical combat loop: board construction, phase flow, player input,
and battle-specific voxel overlays.

## Contents
- `BattleBoard`: board model, presenter, view, and board construction helpers.
- `Core`: battle phase state machine entry point.
- `BattleState`: serializable battle runtime state stored in `Erelia.Core.GameContext`.
- `Mask`: sprite registry for overlay masks.
- `Phase`: shared phase infrastructure and concrete battle flow phases.
- `Player`: player input, selection, and camera control for battles.
- `Setup`: scene loader that binds battle state to presenters.
- `Voxel`: battle voxel extensions and mask meshing.

## Adding Or Extending
1. Add new phases in `Phase` and register them in `Phase/Registry.cs`.
2. Add new board visuals or shared runtime fields in `BattleState` as needed.
3. Add new player interactions in `Player` and ensure the battle scene has the component wired.
