# Open External Requests

This file tracks external dependency changes requested by Erelia when the Erelia codebase already contains a temporary workaround or an implementation that should be simplified after the dependency change lands.

An entry should record:

- the external issue or request;
- why Erelia is affected;
- the current Erelia workaround;
- the exact Erelia changes to revisit after the external issue is resolved;
- the validation required before closing the entry.

## Open requests

### OR-001 — Sparkle TVector3 JSON constructor must not be noexcept

**Status:** Open  
**Dependency:** Sparkle  
**External issue:** https://github.com/EreliaStudio/Sparkle/issues/14  
**Affected Erelia ticket:** ST-001-04  
**Affected Erelia code:** `core/src/voxel/shape.cpp`

Sparkle `TVector3(const JSON::Value&)` is currently declared `noexcept` while delegating to `TVector3::fromJSON()`, which can throw for malformed JSON. Using that constructor directly on authored resource data can therefore terminate the process instead of propagating a parse failure.

Erelia currently avoids that constructor in Shape loading and calls:

```cpp
spk::Vector3::fromJSON(reader.value())
```

inside a `try` / `catch`, then rethrows through `spk::JSON::throwAt` so the Erelia error retains file/path context.

When Sparkle issue #14 is resolved and Erelia adopts a Sparkle version containing the fix:

1. verify that `TVector3(const JSON::Value&)` is no longer incorrectly `noexcept`;
2. replace the direct `spk::Vector3::fromJSON(reader.value())` workaround in `Shape::_loadVertex` with construction through the normal Sparkle JSON constructor;
3. retain Erelia's source-location wrapping through `spk::JSON::throwAt` unless the adopted Sparkle API itself provides equivalent file/path diagnostics;
4. keep the existing normalized-range validation and integer quantization unchanged;
5. run the malformed-vertex tests and the full CI matrix, verifying malformed vector JSON throws instead of terminating;
6. update this entry to Resolved, recording the Sparkle version/commit adopted by Erelia.
