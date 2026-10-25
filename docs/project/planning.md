# Planning and status

## Planning hierarchy

The structure is **Milestone → Epic → Story → technical-task checklist**. There are no sprints.

- **Milestone:** ordinarily, an integrated observable capability with owner-approved acceptance criteria and explicit owner acceptance after verification. **M1 is the infrastructure-preparation exception**: it establishes the reusable engineering foundation and requires no separate milestone-acceptance gate.
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

For delivery Milestones, the owner approves acceptance criteria in advance and accepts the integrated result explicitly; completing subordinate Stories alone does not prove the capability works.

**M1 exception:** M1 prepares the common project infrastructure for subsequent milestones. It has no separate end-of-milestone acceptance or milestone-specific validation procedure. Applicable tests, CI, Story evidence, independent reviews, and the owner's manual PR merge authority remain in force. Missing technical infrastructure must not be reported as implemented.
