# Definition of Ready

A detailed implementation ticket may become Ready only when an implementation agent can write the acceptance tests first and implement the approved behavior without inventing public product or architecture decisions.

## Required

- Parent Epic/capability is known.
- Production owner/module is known.
- Test owner/suite is known.
- Dependencies are known.
- Allowed and forbidden dependencies are known where architecture boundaries matter.
- Intent and independently useful result are explicit.
- Public behavior is explicit.
- Inputs and outputs are explicit.
- Owned state and explicitly non-owned state are explicit.
- Important state transitions are explicit.
- Invariants are explicit.
- Invalid-input behavior is explicit.
- Failure behavior and post-failure state are explicit.
- Lifecycle and ownership semantics are explicit where relevant.
- Determinism and ordering semantics are explicit where relevant.
- Serialization/persistence semantics are explicit where relevant.
- Networking/authority semantics are explicit where relevant.
- Concurrency/cancellation semantics are explicit where relevant.
- Required content/numeric values supplied by the user are known.
- Blocking decisions are resolved and linked.
- Exact test fixtures are specified.
- Acceptance tests cover every applicable category.
- Inapplicable test categories explicitly say why they do not apply.
- Completion evidence is defined.

## Automatic blockers

A ticket is not Ready if the implementation agent would need to decide any material product behavior, architecture contract, protocol semantic, persistence rule, numeric gameplay value, ownership rule, failure behavior, or test expectation.

A ticket is not Ready merely because implementation could begin.

## Ready review question

Could another coding agent write the failing acceptance tests from this document before touching production code, without asking what observable behavior is intended?

If no, the ticket remains Draft or Blocked.
