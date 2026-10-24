# Planning and status

## Planning hierarchy

The structure is **Milestone → Epic → Story → technical-task checklist**. There are no sprints.

- **Milestone:** an integrated, observable capability with owner-approved acceptance criteria and explicit owner acceptance after verification.
- **Epic:** a grouping of Stories toward an objective; no separate completion/acceptance gate.
- **Story:** coherent independently verifiable delivery, often crossing modules, with its own branch and PR.
- **Technical task:** checkbox inside the Story, never an independent PR or authority.

Maintain one primary implementation Milestone. Independently approved work may overlap it. High-level roadmap coverage spans the entire GDD, but detailed requirements and Stories are designed progressively; do not auto-approve a roadmap by creating folders.

## Story statuses

Exactly seven statuses are permitted: `Backlog`, `Designing`, `Ready`, `Implementing`, `Review`, `Awaiting Merge`, `Done`.

A Story becomes `Done` only after its PR is **manually merged by the owner**, the merge is verified and documentation is reconciled. Tests/design checkpoints and blockers are recorded separately from status.

## Dependencies and prioritization

`DependsOn` captures **hard implementation blockers**, not merely conceptual relationships. Dependent Stories can be discussed/designed but cannot start their test-only PR or production work until dependencies are merged, verified and `Done`. Tooling introduced in M1 must reject unknown references and cycles.

AI recommends the next ready Story based on priorities and dependencies; the owner selects and authorizes it. Unrelated authorized Stories may proceed in parallel. Avoid stacked dependent PRs in the standard workflow.

## Acceptance

The owner approves Milestone acceptance criteria in advance and acceptance explicitly at delivery. Completing subordinate Stories is necessary but does not independently prove the integrated Milestone works.
