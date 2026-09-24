# Definition of Ready / Done

## Story Ready

A story is Ready only when:

- its exact starting state, input/action, successful observable state, and unchanged rejection state are specified;
- concrete behavior is testable through a public/domain contract;
- known test values, boundaries, invalid values, stable IDs, and asset data are supplied;
- missing user-owned assets, schemas, balance values, formulas, policies, or technology choices are explicitly listed under user validation;
- every relevant Open Decision is linked, including what it blocks and what may proceed;
- ownership, exclusions, upstream/downstream contracts, deterministic rule, and lifecycle/retry/save-load behavior are identified;
- rendering impact is explicitly `None` or `Yes`;
- each graphical story defines a controlled scene, camera, viewport, deterministic assets, expected golden image, tolerance, and unchanged-versus-intentional-baseline policy;
- logical/value tests remain alongside golden-image tests;
- an implementer does not need to invent feature semantics.
- it contains the mandatory execution decision policy: unspecified observable choices are Open Decisions, not permission to improvise.

A story is not Ready when its acceptance tests merely say “nominal fixture succeeds,” “boundary fixture succeeds,” “deterministic where applicable,” or equivalent boilerplate without observable behavior.

Discovery during implementation can still expose a missing decision in an otherwise Ready ticket. In that case, readiness does not authorize invention: pause the affected portion, ask the user, create or update the relevant OD record, and continue only with independent work.

## Story Done

- The owned behavior and all exhaustive acceptance cases are implemented and passing.
- Invalid operations prove no partial state, event, resource, scheduling, or persistence mutation.
- Deterministic and headless tests pass where applicable.
- Lifecycle, retry, persistence, and neighboring-contract cases pass.
- Required user approvals and resolved decisions are recorded in their persistent files with provenance.
- Graphical changes pass semantic assertions and reviewed golden comparisons; failed references were not silently replaced.
- Completion evidence links exact fixtures, automated tests, CI results, and visual artifacts where applicable.

## Epic Done

- Every subject in the coverage matrix maps to implementation and test tickets.
- Epic integration tests prove composition across its tickets.
- Decision gates and user-input requirements are resolved or explicitly defer only out-of-scope work.
- Documentation, traceability, and affected golden baselines are current.
