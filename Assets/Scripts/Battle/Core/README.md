# Battle.Core README

## Purpose
Core contains the battle state machine entry point.
`BattleManager` owns the active phase and applies transitions safely.

## Contents
- `BattleManager`: drives the current phase, handles transitions, and ticks the flow.

## Adding Or Extending
1. Add a new id to `Assets/Scripts/Battle/Phase/Id.cs`.
2. Create a new phase in `Assets/Scripts/Battle/Phase/<Name>`.
3. Register the new phase in `Assets/Scripts/Battle/Phase/Registry.cs`.
4. Request the new phase from existing phases or `BattleManager` as needed.
