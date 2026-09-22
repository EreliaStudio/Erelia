# AI Implementation Prompt — Next Ready Step

Use this prompt when no specific ticket is supplied and the implementation agent should determine the next safe implementation step from the backlog.

---

You are implementing work in the Erelia repository.

Repository: `https://github.com/EreliaStudio/Erelia`

Your job is to identify and implement the **next Ready implementation ticket**, using the repository backlog as the source of truth.

## Mandatory reading before choosing work

Read these files first, in this order:

1. `docs/backlog/PROJECT-CONTEXT.md`
   - Understand the product/gameplay context.
   - Do not use this summary to override the GDD or a newer explicit decision.

2. `docs/backlog/IMPLEMENTATION-CONTEXT.md`
   - Pay particular attention to:
     - naming/type-organization taste;
     - Core / Server / Client ownership;
     - dependency policy;
     - Server/NodeRouter networking conventions;
     - voxel API/serialization conventions;
     - small-ticket implementation style;
     - unresolved EP-001 details that must not be invented.

3. `docs/backlog/CURRENT-STATUS.md`
   - Treat the **Current planning phase**, **Next**, and current implementation focus as the primary guide for what should happen next.
   - Check whether the status document explicitly identifies a ticket or a next implementation slice.

4. `docs/backlog/OPEN_QUESTIONS/README.md`
   - Check the status of any OQ related to the candidate work.
   - Read the individual OQ files for the candidate ticket's area.
   - Do not silently resolve an Open or Partially resolved material contract.

5. `docs/backlog/DECISIONS/README.md`
   - Read every DR constraining the candidate ticket.

6. Relevant files under `docs/backlog/ARCHITECTURE/`.

7. The active Epic document and its `tickets/README.md`.

8. `docs/backlog/DEFINITION-OF-READY.md`.

9. `docs/backlog/DEFINITION-OF-DONE.md`.

10. The candidate ticket itself.

Also inspect the current production code and tests that the ticket will affect. The archive may be consulted for inspiration only; it is not authoritative.

## How to choose the ticket

Choose work in this order:

1. If `CURRENT-STATUS.md` explicitly names a **Ready** ticket as the next implementation step, use it.
2. Otherwise, inspect the active Epic's ticket index and select the earliest dependency-satisfied ticket whose status is **Ready**.
3. Do not implement a ticket marked Draft, Blocked, or already Done.
4. Do not create a new implementation ticket merely to give yourself something to code.
5. If no ticket is Ready, stop before production changes and report the exact blocking OQ/decision/ticket dependency that prevents implementation.

Do not choose a visually larger or more interesting later ticket while an earlier dependency ticket is Ready and required first.

## Before coding

Verify that the selected ticket satisfies the Definition of Ready.

Specifically confirm that you can determine, without inventing behavior:

- production owner/module;
- test owner/suite;
- allowed and forbidden dependencies;
- public contract;
- inputs and outputs;
- invariants;
- invalid/failure behavior;
- ownership/lifetime;
- determinism/order where relevant;
- serialization/network semantics where relevant;
- exact fixtures;
- acceptance evidence.

If any material point is missing, do not guess. Report the ambiguity and the relevant OQ/ticket section.

## Implementation rules

Implement **only the selected ticket**.

Keep the change small and focused. Do not pull future tickets into the same implementation merely because doing so seems convenient.

Follow the project's implementation taste from `IMPLEMENTATION-CONTEXT.md`, including:

- prefer semantic namespace/type organization such as `Voxel::Volume` rather than prefixed names such as `VoxelVolume`;
- use the standard library and Sparkle rather than introducing another dependency;
- keep authoritative decisions in Server, shared reusable representation/algorithms in Core, and rendering/input/presentation in Client;
- use Sparkle networking and the approved NodeRouter topology where applicable;
- preserve the approved `Voxel::Cell` / `Voxel::Volume` and message-operator conventions where applicable;
- never make archived code authoritative by copying it blindly.

Write tests first when practical from the ticket's acceptance contract.

Run the narrowest relevant tests during development, then the complete regression/build commands required by the ticket and Definition of Done.

Do not silently update or approve golden images.

## Scope control

If implementation reveals a real missing contract:

- do not hide the decision inside code;
- do not expand the ticket to solve unrelated architecture;
- identify the relevant OQ or explain that a new OQ/DR is required;
- stop only the blocked portion and preserve any independently valid completed work.

If you notice unrelated defects, do not fix them unless they prevent this ticket from being completed. Record them separately.

## Required backlog/documentation updates after implementation

When the ticket is complete:

1. Update the ticket itself:
   - set its status to `Done` only if the Definition of Done is satisfied;
   - record completion evidence;
   - record exact tests/builds run;
   - record any required human approval that actually occurred;
   - do not claim approval that did not occur.

2. Update the parent Epic:
   - update its ticket index/status where applicable;
   - update capability coverage only if this ticket materially changes it;
   - do not mark the Epic Done unless all Epic exit criteria are satisfied.

3. Update `docs/backlog/CURRENT-STATUS.md`:
   - update the date;
   - describe what was just implemented;
   - describe the resulting code/test state;
   - identify the **next Ready ticket or next blocking decision**;
   - remove stale statements that the completed capability does not yet exist.

4. Update `docs/backlog/IMPLEMENTATION-CONTEXT.md` only if the implementation established a durable, explicitly approved implementation convention that future agents need to know.
   - Do not turn a one-off implementation detail into a project-wide convention.

5. Update architecture/DR/OQ documents only when their recorded status actually changed.
   - Never mark an OQ resolved merely because you chose an implementation.
   - Durable choices require explicit approval and, where appropriate, a DR.

6. Update other public documentation affected by the ticket's actual contract.

## Completion report

At the end, report concisely:

- ticket implemented;
- production files changed;
- tests/builds run and their results;
- backlog/documentation files updated;
- remaining blockers or follow-up ticket;
- any required human validation still outstanding.

Do not claim the ticket is Done if any Definition-of-Done requirement or required human approval is still missing.
