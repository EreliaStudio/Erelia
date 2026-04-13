# Battle.Mask README

## Purpose
Mask contains the sprite registry used to render overlay masks on the battle board.
Each mask type maps to a sprite that the mask mesher uses for UVs.

## Contents
- `MaskSpriteRegistry`: singleton registry mapping `Battle.Voxel.Mask.Type` to sprites.

## Adding Or Extending
1. Add a new entry to `Battle.Voxel.Mask.Type` if a new mask category is needed.
2. Extend `MaskSpriteRegistry` with a new sprite field and mapping.
3. Create or update the registry asset at `Resources/Mask/MaskSpriteRegistry`.
