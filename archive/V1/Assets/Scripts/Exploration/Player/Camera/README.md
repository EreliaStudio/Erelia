# Exploration.Player.Camera README

## Purpose
Camera contains exploration camera orbit and zoom controls.
It converts input actions into camera rotation and distance changes.

## Contents
- `Presenter`: applies orbit and zoom input each frame.
- `Model`: serialized tuning values for orbit and zoom.
- `View`: exposes the camera transform.

## Adding Or Extending
1. Add new camera tuning fields to `Model` and apply them in `Presenter`.
2. Keep camera-only logic here to avoid coupling with world or player systems.
