# Erelia

Erelia is a C++23 multiplayer voxel-fantasy RPG project. The application is being restarted as **Erelia v2** while preserving the existing v1 Git history.

## Start here

- [Game Design Document](docs/gdd/Erelia_GDD_Current.md) — working game-vision baseline and illustrated gameplay concepts.
- [Project documentation](docs/project/README.md) — approved working methodology, planning and development rules.
- [Current project status](docs/project/STATUS.md) — summary; individual Story documents are the source of truth.
- [Software requirements (SDD)](docs/sdd/README.md) — observable product requirements, to be established progressively.
- [Engineering requirements (ERD)](docs/erd/README.md) — internal engineering guarantees, to be established progressively.
- [Architecture](docs/architecture/README.md) — cross-cutting architecture decisions, to be designed and approved.
- [Agent instructions](AGENTS.md) — entry point for AI-assisted work.

## Restart boundary

The v2 active tree deliberately does not contain v1 application code, historical CI, CMake configuration, tests, or completed backlog. No v1 capability is automatically considered implemented in v2. The GDD and its illustrations are retained as **game-vision references**, not as approval of old implementation choices.

The exact v1 source remains available in [`archive/erelia-v1`](https://github.com/EreliaStudio/Erelia/tree/archive/erelia-v1), from commit `7d46dad25dfb5bdf6f792479e993a8bced4e3aef`.

## Development status

This is the **v2 restart and M0 documentation baseline**. Independent documentation review and owner approval remain necessary before M0 is accepted. The first engineering milestone (M1) will bootstrap tooling, tests and CI. No application feature development is authorized by this reset.
