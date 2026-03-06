# Battle.Data README

## Purpose
Data contains the serializable containers used to share battle runtime state through `Erelia.Core.Context`.
These objects hold the board, encounter table, and derived info used during setup and placement.

## Contents
- `Data`: holds `Board` and `EncounterTable` for the current battle.
- `Unit`: wrapper for a creature instance, its occupied cell, and its spawned view.

## Adding Or Extending
1. Add new fields to `Data` when battle systems need shared state.
2. Initialize new fields in constructors or setup phases to keep defaults safe.
3. Update loaders or phases that read the new fields.
