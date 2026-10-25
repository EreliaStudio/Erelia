# Erelia

**Erelia** is a C++23 multiplayer fantasy RPG built around persistent voxel Worlds, shared progression, classless Heroes and tactical combat on the same terrain used during exploration.

## Project documentation

- [Game Design Document](docs/gdd/Erelia_GDD_Current.md) — game vision and 34 illustrations.
- [Project governance](docs/project/README.md) — methodology, owner approvals and agent workflow.
- [Project status](docs/project/STATUS.md) — current stage and approved work.
- [Software requirements (SDD)](docs/sdd/README.md) — observable product guarantees.
- [Engineering requirements (ERD)](docs/erd/README.md) — internal engineering guarantees.
- [Architecture](docs/architecture/README.md) — cross-cutting technical decisions.
- [AI development instructions](AGENTS.md) — starting point for agents.

## Development

Work proceeds through owner-approved requirements, technical design, tests-first implementation and independent review. The GDD defines the game vision, while technical details are approved progressively.

**Current stage:** M0 documentation review remains pending. The M1 engineering foundation is being developed in draft PR #20. M1 prepares permanent development infrastructure for future milestones and has no separate milestone acceptance gate; the owner still reviews and merges each PR. The scaffold exists only on the development branch and the remaining Python/graphical and traceability tooling is not complete.
