# DR-002 — Core may depend on Sparkle Core

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Applies to:** Core, dependency policy, Sparkle Core

## Context

Core currently links Sparkle Core. A choice was required between keeping Erelia's common library independent from Sparkle or allowing Core to directly use Sparkle's non-graphical foundational facilities.

The project expects shared needs such as Vector3, mathematics, random generation, noise, collision-related helpers, and future generic algorithms.

## Already-fixed constraints

- Core is the common library consumed by both Server and Client.
- Core must remain usable in headless Server builds.
- Graphical/presentation dependencies belong to Client rather than Core.

## Question

May Erelia Core directly depend on Sparkle Core?

## Options

### Option A — Sparkle-independent game Core

Keep Erelia Core independent and wrap Sparkle through adapters.

### Option B — Direct Sparkle Core dependency

Allow Core to use Sparkle Core foundational types and algorithms directly.

## Decision

Use **Option B**.

Erelia Core may directly depend on **Sparkle Core** for shared foundational functionality such as math/vector types and future generic algorithms.

This approval is specifically for dependencies suitable for both Server and Client. It does not authorize graphical/presentation-only Sparkle dependencies inside Core.

## Consequences

- Shared Erelia types and algorithms may expose or internally use Sparkle Core foundational types where appropriate.
- We do not require adapter layers solely to hide Sparkle Core from Erelia Core.
- Server must remain able to consume Core without pulling graphical Client dependencies.
- New Sparkle dependencies proposed for Core must be evaluated by whether they remain valid for headless/shared use.

## Required tests

- Headless Core and Server builds must continue to compile and test without graphical runtime requirements.
- CI must catch accidental introduction of Client/graphics-only dependencies into Core.
- Determinism and serialization tests must explicitly cover any Sparkle primitives used in public persistent/network contracts when those contracts are introduced.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22 while answering Q-003.

## Supersession

None.
