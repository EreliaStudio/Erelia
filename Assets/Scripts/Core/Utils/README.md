# Core.Utils README

## Purpose
Utility helpers used across core systems for path resolution and JSON I/O.

## PathUtils
`PathUtils.ReadTextFromPath` reads a text file from multiple supported locations.

Resolution order:
1. Absolute filesystem path (if the file exists).
2. Unity `Resources` (path without extension).
3. `StreamingAssets` (relative to `Application.streamingAssetsPath`).

If nothing is found, it returns `null`.

## JsonIO
Thin wrappers over `JsonUtility`:
- `Save<T>(path, data, prettyPrint)` writes a JSON file to disk.
- `Load<T>(path)` loads JSON from any location supported by `PathUtils`.
- `TryLoad<T>(path, out data)` returns `false` if missing/invalid, `true` otherwise.

Notes:
- `Save` always writes to the filesystem and will create the directory if needed.
- `Load`/`TryLoad` can read from filesystem, Resources, or StreamingAssets.

## Examples
```csharp
// Save a serializable object:
JsonIO.Save(path, data, true);

// Load (null/default if missing/invalid):
MyType loaded = JsonIO.Load<MyType>(path);

// Load safely:
if (JsonIO.TryLoad(path, out MyType loadedOk))
{
    // use loadedOk
}
```
