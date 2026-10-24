# Owner-controlled Story workflow

The default workflow applies to new production-code Stories. Documentation-only and project-management work may use justified exemptions, but not bypass owner authority or independent review.

## Phase 1 — Requirements

1. Define observable behaviors, scope/exclusions, requirement links and acceptance criteria.
2. The owner approves the **requirements and functional scope** before technical design.
3. If later design discovers a material new behavior, return to the requirements gate.

## Phase 2 — Technical design and complexity audit

4. Design responsibilities, interfaces, ownership, errors, resource contracts and **exact tests**. Obtain owner approval for consequential decisions.
5. Approve any newly identified [ERD](../erd/README.md) engineering guarantees before writing tests.
6. A **different, read-only AI reviewer** audits the complete technical design for unnecessary complexity. Submit **each material finding** to the owner for decision and record accepted/rejected findings and rationale.
7. Obtain explicit owner authorization for the final design and tests.

## Phase 3 — Strict test-first PR checkpoint

8. Create **one Story branch and PR**. Implement approved tests **before** substantive feature code.
9. Tests must compile and execute and fail on the expected missing behavior. Minimal approved API scaffolding is allowed (declarations, constructors, simple getters/setters, stubs); production behavior is not.
10. Unrelated tests remain passing. Expected-red new tests are permitted at this checkpoint, but compile failures, unrelated regressions and invalid metadata are not valid evidence.
11. Get an **independent read-only review** of the test-only PR and explicit owner authorization before feature implementation. Keep evidence of checkpoint and test expectations, but no artificial permanent Git checkpoint commit is required.

## Phase 4 — Implementation and delivery

12. Implement the approved design to pass the locked tests. Do not silently modify test expectations, expand scope or add speculative recovery behavior.
13. When operational, regenerate SDD/ERD Story/Test links before every PR update and ensure strict CI verifies the generated tables and dependencies.
14. Run relevant unit, integration, end-to-end, golden and performance checks as approved. Distinguish unrun from passed checks.
15. Obtain a **different read-only AI implementation reviewer**. The development AI may fix unambiguous defects; the reviewer verifies corrections. Consequential changes require owner approval.
16. Write a detailed, evidence-based [acceptance report](review-and-acceptance.md), request owner review, and **do not merge**.
17. The owner manually merges; the AI verifies the result and reconciles the Story status to `Done`.

## Exceptions

Pure behavior-preserving refactoring can begin from passing **owner-approved regression tests**. A blocked implementation may return to a reviewed/approved test-development phase. Documentation-only M0 is not subject to unavailable M1 tooling or the production-code test-first phase.

## M1 bootstrapping

Traceability generator, Story metadata validation and CI enforcement are early M1 deliverables. Only approved Stories implementing this infrastructure may rely on manually verified requirements, metadata, Test IDs and dependencies until automation exists; other feature delivery waits for operational enforcement.
