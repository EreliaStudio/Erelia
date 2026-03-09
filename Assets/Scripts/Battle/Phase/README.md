# Battle.Phase README
## Purpose
Phase contains the battle phase framework, the phase `Orchestrator`, and the concrete battle flow phases.
`Orchestrator` is responsible for driving the active phase and applying transitions safely.

Each `phase` lives in its own folder so phase-specific supporting classes can stay colocated with the phase root.

## Layout

Orchestrator.cs: drives the current phase, applies transitions, and ticks the battle flow.

Controller.cs, Root.cs, Registry.cs, Id.cs, Info.cs: shared phase infrastructure in Erelia.Battle.Phase.

### Concrete phases:
Initialize/MainRoot.cs: prepares battle data and placement centers.
Placement/MainRoot.cs: handles unit placement and placement masks (player = lower Z half, enemy = upper Z half).
PlayerTurn/MainRoot.cs: runs player turn logic.
EnemyTurn/MainRoot.cs: runs enemy turn logic.
ResolveAction/MainRoot.cs: resolves queued actions or effects.
Victory/MainRoot.cs: handles end-of-battle victory flow.
Defeat/MainRoot.cs: handles end-of-battle defeat flow.
Cleanup/MainRoot.cs: resets state and exits the battle scene.

## Naming

Shared types live under Erelia.Battle.Phase.

Each concrete phase root uses the pattern:
Erelia.Battle.Phase.<Name>.MainRoot.
Add any extra classes for a phase inside that same folder and namespace branch.

## Adding Or Extending
1. Implement a new phase root that derives from Root.
2. Place it under its own Phase/<Name>/ folder.
3. Add its id to Id.
4. Register the phase in Registry.
5. Request transitions through the Orchestrator when appropriate.