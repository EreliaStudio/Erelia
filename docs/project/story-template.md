# Story template

Copy the following structure for each newly approved Story. This file is a **template**, not an actual Story or a preapproved set of requirements. Fill sections progressively. If a section is inapplicable, write `N/A` and explain why.

```md
# ST-XXXX — Story title

StoryID: ST-XXXX
Milestone: M?-...
Epic: EP-...
Status: Backlog
RequirementIDs: (approved SDD-... / ERD-... IDs, or justified N/A)
DependsOn: (hard prerequisite Story IDs, or none)
Blockers: (if any)
PR: (when available)

## 1. Objective and requirements
- Context, intended behavior and owner-approved requirements
- Scope and explicit exclusions

## 2. Acceptance criteria
- Observable behavior and meaningful boundary/error cases

## 3. Technical design
- Responsibilities, architecture, APIs, ownership, errors, constraints
- Exact automated test specifications

## 4. Decision history
- Options presented, explicit owner choices, rationale, amended scope

## 5. Implementation plan
- [ ] Work items

## 6. Independent complexity audit
- Findings, owner decisions, verification and approval

## 7. Implementation evidence
- Tests (with IDs and requirement links), results and CI/platform evidence
- Performance/golden artifacts if required

## 8. Independent implementation review
- Findings, corrections, repeated verification and limitations

## 9. Final acceptance
- Criterion-by-criterion evidence and exceptions
- Review and approvals, PR/merge verification, status reconciliation
```

Use `RequirementIDs` for both SDD and ERD references. A production-code Story must have at least one approved requirement before implementation. Documentation-only/management work may record a justified exemption. Test IDs are **globally numbered** (e.g. `TEST-000001`), while requirements have **domain-specific** IDs.
