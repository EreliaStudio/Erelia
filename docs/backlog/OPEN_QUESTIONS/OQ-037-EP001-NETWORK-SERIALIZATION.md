# OQ-037 — What networking transport and serialization / framing should EP-001 use?

**Status:** Resolved
**Decision records:** [DR-016](../DECISIONS/DR-016-SPARKLE-NETWORK-NODE-ROUTER.md), [DR-017](../DECISIONS/DR-017-VOXEL-VOLUME-MESSAGE-SERIALIZATION.md)
**Affected areas:** EP-001 networking, Core serialization

## Question

What networking transport and serialization / framing should EP-001 use?

## Problem / context

EP-001 needs concrete networking without adding external libraries. Sparkle provides Client/Server/Message/NodeRouter APIs, and `Voxel::Volume` needs a concise domain-level serialization API shared by Client and Server.

## Known constraints

- Use Sparkle Version-0.1.3 networking.
- No additional networking library.
- `Voxel::Volume` exposes friend `operator<<` / `operator>>` against `spk::Message`.
- The Volume object itself is not raw-memcopied because it owns a `std::vector`.
- `Voxel::Cell` is exactly 32 bits, trivially copyable, and Volume cell storage is contiguous in Y-fastest, then X, then Z order.

## Considered solutions

1. Use Sparkle's native object representation for scalar/trivially-copyable fields on supported platforms.
2. Define a fixed endian/platform-independent Erelia scalar wire representation on top of `spk::Message`.
3. Introduce a separate DTO/serialization layer despite the direct Volume operator API.

## Chosen solution

Use Sparkle networking and direct friend `spk::Message` operators on `Voxel::Volume`.

The Volume wire payload is, in this exact order:

1. `spk::Vector3UInt dimensions`, serialized through Sparkle's native trivially-copyable representation as one value;
2. `Voxel::Volume::UnitSize unitSize`, serialized through Sparkle's native representation;
3. one contiguous native block containing exactly `dimensions.x * dimensions.y * dimensions.z` `Voxel::Cell` values.

No explicit Cell-count field is serialized. The Cell count is derived from dimensions using checked multiplication.

The selected scalar/Cell wire policy is deliberately Sparkle-native rather than platform-independent. This accepts the same ABI/endianness/floating-point representation assumptions already made by Sparkle's native `Message` operators on the supported target platforms. Implementations should make the required native-layout assumptions explicit with compile-time checks where appropriate.

The canonical empty representation is exactly:

- dimensions `{0, 0, 0}`;
- unit size `0.0f`;
- no Cell bytes.

Any mixed-zero dimensions, non-empty dimensions with a non-finite or non-positive unit size, or empty dimensions with a non-zero unit size are malformed.

Both insertion and extraction validate the Volume contract. Invalid serialization input or malformed serialized input throws `spk::Exception`.

On extraction, metadata and derived byte counts are validated before allocating Cell storage. The destination `Voxel::Volume` is replaced only after a complete valid Volume has been reconstructed, so it remains unchanged if decoding fails. The `spk::Message` read cursor follows Sparkle's normal semantics and may already have advanced through successfully-read fields when a later extraction step fails.

A decoded Volume owns its storage independently of the source Message. Trailing Message bytes are allowed because Volume serialization is embeddable inside larger protocol messages.

## Remaining ambiguity

None for ST-001-05. Higher-level Chunk message kinds, routing, request/response semantics, retries and framing remain owned by their dedicated tickets/questions.

## Resolution provenance

Resolved with the project owner on 24 September 2026 while preparing ST-001-05. The owner selected Sparkle-native representation, whole-`spk::Vector3UInt` serialization, derived Cell count, contiguous Cell-block transfer, normal Sparkle cursor semantics on failure, and symmetric Volume invariant validation on both insertion and extraction.
