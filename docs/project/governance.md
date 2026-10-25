# Decision authority and governance

## Roles

The human owner is Erelia's product owner, technical architect, requirements authority and final delivery/merge authority. AI agents prepare analyses, code and documentation **within explicitly authorized tasks**.

The owner must approve every consequential technical decision: responsibilities, architecture, public APIs, ownership/lifetimes, concurrency, error and recovery behavior, performance and resource bounds, security constraints and test expectations. Trivial coding syntax does not require separate approval.

Raise **one consequential choice at a time** and recommend a simple option. Challenge speculative abstractions, duplicate safeguards, caches, retries and auxiliary state. Maintain necessary correctness and security.

An explicit task prompt conveys authorization even when the Story has not yet been updated; synchronize approvals into the Story history. A reviewer may report findings but **must not edit** the reviewed code or test files.

## Documentation authority

- Repository Markdown is the durable record, not GitHub Issues or Projects.
- Each Story owns its status, design and decision history. Cross-cutting architecture decisions may have separate records under [architecture](../architecture/README.md).
- `STATUS.md` is derived and must not compete with Story source-of-truth.
- Significant GDD changes require owner approval, impact assessment and do not automatically authorize rewriting implemented behavior.
- Approved decisions can be committed in coherent batches rather than one commit per question.

## Change control

Requirements and scope must be approved before design. If design reveals new observable requirements, return to approval. If scope grows within an active Story, ask the owner, reapprove **only affected** design/tests, and keep the same branch/PR for related work. Independently deliverable changes receive their own Story. Post-merge defects receive a new corrective Story rather than reopening a completed one.

## Delivery authority

An AI never autonomously merges a Story PR, marks a Milestone accepted or declares undocumented work approved. The owner manually merges PRs and explicitly accepts delivery milestones. M1 is an infrastructure-preparation exception: it has no separate milestone acceptance decision. This does not delegate merge authority to an AI or waive technical tests or applicable Story reviews. 
