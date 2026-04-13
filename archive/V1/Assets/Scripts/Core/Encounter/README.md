# Encounter README

## Purpose
Encounter defines how random encounters are configured and selected.
It is used by exploration to trigger battles and by battle setup to build boards.

Core flow:
1. Create one or more `EncounterTable` JSON files.
2. Register them in the encounter registry JSON (id -> path).
3. Load the registry via `EncounterTableRegistry`.
4. Use the resolved table at runtime to drive placement and selection.

## EncounterTable
`EncounterTable` stores encounter parameters:
- `EncounterChance` (0..1)
- `BaseRadius`, `NoiseAmplitude`, `NoiseScale`, `NoiseSeed`
- `Teams` (weighted list of team paths)

Example JSON:
```json
{
  "EncounterChance": 0.25,
  "BaseRadius": 10,
  "NoiseAmplitude": 4,
  "NoiseScale": 0.15,
  "NoiseSeed": 1337,
  "Teams": [
    { "TeamPath": "Encounters/Teams/Forest_Team_A.json", "Weight": 70 },
    { "TeamPath": "Encounters/Teams/Forest_Team_B.json", "Weight": 30 }
  ]
}
```

## EncounterTableRegistry
The registry maps an integer id to a table path.
It is loaded from a JSON file (default resource: `Resources/Encounter/EncounterRegistry`).

Registry JSON format:
```json
{
  "Encounters": [
    { "Id": 1, "Path": "Encounters/Tables/Forest.json" },
    { "Id": 2, "Path": "Encounters/Tables/Desert.json" }
  ]
}
```

## Usage
```csharp
// Load registry (from Resources by default):
EncounterTableRegistry.LoadFromResources(null);

// Resolve a table:
if (EncounterTableRegistry.TryGetTable(1, out EncounterTable table))
{
    // use table
}
```

## Authoring Workflow
1. Create a new encounter table JSON file.
2. Add it to the registry JSON with a unique id.
3. Ensure each table references valid Team JSON paths.

## Notes
- Table paths are resolved via `PathUtils` and `JsonIO`.
- Duplicate registry ids are ignored (first wins, warning logged).
