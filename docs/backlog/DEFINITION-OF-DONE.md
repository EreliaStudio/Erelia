# Definition of Done

A ticket is Done only when the implementation and the evidence requested by the ticket both exist.

## Required completion

- Production implementation exists.
- All ticket acceptance tests pass.
- Nominal behavior is tested.
- Applicable boundary behavior is tested.
- Invalid/rejected operations are tested.
- Post-failure state/failure atomicity is tested where relevant.
- Determinism/ordering is tested where required.
- Lifecycle/ownership is tested where relevant.
- Serialization/persistence round-trips and malformed inputs are tested where relevant.
- Retry/idempotency is tested where relevant.
- Concurrency/cancellation paths are tested where relevant.
- Client/server authority and trust boundaries are tested where relevant.
- Dependency failures are tested where meaningful.
- Cross-system integration contracts pass where required.
- Performance invariants/benchmarks requested by the ticket pass.
- Client-visible golden-image or visual validation artifacts are reviewed where required.
- No forbidden dependency was introduced.
- Documentation matches the resulting public contract.
- Required human approval is recorded.
- Completion evidence is recorded in or linked from the ticket.
- The relevant regression suite passes.

## Golden-image rule

Expected images are never silently replaced.

A missing or changed reference produces an actual/candidate result and comparison evidence for explicit human review. Approval of a new reference is a separate human action.

## Not sufficient

The following alone do not make a ticket Done:

- code compiles;
- the happy path works manually;
- unit tests pass while required integration tests are missing;
- a visual result looks acceptable to the implementation agent;
- a new golden image was automatically accepted;
- behavior changed without updating its backlog contract.
