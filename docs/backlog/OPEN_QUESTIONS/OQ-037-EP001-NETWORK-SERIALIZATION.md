# OQ-037 — What networking transport and serialization / framing should EP-001 use?

**Status:** Partially resolved
**Decision records:** [DR-016](../DECISIONS/DR-016-SPARKLE-NETWORK-NODE-ROUTER.md), [DR-017](../DECISIONS/DR-017-VOXEL-VOLUME-MESSAGE-SERIALIZATION.md)
**Affected areas:** EP-001 networking, Core serialization

## Question

What networking transport and serialization / framing should EP-001 use?

## Problem / context

EP-001 needs concrete networking without adding external libraries. Sparkle now provides Client/Server/Message/NodeRouter APIs, and Volume should have a concise domain-level serialization API.

## Known constraints

- Use Sparkle Version-0.1.3 networking.
- No additional networking library.
- `Voxel::Volume` exposes friend `operator<<` / `operator>>` against `spk::Message`.
- The Volume object itself is not raw-memcopied because it owns a `std::vector`.

## Possible solutions

1. Use Sparkle's native object representation for scalar/trivially-copyable fields on supported platforms.
2. Define a fixed endian/platform-independent Erelia scalar wire representation on top of `spk::Message`.
3. Introduce a separate DTO/serialization layer despite the direct Volume operator API.

## Remaining ambiguity

The scalar byte-order/platform-compatibility policy is still open.

## Chosen solution

Use Sparkle networking and direct friend `spk::Message` operators on `Voxel::Volume`, serializing dimensions, voxel size and contiguous Cell data logically. Exact scalar wire portability policy remains unresolved.
