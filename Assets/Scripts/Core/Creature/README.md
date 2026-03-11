# Creature README

## Purpose
Creature is the data layer that defines species and individual creature instances, and groups them into teams.
It is used by battle, encounter, and save systems.

Core flow:
1. A `Species` ScriptableObject defines the base data (unit prefab, display name).
2. `SpeciesRegistry` maps a stable integer id to a `Species`.
3. An instance `Model` stores the species id + per-instance data.
4. A `Team` is a fixed-size array of instance models.

## Species
- `Species` is a ScriptableObject asset.
- It holds the unit prefab and a display name.
- New species are authored as assets.

## SpeciesRegistry
- `SpeciesRegistry` is a singleton registry loaded from:
  - `Resources/Creature/SpeciesRegistry.asset`
- It provides two lookups:
  - id -> species (`TryGet`)
  - species -> id (`TryGetId`)
- Duplicate ids are ignored (first entry wins) and logged.
- `EmptySpeciesId = -1` means "no species".

## Instance Model
- `Erelia.Core.Creature.Instance.Model` stores:
  - `speciesId` (from `SpeciesRegistry`)
  - optional `nickname`
- `speciesId < 0` should be treated as "empty / invalid".

## Team
- `Team` is a fixed-size array of slots (default size is 6).
- Each slot can be:
  - a creature instance model
  - or `null` (empty slot)

## Serialization
- Creature data uses Unity `JsonUtility`.
- Teams and instances are serialized as JSON objects:
  - Team JSON contains `"slots"` array.
  - Instance JSON contains `"speciesId"` and `"nickname"`.
- File I/O is handled externally (see `Erelia.Core.Utils.JsonIO`).

## Authoring Workflow
1. Create a `Species` asset.
2. Assign its unit prefab and display name.
3. Add it to `SpeciesRegistry` with a unique id.
4. Use that id in creature instance models.

## Runtime Usage (examples)
```csharp
// Resolve a species from an instance model:
if (SpeciesRegistry.Instance.TryGet(model.SpeciesId, out Species species))
{
    // Use species.UnitPrefab, species.DisplayName, etc.
}

// Serialize a team:
JsonIO.Save(path, team, true);

// Load a team:
Team loaded = JsonIO.Load<Team>(path);
```

## Troubleshooting
- A creature does not appear:
  - Check the species id exists in `SpeciesRegistry`.
  - Check the species unit prefab is assigned.
- A team slot is empty:
  - Slot is `null`, or the model has `speciesId < 0`.
- Duplicate species ids:
  - Registry keeps the first and warns in the console.
