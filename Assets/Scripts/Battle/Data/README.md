# BattleState README

## Purpose
This folder currently holds `BattleState`, the serializable runtime object shared through `Erelia.Core.GameContext`.
It owns the board, enemy team, placed units, active unit, and derived state used during setup and combat flow.

## Contents
- `BattleState`: holds the board, enemy team, runtime units, deployment cells, and active-turn state.

## Adding Or Extending
1. Add new fields to `BattleState` when battle systems need shared runtime state.
2. Initialize new fields in constructors or setup phases to keep defaults safe.
3. Update loaders or phases that read the new fields.
