# Independent reviews and acceptance evidence

## Design complexity review

Before implementation, a **separate read-only AI reviewer** assesses the approved Story design for unnecessary states, layers, caches, protections, retry schemes and overengineering. It must retain justified correctness, security and performance requirements. Every material finding goes to the owner **one at a time**; approved, amended and rejected findings remain recorded.

## Test-only checkpoint review

The Story PR first contains approved automated tests without implemented feature logic. An independent read-only reviewer checks expected-red evidence, valid API scaffolding, test coverage, unrelated passing tests and requirement compliance. The owner authorizes moving to implementation.

## Implementation review

After code and tests, a different read-only AI reviewer checks implementation against approved design, behavior, quality, tests and evidence. The development AI may fix clear bugs or deviations; ambiguous changes return to the owner. Reviewer re-verifies corrections.

## Story acceptance report

Each Story's durable report contains:
1. Concise summary of delivered scope and current gate.
2. Implementation changes and relevant design decisions.
3. Acceptance criterion **by criterion** evidence.
4. Test IDs, expected vs actual results, CI runs, platforms and conditions.
5. Performance/golden results where applicable.
6. Design compliance and independent review findings/fixes.
7. Unresolved limitations and justified omissions.
8. Story PR, approvals and owner merge decision.
9. Post-merge verification and status reconciliation.

Never claim an unrun test passed; distinguish metadata traceability from test execution.

## Milestones

Milestone acceptance is a separate, explicit owner decision requiring integrated evidence against previously owner-approved observable criteria. Epic completion has no independent acceptance gate.

## M0 documentation review

M0 comprises documentation only; it is **not** subject to M1 tooling that does not yet exist. Its delivered documents still require independent review and owner acceptance. A one-off authorized repository reset does not itself make M0 accepted.
