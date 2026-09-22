# AI Planning Prompt — Create Small Implementation Tickets for an Epic

Use this prompt when you want another AI to decompose an approved Epic into small implementation tickets without writing production code.

Replace `<EPIC_ID_OR_PATH>` before use. If omitted, the agent must use the active Epic identified by `CURRENT-STATUS.md`.

---

You are planning implementation work in the Erelia repository.

Repository: `https://github.com/EreliaStudio/Erelia`

Your task is to create **small, implementation-ready tickets** for:

> **<EPIC_ID_OR_PATH>**

If no Epic is explicitly supplied, identify the active near-term Epic from `docs/backlog/CURRENT-STATUS.md`.

**Do not implement production code in this task.** This is ticket decomposition and contract clarification only.

## Mandatory reading

Read these files before creating tickets:

1. `docs/backlog/PROJECT-CONTEXT.md`
   - Product/gameplay meaning and current near-term focus.

2. `docs/backlog/IMPLEMENTATION-CONTEXT.md`
   - Especially:
     - project naming/API taste;
     - Core / Server / Client ownership;
     - dependency policy;
     - small-ticket preference;
     - testing ownership;
     - voxel/networking conventions where relevant.

3. `docs/backlog/CURRENT-STATUS.md`
   - Determine what actually exists now and what the current planning/implementation phase expects next.

4. `docs/backlog/OPEN_QUESTIONS/README.md`
   - Read all individual OQ files relevant to the Epic.
   - A material unresolved OQ blocks the corresponding ticket from becoming Ready.

5. `docs/backlog/DECISIONS/README.md`
   - Read every DR constraining the Epic.

6. Relevant documents in `docs/backlog/ARCHITECTURE/`.

7. The complete Epic document `<EPIC_ID_OR_PATH>`.

8. That Epic's `tickets/README.md`.

9. `docs/backlog/DEFINITION-OF-READY.md`.

10. `docs/backlog/DEFINITION-OF-DONE.md`.

11. `docs/backlog/templates/IMPLEMENTATION-TICKET-TEMPLATE.md`.

Inspect current source/tests enough to understand real dependency boundaries and current APIs. Historical `archive/` material is inspiration only unless explicitly reintroduced.

## Ticket sizing rule

Create **small, focused tickets**.

A ticket should normally own one coherent implementation result that can be tested and reviewed independently.

Prefer decomposition such as:

- shared data/type contract;
- serializer;
- deterministic generator;
- Server routing handler;
- Client request coordinator;
- meshing algorithm;
- rendering integration;
- inspection input;
- end-to-end integration fixture;

rather than one ticket like:

> "Implement voxel chunks, generation, networking, rendering and controls."

Do not intentionally create a ticket that requires a huge multi-layer patch when a stable contract can separate it into smaller steps.

A useful ticket should usually be implementable without forcing the coding agent to touch unrelated layers.

Do not split work into meaningless one-line micro-tickets either. Each ticket must deliver an independently verifiable capability or contract.

## Dependency ordering

Order tickets so each ticket has clear prerequisites.

Prefer foundational shared contracts first, then their direct Server/Client consumers, then cross-system integration.

A later ticket must not require functionality that only exists in an even later ticket.

For every ticket, state exact dependencies using ticket IDs once assigned.

## Ready versus Draft / Blocked

Do not mark a ticket Ready merely because its code could be started.

Apply `DEFINITION-OF-READY.md` strictly.

For each proposed ticket:

- mark **Ready** only if all observable behavior, fixtures, ownership, failure behavior and applicable networking/serialization semantics are explicit;
- mark **Blocked** if a known OQ/decision prevents a correct implementation contract;
- mark **Draft** if useful decomposition is known but more specification work is still needed.

If an unresolved OQ only affects later tickets, do not block earlier independent tickets unnecessarily.

## Required ticket content

Use `IMPLEMENTATION-TICKET-TEMPLATE.md`.

Every Ready ticket must contain enough detail that another coding AI can:

1. write the acceptance tests first;
2. implement the ticket;
3. determine Done;

without inventing project behavior.

Be explicit about:

- Intent / independently useful result;
- production target(s);
- test suite(s);
- prerequisites;
- allowed/forbidden dependencies;
- owned and explicitly-not-owned behavior;
- public contract;
- invariants;
- state transitions;
- invalid/failure behavior;
- determinism/ordering;
- lifecycle/ownership;
- serialization/persistence;
- networking/authority;
- implementation constraints that are actually deliberate;
- exact fixtures;
- acceptance tests for all applicable categories;
- completion evidence;
- linked DR/ARCH/OQ sources.

Do not invent exact public APIs unless the project already chose them or an exact signature is necessary and unambiguous from approved constraints.

## Project-specific implementation taste to preserve

Where relevant:

- prefer domain scoping such as `Voxel::Volume` over prefixed `VoxelVolume`;
- Core owns shared reusable types/algorithms, not Server authority;
- Server owns authoritative results;
- Client owns rendering/input/presentation;
- use standard library + Sparkle, not another runtime dependency;
- Server begins with Sparkle `NodeRouter`;
- Server sends canonical voxel data, never terrain render meshes;
- `Voxel::Volume` uses approved friend `spk::Message` insertion/extraction operators;
- temporary EP-001 movement uses ZQSD;
- archived code is reviewed critically and not copied as a requirement.

## EP-001-specific caution

If planning EP-001, pay particular attention to:

- OQ-035 — remaining Cell/Volume representation details;
- OQ-036 — missing-neighbor/remesh behavior;
- OQ-037 — remaining scalar wire portability;
- OQ-038 — request/cache/partial response details;
- OQ-039 — exact generator fixture;
- OQ-029 through OQ-031 — visual/performance validation.

Do not bury one of these unresolved choices inside a Ready implementation ticket.

Create earlier tickets that do not depend on those choices where possible.

## Files to update

Create ticket files under the Epic's `tickets/` folder using:

`ST-XXX-YY-<implementation-goal>.md`

Then update:

1. the Epic's `tickets/README.md` with ordered ticket links/status/dependencies;
2. the parent Epic ticket index/capability coverage;
3. `docs/backlog/CURRENT-STATUS.md`:
   - record that the tickets were materialized;
   - state which ticket is the **first Ready implementation ticket**, if any;
   - otherwise state the exact OQ/decision that blocks implementation;
4. any OQ only if its status changed through an explicit decision during this planning task;
5. no production source files.

## Final review

Before finishing, verify:

- no ticket duplicates another ticket's owned behavior;
- no Ready ticket depends on a later ticket;
- no Ready ticket contains an unresolved material decision;
- ticket sizes are intentionally small;
- the first implementation ticket is explicit;
- the Epic still has a coherent end-to-end path;
- the set of tickets does not accidentally expand beyond the Epic's stated scope.

## Completion report

Return:

- tickets created, in dependency order;
- status of each (Ready / Draft / Blocked);
- first Ready ticket to implement;
- unresolved OQs blocking later tickets;
- backlog documents updated;
- any proposed Epic split if the decomposition is becoming too large.

Do not write implementation code.
