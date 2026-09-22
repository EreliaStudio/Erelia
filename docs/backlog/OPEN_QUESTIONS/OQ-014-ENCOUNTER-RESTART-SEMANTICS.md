# OQ-014 — Do active encounters survive a Server restart?

**Status:** Open
**Decision records:** None.
**Affected areas:** Encounter persistence, recovery, clocks

## Question

Do active encounters survive a Server restart?

## Problem / context

An active tactical encounter contains readiness, statuses, temporary entities and a pausable Encounter Clock. A Server restart needs a defined participant outcome.

## Known constraints

- Encounter Time is distinct from World and Real Time.
- Recovery must not duplicate rewards/actions or leave players stranded.

## Possible solutions

1. Persist and resume the exact encounter snapshot.
2. Abort active encounters on restart and return participants to a defined recovery state.
3. Persist only at safe encounter checkpoints and resume from the last committed checkpoint.

## Chosen solution

None yet. Whether encounters resume, reset or recover through checkpoints remains open.
