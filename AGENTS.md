# AI development instructions — Erelia

Before proposing or changing work, read the following documentation in order:

1. [Project index](docs/project/README.md).
2. [Decision authority and governance](docs/project/governance.md).
3. [Story development workflow](docs/project/workflow.md).
4. [Planning and statuses](docs/project/planning.md).
5. [Requirements and traceability](docs/project/traceability.md).
6. [Review and acceptance](docs/project/review-and-acceptance.md).
7. [Current status](docs/project/STATUS.md).
8. The **specific authorized Story**, its requirements and approved technical design, and the relevant GDD / SDD / ERD / architecture documents.

## Non-negotiable rules

- The human owner is product owner, technical architect and final merge authority. Obtain explicit approval for consequential behavior, architecture, APIs, ownership, error policies, security/performance contracts and test design. Ask about **one consequential decision at a time**.
- Don't implement a Story before approval of requirements, design, independent complexity audit and test plan. Do not quietly broaden scope or invent defensive infrastructure.
- Implement reviewed, approved tests first in a distinct **test-only PR checkpoint**. They must compile, run and fail for the expected missing behavior; separate review and owner authorization precede production code. The same development AI may implement both phases; reviewers are independent and read-only.
- Keep exactly one Story branch and PR for ordinary feature work. Independent reviews are required. **Do not merge PRs.** The owner manually merges and accepts milestones.
- Traceability must link Stories and C++ tests to approved SDD / ERD requirements. Generator, validators and CI integration are **M1 engineering deliverables**, not prerequisites for this M0 documentation baseline.
- Preserve the game vision in [the GDD](docs/gdd/Erelia_GDD_Current.md); significant changes require owner approval and an impact assessment.
- No v1 implementation or tests are grandfathered into v2. Consult [restart boundaries](docs/project/restart.md) before reusing historical material.
- Existing Sparkle defects belong in the Sparkle repository; proposed general-purpose extensions may use an independent Sparkle Addon in Erelia only through approved design.

This file is an index of rules, not a replacement for the linked documents.
