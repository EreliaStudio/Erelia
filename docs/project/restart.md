# Erelia v2 restart record

## Historical preservation

Repository: [EreliaStudio/Erelia](https://github.com/EreliaStudio/Erelia).

The authoritative final v1 snapshot is commit `7d46dad25dfb5bdf6f792479e993a8bced4e3aef`. The [`archive/erelia-v1` branch](https://github.com/EreliaStudio/Erelia/tree/archive/erelia-v1) references that commit. The historical implementation, tests, original CI and backlog remain available in the archive.

The owner intentionally did **not** require preservation of unmerged historical development branches. Old code remains reference material, not an approved basis for v2 implementation.

## V2 baseline strategy

By explicit owner direction, v2 uses **a single ordinary replacement commit based on the existing `master` history**, preserving ancestry, and updating `master` directly as a one-off transition. It does not use an orphan root. The v1 archive remains untouched.

The active baseline retains the [GDD](../gdd/Erelia_GDD_Current.md) and its 34 illustrations. Existing v1 architecture, app code, tests, CI, backlog and other historical implementation artifacts are **not imported**. Prior technical assumptions must be separately reviewed against the current Sparkle API before new M1 architecture is approved.

## Current approval gates

The independent methodology audit was reported to the owner as finding no substantial blockers. The consolidated M0 governance is reproduced as a **documentation baseline pending independent review and owner acceptance**. It is not automatically accepted because its files were committed.

There are no active v2 C++ tests, production code, accepted feature Stories or implemented traceability tools. M1 bootstrapping is a later phase requiring explicit owner-approved requirements, design, tests and verification.

## Repository-specific transition exception

The owner explicitly authorized the one-time direct `master` reset. This does not override the normal future Story delivery policy of one Story branch and PR, independent review and **owner-only manual merging**.

The requested archival tag `erelia-v1-final` is tracked separately as an operational follow-up if it has not been created; the independently verified archive branch and immutable commit SHA already preserve the exact snapshot.
