# AI Implementation Prompt — Specific Ticket

Use this prompt when you already know the implementation ticket to execute.

Replace `<TICKET_ID_OR_PATH>` before giving this prompt to the implementation agent.

---

You are implementing work in the Erelia repository.

Repository: `https://github.com/EreliaStudio/Erelia`

Implement this ticket and **only this ticket**:

> **<TICKET_ID_OR_PATH>**

## Mandatory reading before coding

Read these files first:

1. `docs/backlog/PROJECT-CONTEXT.md`
   - Understand the product/gameplay context.

2. `docs/backlog/IMPLEMENTATION-CONTEXT.md`
   - Pay particular attention to:
     - naming/type-organization taste;
     - Core / Server / Client ownership;
     - dependency policy;
     - networking/NodeRouter conventions;
     - voxel API/serialization conventions;
     - testing and small-ticket implementation style.

3. `docs/backlog/CURRENT-STATUS.md`
   - Verify that repository/backlog state is compatible with the requested ticket.
   - Note dependencies that were already completed and any known current blockers.

4. The complete requested ticket `<TICKET_ID_OR_PATH>`.

5. Its complete parent Epic document and that Epic's `tickets/README.md`.

6. Every DR listed by the ticket and every DR constraining the parent Epic.

7. Every Architecture document listed by the ticket/Epic that affects the work.

8. Every OQ linked from the ticket or relevant to its public contract.
   - Check `docs/backlog/OPEN_QUESTIONS/README.md`.
   - Read individual OQ files rather than relying only on their titles/status.

9. `docs/backlog/DEFINITION-OF-READY.md`.

10. `docs/backlog/DEFINITION-OF-DONE.md`.

Also inspect the current implementation and tests of all production targets named by the ticket.

Historical files under `archive/` are reference/inspiration only unless the active ticket or an explicit user instruction deliberately reintroduces a specific behavior.

## Readiness gate

Before changing production code, verify the requested ticket is marked **Ready**.

If it is Draft or Blocked, do not silently convert it to Ready and do not invent the missing contract.

If the ticket is marked Ready but contradicts an unresolved OQ, newer DR, Architecture document, or explicit repository state, stop and report the conflict before implementing that behavior.

A requested ticket must be implementable without inventing:

- public behavior;
- ownership;
- authority;
- lifecycle;
- serialization/network semantics;
- error behavior;
- ordering/determinism;
- fixtures/expected results.

## Implementation scope

Implement exactly the ticket's owned behavior.

Do not also implement the next ticket, a convenient future subsystem, or a broad refactor unless the ticket explicitly requires it.

Prefer a small reviewable patch over a large cross-layer change.

Follow `IMPLEMENTATION-CONTEXT.md`, including where applicable:

- semantic namespace/type names such as `Voxel::Volume` rather than duplicated prefixes;
- standard library + Sparkle dependency preference;
- Server authority / Core shared-tools / Client presentation ownership;
- Sparkle `NodeRouter` Server topology;
- Server sends canonical voxel data, never Client render meshes;
- direct friend `spk::Message` operators on `Voxel::Volume`;
- ZQSD for the temporary inspection controls;
- archive code is critically reviewed inspiration, not a specification.

## Tests and evidence

Use the ticket's acceptance tests as the implementation contract.

Prefer test-first development where practical.

Run:

1. the narrow test suite(s) owned by the ticket;
2. required integration tests;
3. the relevant regression/build configuration(s) required by the ticket or Definition of Done.

Do not replace golden/reference images automatically. Produce candidates/diffs and wait for explicit human approval when required.

Do not weaken tests merely to make the implementation pass.

## If you discover ambiguity or a problem

If implementation exposes a material unresolved choice:

- do not invent a solution;
- identify the exact missing contract;
- reference the relevant OQ/DR/Architecture/ticket section;
- keep independently correct work if possible, but do not cross the unresolved boundary.

If you find archived/current code that conflicts with the ticket, the active approved backlog contract wins unless a newer explicit user decision says otherwise.

## Required backlog/documentation updates after implementation

When implementation is complete:

1. Update `<TICKET_ID_OR_PATH>`:
   - mark `Done` only if all Definition-of-Done requirements are actually met;
   - record completion evidence;
   - list exact tests/builds run and results;
   - record actual human approvals where required.

2. Update the parent Epic:
   - reflect ticket completion in its index/coverage;
   - update integration/exit status only for behavior actually delivered.

3. Update `docs/backlog/CURRENT-STATUS.md`:
   - update the date;
   - add the completed ticket/behavior under the latest implementation state;
   - update what currently exists;
   - make the **next Ready ticket** explicit when one exists;
   - otherwise identify the exact next blocking OQ/decision;
   - remove stale statements contradicted by the implementation.

4. Update `docs/backlog/IMPLEMENTATION-CONTEXT.md` only when the ticket establishes an explicitly approved durable convention useful to future implementation agents.

5. Update OQ/DR/Architecture documents only when their real decision/status changed. Do not retroactively invent approval.

6. Update any public documentation whose contract changed.

## Completion report

Return a concise implementation report containing:

- ticket ID/title;
- production changes;
- tests/builds run and results;
- documentation/backlog updates;
- whether the ticket is truly Done;
- next Ready ticket or blocker;
- required human validation still pending, if any.
