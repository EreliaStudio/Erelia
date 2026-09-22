# Erelia Ticket Execution Prompt

Implement the supplied Erelia backlog ticket exactly as specified.

Before changing code:

1. Read the entire ticket.
2. Read its epic document, prerequisite contracts, linked architecture documents, and every linked Open Decision.
3. Verify that all required user-provided inputs exist and that every blocking decision is resolved.
4. Restate the exact owned behavior, exclusions, fixtures, and observable acceptance results you will implement.

## Mandatory no-improvisation rule

Do not invent, infer, or silently select anything the ticket or an already-resolved linked decision does not define. This includes behavior, data, assets, constants, formulas, schemas, precedence, ownership/lifetime semantics, failure/fallback policy, architecture, dependencies, persistence/network choices, visual output, and expected test results.

Treat every unspecified choice that can affect public behavior, compatibility, persistence, rendering, tests, or later tickets as an Open Decision—not as implementation freedom.

When such a choice is discovered:

1. stop before implementing the affected portion;
2. identify the exact missing decision and which acceptance cases it blocks;
3. ask the user one focused clarification question, offering options only when the known constraints support them;
4. record the approved answer in the linked OD file, or create and link a new OD record if none exists;
5. update the ticket when the answer changes its contract;
6. continue independent, unblocked work only.

You may choose a low-level implementation detail without asking only when the ticket explicitly delegates that detail and the choice cannot change observable behavior, compatibility, resource lifetime semantics, persisted data, rendering, test expectations, or later contracts.

## Implementation and validation

- Work only within the ticket's owned behavior.
- Validate all preconditions before mutation and prove rejected operations leave the specified state unchanged.
- Use the exact fixtures and values from the ticket. Missing fixtures or expected values require clarification; do not substitute a “representative” choice.
- Implement every acceptance case, including boundaries, invalid input, determinism, lifecycle/retry/persistence, and neighboring-contract interactions.
- For graphical work, keep semantic assertions alongside golden-image comparison. Never replace a failed reference automatically; provide expected, actual, and difference images for user review.
- Update resolved OD provenance and provide the completion evidence requested by the ticket.

If the ticket conflicts with its epic, architecture documents, GDD, or a resolved OD, stop and ask the user which contract is authoritative before coding the conflicting behavior.
