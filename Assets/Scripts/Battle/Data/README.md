# Battle.Data README

## Purpose
Data contains the serializable containers used to share battle runtime state through `Erelia.Core.Context`.
These objects hold the board, encounter table, and derived info used during setup and placement.

## Contents
- `Data`: holds `Board`, `Info`, and `EncounterTable` for the current battle.
- `Info`: derived placement centers for player and enemy teams.

## Adding Or Extending
1. Add new fields to `Data` or `Info` when battle systems need shared state.
2. Initialize new fields in constructors or setup phases to keep defaults safe.
3. Update loaders or phases that read the new fields.
