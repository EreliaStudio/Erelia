# OQ-003 — What may Core depend on?

**Status:** Resolved
**Decision records:** [DR-002](../DECISIONS/DR-002-SPARKLE-CORE-DEPENDENCY.md)
**Affected areas:** Core dependency policy, Sparkle

## Question

What may Core depend on?

## Problem / context

Core already links Sparkle Core. The dependency needed explicit approval because removing it would require adapters around basic math/network/foundation types, while allowing unrestricted Sparkle dependencies could make headless code depend on graphics.

## Known constraints

- Core must remain usable by the headless Server.
- Sparkle Core already provides shared math/foundation functionality needed by Erelia.
- Graphical Sparkle dependencies must not leak into headless Core.

## Possible solutions

1. Keep Core entirely independent from Sparkle.
2. Allow direct dependency on headless-safe Sparkle Core only.
3. Allow Core to depend on the full graphical Sparkle library.

## Chosen solution

Core may depend directly on headless-safe Sparkle Core. Graphical/presentation Sparkle dependencies remain outside Core.
