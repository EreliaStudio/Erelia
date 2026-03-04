# Exploration.Player README

## Purpose
Player contains exploration movement, player state, and encounter-related emitters.
It converts input into movement and emits events used by world streaming and encounters.

## Contents
- `Presenter`: moves the player based on input and camera orientation.
- `Model`: placeholder for exploration player state.
- `View`: provides the linked camera transform.
- `PlayerMotionEmitter`: emits cell-level movement events.
- `ChunkMotionEmitter`: emits chunk-change events.
- `EncounterTriggerEmitter`: checks encounter ids and requests battle scenes.

## Adding Or Extending
1. Add new player state fields in `Model` and bind them in `Presenter`.
2. Add new emitters here if other systems need movement-related events.
3. Keep encounter logic in `EncounterTriggerEmitter` so it stays centralized.
