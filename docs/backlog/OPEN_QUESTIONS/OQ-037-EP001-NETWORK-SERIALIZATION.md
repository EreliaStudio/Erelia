# OQ-037 — What networking transport and serialization / framing should EP-001 use?

**Status:** Resolved
**Decision records:** [DR-016](../DECISIONS/DR-016-SPARKLE-NETWORK-NODE-ROUTER.md), [DR-017](../DECISIONS/DR-017-VOXEL-VOLUME-MESSAGE-SERIALIZATION.md)
**Affected areas:** EP-001 networking, Core serialization

## Question

What networking transport and shared Volume serialization should EP-001 use?

## Chosen solution

Use Sparkle Version-0.1.3 networking. No additional networking library is introduced.

`Voxel::Volume` uses the Sparkle Message Writer/Reader model:

```cpp
spk::Message::Writer writer;
writer << volume;

spk::Message message = std::move(writer).build();
auto reader = message.reader();
reader >> volume;
```

`Volume(const spk::Message&)` remains the convenience decode constructor and internally creates a Reader.

The Volume payload is:

1. `spk::Vector3UInt dimensions`;
2. `Voxel::Volume::UnitSize unitSize`;
3. one contiguous native Cell block containing exactly the checked dimension product.

No explicit Cell count is serialized.

The selected representation is Sparkle-native rather than a separate Erelia endian/floating-point format.

The canonical empty representation is `{0,0,0}`, unit size `0.0f`, and no Cells. Invalid dimensions/unit size, overflow, or truncated input throw `spk::Exception`.

Decode reconstructs a complete temporary Volume before replacing the destination. Failure leaves the destination unchanged. Reader cursor advancement belongs only to the Reader used for that parse; independent Readers over the same immutable Message do not share cursor state.

Decoded Volume storage is independent from the source Message lifetime. Trailing bytes are permitted because Volume is embeddable inside larger payloads.

## Sparkle Message clarification — 2026-10-01

The original OQ-037 wording described insertion/extraction directly on mutable `spk::Message`. The merged Sparkle Version-0.1.3 Message redesign supersedes that API shape.

Finalized Messages are immutable; `Message::Writer` owns construction and `Message::Reader` owns decoding cursor state over shared pooled payload storage.

The logical Volume bytes and validation semantics selected by OQ-037/DR-017 are unchanged.

## Remaining ambiguity

None for the shared Volume codec.

Higher-level Collection/Chunk/Column MessageIDs, correlation, batching, Response sectioning, retry/cache policy, and routing are owned by their dedicated tickets. ST-001-11 now owns the current generic Collection protocol.
