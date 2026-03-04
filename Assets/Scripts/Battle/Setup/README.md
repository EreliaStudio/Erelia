# Battle.Setup README

## Purpose
Setup contains scene-level bootstrapping for battles. It binds battle data to presenters
and positions the player relative to the board.

## Contents
- `Loader`: reads `Erelia.Core.Context` and assigns the battle board to the presenter.

## Adding Or Extending
1. Add new scene-level bindings in `Loader` when additional presenters need data.
2. Keep `Loader` lightweight; heavy logic should live in phases or presenters.
