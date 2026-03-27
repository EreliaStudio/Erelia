# Model Proposition

This document proposes a model structure for the game described in the GDD.

The goal is not to define methods yet, only the data classes that make the domain explicit.

Naming rule used in this proposal:
- `Definition` = authored or static data shared by many runs or many creatures
- `Data` / `State` / `Progress` = runtime or save-specific data

Ownership rule used in this proposal:
- owned child data should be stored directly inside the parent object
- authored assets that are only meaningful inside one parent asset should stay composed directly inside that parent
- authored `Definition` data should usually be created as Unity `ScriptableObject` assets and linked directly to other authored assets through inspector references
- explicit ids or keys are still acceptable for save slots, generated instances, debugging, compact voxel storage, or other high-volume runtime data

Unity authoring rule used in this proposal:
- most `Definition` classes are good candidates to become Unity `ScriptableObject` assets
- authored assets should usually reference each other directly, instead of using manual lookup ids
- singleton registries built from `SingletonRegistry<T>` are optional convenience accessors, not the primary authoring link mechanism
- large serialized runtime data, such as voxel chunks, may still store compact definition indexes instead of full object references

Reading note:
- when a section explicitly says `This class is a scriptable object.`, it means the class is intended to be authored as a Unity `ScriptableObject` asset
- when a section explicitly says `This class is serializable.`, it means the class is intended to be stored inside another Unity-serialized class or asset
- sections without that note should usually be read as runtime helpers, enum-like types, or namespace/grouping sections

# Model.Core

## GameRun
This class is serializable.

Represents one full run of the game.

Composed of:
- `World.Data world`
- `Player.State player`
- `Progression.RunProgress progress`

## HealPoint
This class is serializable.

Represents a healing or checkpoint location in the world.

Composed of:
- `World.Structure.Location location`
- display name

# Model.Player

## Player.State
This class is serializable.

Represents the player's persistent state outside battle.

Composed of:
- `World.MapLocation currentLocation`
- `World.MapLocation respawnLocation`
- money if relevant
- `Player.Team team`
- `Creature.PCStorage pcStorage`
- `Item.Inventory inventory`
- obtained badges

## Player.Team
This class is serializable.

Represents the player's active team of up to 6 `Creature.Unit`.

This is the player-side team model used outside battle.

Composed of:
- units

# Model.World

## World.MapLocation
This class is serializable.

Represents one saved or runtime location inside one map.

This is the structure that should let save/load know which map to load, then where to place the object inside that map.
The map should be resolved through the dictionary stored in `World.Data`, keyed by the saved world name.

Composed of:
- world name
- position inside the map

## World.Data
This class is serializable.

Represents the whole world state for the current run.

Composed of:
- dictionary from world name to `World.MapData`
- `World.Generator generator`

This is the container that should let the save system resolve a world name such as `Overworld`, `GymFire_MainRoom`, or `Cave_X` into the actual map data.

## World.MapData
This class is serializable.

Represents one named map of the world, such as the overworld or one interior map.

Composed of:
- loaded or known chunks

This should stay focused on run state that is directly loaded or currently active for one map.
Biome and other authored definitions should be referenced as Unity assets where convenient.
Only dense runtime data such as voxel cells should stay in a compact indexed form.

## World.Biome
This class is a scriptable object.

Represents one biome type used by world generation and wild encounters.

Composed of:
- display name
- terrain generation parameters
- voxel palette for the ground
- scenery placement rules
- locations generable inside the biome
- standard wild encounter
- rare wild encounter if relevant

## World.Biome.WildEncounter
This class is serializable.

Represents the wild encounter data owned by one biome.

This is the data the movement-based encounter check should read when the player finishes a move on a valid encounter surface.

Composed of:
- `Encounter.Definition encounter`
- trigger chance percent

## World.BiomeRegistry
This class is a scriptable object.

Represents the optional singleton registry used to expose biome assets globally in Unity.

This should follow the `SingletonRegistry<T>` pattern if you want a central biome list or lookup helper.

Composed of:
- serialized dictionary from registry key string to biome asset
- optional fallback biome

## World.Generator
This class is serializable.

Represents the deterministic procedural generation context for the current run.

This class should store only the data required to regenerate the world from seed-based rules.
It should not save preloaded chunks, generated roads, generated locations, or other generated world outputs, because those can be rebuilt when loading the run.

Composed of:
- generation profile
- biome field

## World.BiomeField
This class is serializable.

Represents the generated biome distribution of the world.

This is the class the world should query to know which biome applies at a given world coordinate.
It stays outside chunk data because biome borders may cut across chunks and may vary inside a single chunk.

Composed of:
- biome generation seed or derived seed
- biome placement rules or sampling data
- generated biome regions, masks, or caches if needed

## World.Generation.Profile
This class is a scriptable object.

Represents the authored configuration used to generate a world.

This is useful if you want the generator to be driven by data instead of hardcoded constants.

Composed of:
- seed
- terrain noise settings
- biome distribution settings
- location count targets
- road generation settings

## World.Structure.Location
This class is serializable.

Represents any named/generated place placed on the world map.

Composed of:
- location id
- location type
- display name
- world position
- biome
- connected roads

## World.Structure.Town
This class is serializable.

Represents a safe settlement generated in the world.

Composed of:
- inherited location data
- town profile
- buildings
- heal point
- npc or service flags

## World.Structure.GymLocation
This class is serializable.

Represents a gym placed in the generated world.

Composed of:
- inherited location data
- gym prefab
- interior space

## World.Structure.PointOfInterest
This class is serializable.

Represents a notable non-town, non-gym location such as a cave, ruin, pond, or dungeon entrance.

Rare encounter behavior inside a point of interest should come from special voxel definitions placed by the authored prefab, rather than from a generated rare-spot list.

Composed of:
- inherited location data
- point of interest prefab
- poi category
- linked structures
- interior space if enterable

## World.Structure.Road
This class is serializable.

Represents a generated connection between two locations.

Composed of:
- road id
- start location
- end location
- path points
- road profile

## World.Structure.RoadProfile
This class is a scriptable object.

Represents the authored cross-section used when stamping a road into the world.

This is where the road "layers" should live: the slice of blocks to apply around the road centerline.

Composed of:
- local width
- local depth
- voxel layout on local X and Y axes
- edge rules

## World.Structure.Building
This class is serializable.

Represents one placed building in the generated world.

Composed of:
- building id
- building prefab
- exterior world position
- linked interior

## World.Structure.Template
This class is a scriptable object.

Represents a reusable authored world pattern built from voxels or cells.

This is the generic version of a reusable structure. `BuildingPrefab`, `GymPrefab`, and `SceneryPrefab` are more explicit specializations of the same idea.

Composed of:
- template category
- voxel or module layout reference
- connection points
- local interactive objects
- allowed biome tags

## World.Structure.TownProfile
This class inherit from World.Structure.Template.

Represents the authored rules used to compose a generated town.

This is the place where the generator learns which building prefabs it may choose from, how many lots the town can have, and which road profile it should use.

Composed of:
- allowed building prefabs
- lot count range
- road profile
- scenery placement rules

## World.Structure.BuildingPrefab
This class inherit from World.Structure.Template.

Represents one authored building prefab that can be inserted into a generated town or location.

This is the explicit class for the workflow you described: the building is designed manually outside the run, then world generation picks one prefab and places it.

Composed of:
- voxel layout to stamp into the world
- entry position
- interior prefab
- local interactive objects
- placement tags

## World.Structure.GymPrefab
This class inherit from World.Structure.Template.

Represents one authored gym prefab, including its manual puzzle or handcrafted layout.

Composed of:
- voxel layout to stamp into the world
- entry position
- interior prefab
- local interactive objects
- gym theme tags

## World.Structure.PointOfInterestPrefab
This class inherit from World.Structure.Template.

Represents one authored prefab for a cave, ruin, pond, dungeon entrance, or other special generated place.

Composed of:
- voxel layout to stamp into the world
- entry position if enterable
- interior prefab if needed
- local interactive objects
- placement tags

## World.Structure.SceneryPrefab
This class inherit from World.Structure.Template.

Represents one authored scenery prefab used to diversify procedural generation.

This covers things like trees, rocks, ruins, statues, and other decorative or blocking elements.

Composed of:
- voxel layout to stamp into the world
- local interactive objects
- placement tags
- allowed biome tags

## World.Structure.InteriorPrefab
This class inherit from World.Structure.Template.

Represents one authored interior layout that can be instantiated when entering a building, gym, cave, or other structure.

Some interiors are safe spaces such as heal points, shops, or gym service areas.
Others can behave like a local biome, with their own wild encounter definition and trigger chance.

Composed of:
- voxel layout or room layout
- local interactive objects
- local trainer placements
- wild encounter data if this interior allows encounters

## World.Structure.InteriorSpace
This class is serializable.

Represents an interior map entered through a building or entrance trigger.

If the interior supports random encounters, that behavior should come from its `InteriorPrefab`, not from a separate encounter-zone list.

Composed of:
- interior id
- source building or entrance
- interior prefab
- generated interactive objects
- generated trainers

## World.Structure.TrainerPlacement
This class is serializable.

Represents one authored placement of a trainer inside an interior or other handcrafted structure.

This is the data you would place manually when building a gym, cave room, house, or other interior with trainer encounters.

Composed of:
- trainer
- local position
- facing direction if relevant

## World.Structure.InteractiveObjectDefinition
This class is a scriptable object.

Represents one reusable authored interaction definition for template-based world objects.

This is mainly intended for teleporter-like world interactions such as doors, cave entrances, exits, ladders, or similar transition points.
You define the interaction behavior once as an asset, then local interactive objects inside templates only reference it.

Composed of:
- display name
- interaction type
- default trigger mode
- default payload data
- validation or generation tags if needed

## World.Structure.InteractiveObject
This class is serializable.

Represents an authored local interactive object embedded inside a prefab, module, or template.

This is mainly used for door-like or entrance-like interactions that teleport the player to another place.
Typical examples are building doors that require a click interaction, and cave or dungeon entrances that trigger when the player walks onto them.
The interactive object itself does not need its own id if it already lives inside a parent prefab, module, or template.
Its identity can come from the parent asset plus its position in the local interactive object list.

Composed of:
- interactive object definition
- local position
- local direction if relevant
- trigger mode override if needed
- destination data or payload overrides if needed

## World.Structure.PlacedStructure
This class is serializable.

Represents one generated placement of a `World.Structure.Template` in the current run.

This exists separately from the template because one template can be placed many times in different runs and different positions, and can span more than one chunk.
This is the generic runtime type for placed prefab or template content in the voxel world.
It should be used for things like buildings, scenery, or prefab-based points of interest, but not for semantic world-map concepts such as roads, towns, or locations.

Composed of:
- placed structure id
- template
- world position
- rotation
- world bounds
- covered chunk coordinates
- local seed or variation id

## World.Structure.PlacementState
This class is serializable.

Represents the structure placement state of the generated world.

This is the place to track already placed structures, occupied areas, and cross-chunk structure data instead of storing that directly inside each chunk.
It should typically be owned by `World.Generator`, not by individual chunks.

Composed of:
- placed structures
- occupied or reserved world bounds
- preload or generation state flags

## World.Structure.PlacementRule
This class is a scriptable object.

Represents the rule used to decide where and how a prefab or template may be placed.

This is useful for scenery, buildings, points of interest, and other generated structures.

Composed of:
- source prefab or template
- weight
- allowed biome tags
- spacing constraints
- terrain constraints
- quantity or density data

## World.Chunk
Namespace grouping chunk-scoped world data.

### World.Chunk.Coordinates
This class is serializable.

Represents the chunk address in the world grid.

This should usually be used by `World.MapData` or `World.Generator` as the external key used to store and retrieve chunks, rather than being embedded inside each chunk data object.

Composed of:
- x
- y

### World.Chunk.Data
This class is serializable.

Represents the voxel content of one generated chunk.

This should stay as a pure voxel container.
It does not need to store its own coordinates if the world already indexes chunks by coordinates.
It also should not store biome data if biome sampling can vary inside the chunk.

Composed of:
- voxel cells storing compact voxel definition indexes

# Model.Voxel

## Voxel.Registry
This class is a scriptable object.

Represents the optional singleton registry used to resolve a compact voxel definition index to a `Voxel.Definition`.

This follows the previous `VoxelKit` approach and is especially useful because chunks contain large voxel arrays that should stay compact in memory and in serialization.
Unlike most authored definitions, voxel cells are numerous enough that keeping a compact numeric index such as a `ushort` is still preferable.

Composed of:
- voxel definition list
- lookup by voxel definition index
- optional fallback voxel definition

## Voxel.Definition
This class is a scriptable object.

Represents one authored voxel asset used by both exploration and battle.

This is the merged version of the old `Core.VoxelKit.Definition` and `Battle.Voxel.Definition`.
One voxel definition owns:
- gameplay data for traversal
- render and collision geometry
- surface geometry used to draw movement, selection, and combat masks on top of the same world voxel

Composed of:
- display name
- voxel data
- shape

## Voxel.Data
This class is serializable.

Represents gameplay and authoring data attached to a voxel definition.

This is also the right place to describe voxel-based world interactions such as cuttable trees, breakable rocks, or other obstacles that are removed or changed when the player uses a matching action on that voxel.
In that model, the interaction is resolved by checking the clicked voxel definition rather than by spawning a separate gate object.

Composed of:
- traversal type
- blocks line of sight flag
- movement cost
- surface tags
- optional world interaction tags
- optional replacement or removed state behavior
- material or texture data if needed

## Voxel.Cell
This class is serializable.

Represents one voxel cell stored in a chunk.

This is one of the few places where a compact voxel definition index is preferable to direct object composition, because the world contains many cells and they should serialize cheaply.
It should only store data that is intrinsic to the cell itself.
Its position should normally come from the chunk container or array index, not from the cell.
Per-cell state flags are only useful if you later need mutable voxel state that cannot be represented by simply replacing the voxel definition index.

Composed of:
- voxel definition index resolved through `Voxel.Registry`
- orientation
- flip orientation

## Voxel.Traversal
Represents the traversal category of a voxel.

Examples:
- walkable
- obstacle
- water
- climbable

## Voxel.Shape
This class is serializable.

Represents the canonical geometry source used to build render meshes, collision meshes, and surface masks for a voxel definition.

This should be the single geometry description used by both exploration and battle.
Since movement and selection masks may also be shown during exploration, there is no need for a separate battle-only overlay shape.
Following the old voxel implementation, concrete voxel shapes should also describe the sprites used on their faces so the mesher can bake the correct UVs directly into the mesh.

Composed of:
- render faces
- collision faces
- overlay faces
- cardinal point set
- sprite references used to build face UVs

## Voxel.CardinalPointSet
This class is serializable.

Represents the local placement anchors available on a voxel surface.

This is useful for placement, movement entry points, and aligning battle overlays to non-flat voxel shapes.

Composed of:
- positive X point
- negative X point
- positive Z point
- negative Z point
- stationary point

## Voxel.Mesher
Represents the runtime mesh builder used to generate voxel meshes and surface masks from voxel grids.

This should be the single mesher used for both exploration and battle presentation.
It should handle regular render geometry, collision geometry, and the mask geometry used for movement, targeting, placement, or selection overlays on the world itself.
It should not own the final render material.
The renderer can receive its material later, while the mesher only needs the geometry rules and the sprite lookup data required to bake UVs.
It also should not own the voxel registry or the mask sprite registry.
Instead, it should read the `Voxel.Cell` data from a chunk, resolve each voxel definition index through the external voxel registry, and return the mesh data requested by the caller.
If mask meshes are needed, the caller can provide the external mask sprite registry at build time.

Composed of:
- render mesh build rules
- collision mesh build rules
- overlay mask mesh build rules

## Voxel.MaskSpriteRegistry
This class is a scriptable object.

Represents the sprite lookup used when baking mask meshes.

The mesher receives a mask type and resolves the sprite that should be used for its UVs.
It should be shared by exploration movement masks, selection masks, and combat masks if they all use the same world overlay system.
This is an external lookup asset used by callers of the mesher, not data owned by the mesher itself.

Composed of:
- sprite by mask type

# Model.Creature

## Creature.Form
This class is serializable.

Represents one visual state owned by a creature species.

This is not a different species.
It is the cosmetic state applied to the unit by feat nodes.
The starting node of the board can apply the base form, while later branch nodes can apply alternate forms.
It should have a stable unique name inside the species so the save system can resolve it again after loading.

Composed of:
- unique form name used by the save system
- display name
- form tags
- icon sprite
- form tier
- model prefab
- animation set
- optional material or VFX overrides

## Creature.Species
This class is a scriptable object.

Represents the static data of one creature species.

At runtime, units should reference the species directly.
For saving and loading, the species should also expose a stable unique name used by the species registry.
The species should own all of its possible forms so the loader can resolve a saved `Species.Form` string by first resolving the species, then resolving the form inside that species.

Composed of:
- unique species name used by the save system
- display name
- base stats
- default actions
- feat board
- list of the forms of the species
- dictionnary linking a name to a form, used for the save/load system

## Creature.SpeciesRegistry
This class is a scriptable object.

Represents the singleton registry used by the save system to resolve species names back to species assets.

This should keep a list of all species assets, then build a dictionary from unique species name to `Creature.Species` at initialization time.
At save time, the unit asks its species for that unique name.
At load time, the saved species name is used to resolve the correct species asset through this registry.
If the save stores a `Species.Form` string, the loader should split it, resolve the species through this registry, then resolve the form through the species.

Composed of:
- list of all species assets
- dictionary from unique species name to species asset

## Creature.Unit
This class is serializable.

Represents one concrete creature instance.

This is the persistent or authored creature data used before battle state is created.
It is the base creature data used by the player, by storage, and by any system that does not need AI-specific behavior.
The species should be referenced directly.
The unit id is only useful as a stable identity if other runtime or save structures need to point back to this exact creature instance.
The save system should serialize the species unique name rather than trying to serialize the species asset reference itself.
For the current form, the save system can serialize `SpeciesName.FormName`, then rebuild the direct references during loading.

Composed of:
- optional unit id
- species
- current form of the unit
- nickname
- current feat board progress
- additional stats earned from completed feats
- unlocked actions
- persistent unit effects obtained through feat progression

## Creature.Stats
This class is serializable.

Represents a creature stat block.

Composed of:
- health
- strength
- ability
- armor
- resistance
- action points
- movement points
- stamina
- range

## PCStorage
This class is serializable.

Represents stored creatures not currently in the active team.

Composed of:
- stored units in a single array

# Model.Item

## Item.Definition
This class is a scriptable object.

Represents one abstract item type that can exist in the world or inventory.

This should be the abstract base of an item-definition hierarchy.
Concrete item subtypes should describe what the item actually does.

Composed of:
- display name
- description
- icon
- stackable flag
- max stack if relevant
- consumable flag

## Item.BattleItem
This class inherit from `Item.Definition`.
This class is a scriptable object.

Represents one item used during battle, such as a potion, a revive, or a capture item if that mechanic exists.

Composed of:
- inherited item data
- battle targeting rules
- battle effects

## Item.PassiveItem
This class inherit from `Item.Definition`.
This class is a scriptable object.

Represents one item used outside battle on a creature to grant bonus stats, passive effects, or temporary/permanent bonuses.

Composed of:
- inherited item data
- granted stat bonuses if relevant
- granted passive effects if relevant
- duration mode if relevant

## Item.ActionItem
This class inherit from `Item.Definition`.
This class is a scriptable object.

Represents one item used on a creature to make it learn a new action.

Composed of:
- inherited item data
- action to teach
- target creature filters if relevant

## Item.KeyItem
This class inherit from `Item.Definition`.
This class is a scriptable object.

Represents one non-battle progression or interaction item.

Composed of:
- inherited item data
- world usage or progression meaning

## Item.Inventory
This class is serializable.

Represents the player's stored items.

Composed of:
- item stacks

## Item.Stack
This class is serializable.

Represents a quantity of one item inside the inventory.

Composed of:
- item
- quantity

# Model.Action

## Action.Definition
This class is a scriptable object.

Represents one creature ability that can be used in battle, and optionally in exploration too.

This is kept outside `Battle` because the same action may need to be referenced by species data, feat nodes, battle commands, and world interactions.
This is the main action asset you should author in Unity, then reference from species default actions, unlocked unit actions, and feat nodes that grant new actions.
The action should hold a list of `Action.Effect`, and the inspector should let you choose which concrete effect subclass to add.
In Unity, this implies a custom editor or property drawer for the action effect list.
When adding a new effect, the editor should first let you choose the effect subtype through a selector or enum-like menu, then instantiate the matching concrete effect class and expose only its relevant fields in the inspector.
The same authoring pattern should also be used for action activation conditions, so an action can declare when it is currently usable.

Composed of:
- display name
- ap cost
- range data
- targeting profile
- cast profile
- activation conditions if relevant
- line of sight rule
- list of polymorphic effects stored through managed-reference serialization
- pending cast animation name to request on the caster if relevant
- cast release animation name to request on the caster if relevant
- usable in battle flag
- usable in world flag
- world interaction tags

## Action.TargetingProfile
This class is serializable.

Represents how an action selects valid targets or cells.

This should usually be embedded directly inside `Action.Definition`, not authored as a standalone asset.
It only needs to be serializable so each action can tune its own targeting rules in the inspector.
If different targeting modes need different fields, this should also use a custom inspector or property drawer so the action editor stays easy to use.
It should describe what kind of runtime `Battle.TargetSelection` the action expects, including actions that select more than one board cell or more than one unit.

Examples:
- straight line
- free target cell
- two selected board cells
- area of effect
- self
- ally
- enemy

Composed of:
- expected target selection kind
- minimum selected targets
- maximum selected targets
- selection order or duplicate-selection rule if relevant
- area shape if relevant
- filters

## Action.CastProfile
This class is serializable.

Represents how an action resolves in time instead of assuming every action happens immediately.

This is the right place for mechanics such as:
- immediate actions
- actions that are prepared now and released on a later turn
- actions that block the caster while waiting
- actions that resolve now but impose a recovery delay afterward

This should stay at the action-definition level, not inside `Action.Effect`, because it controls when the action resolves, whether the caster is blocked, and whether target data is stored for later resolution.

Composed of:
- cast mode such as immediate, delayed cast, or immediate with recovery
- delay value in turns or seconds if relevant
- target lock mode such as choose at start or choose at release
- block caster while pending flag
- recovery value in turns or seconds if relevant
- cancel conditions such as on move, on damage, or on death

## Action.ActivationCondition
This class is serializable.

Represents one condition that must pass for an action to be usable.

This should be the abstract base of a polymorphic activation-condition hierarchy, authored with managed-reference serialization and a custom inspector.
This is the right place for checks such as required form tags, forbidden form tags, or required unit-effect stack counts on the acting creature.

Composed of:
- the data shared by all activation-condition subtypes if any

## Action.RequiredFormTagCondition
This class is serializable.
This class inherits from `Action.ActivationCondition`.

Represents an activation condition requiring the acting creature's current form to have one or several matching form tags.

Composed of:
- required form tags
- match-all or match-any rule if relevant

## Action.ForbiddenFormTagCondition
This class is serializable.
This class inherits from `Action.ActivationCondition`.

Represents an activation condition forbidding the acting creature's current form from having one or several matching form tags.

Composed of:
- forbidden form tags
- match-all or match-any rule if relevant

## Action.UnitEffectStackCondition
This class is serializable.
This class inherits from `Action.ActivationCondition`.

Represents an activation condition requiring the acting creature to currently have a certain number of stacks of one matching unit effect.

This is the main condition to support actions that build stacks first, then can only be used once enough stacks are present.

Composed of:
- unit effect filters
- minimum required stack count

## Action.Effect
This class is serializable.

Represents one effect applied by an action.

This should be the abstract base of a polymorphic effect hierarchy, authored with managed-reference serialization and a custom inspector.
That is a better fit than one rigid enum-only structure, because actions may need custom behaviors such as "reduce all damage received by half", "deal damage based on MP consumed", or "repeat damage once per stack already on the target".

Examples of concrete effect subclasses:
- `Action.DamageEffect`
- `Action.HealEffect`
- `Action.ReviveEffect`
- `Action.ApplyUnitEffect`
- `Action.RemoveUnitEffect`
- `Action.CleanseEffect`
- `Action.CreateBoardEffect`
- `Action.RemoveBoardEffect`
- `Action.ResourceChangeEffect`
- `Action.MoveUnitEffect`
- `Action.SwapPositionEffect`
- `Action.TeleportEffect`
- `Action.RecordPositionEffect`
- `Action.StealResourceEffect`
- `Action.ConsumeUnitEffect`
- `Action.ConditionalEffect`
- `Action.ChangeFormEffect`

Composed of:
- the data shared by all effect subtypes if any

## Action.ValueFormula
This class is serializable.

Represents a reusable numeric formula used by action effects.

This is what should let an effect say things like:
- deal a fixed amount of damage
- deal damage per MP consumed
- repeat an effect once per unit-effect stack on the target
- scale healing or shielding from source or target data

This should be the abstract base of a polymorphic formula hierarchy, authored with managed-reference serialization and a custom inspector driven by the selected formula type.

Examples of concrete formula subclasses :
- `Action.ConstantFormula`
- `Action.StatFormula`
- `Action.ResourceConsumedFormula`
- `Action.StackCountFormula`
- `Action.AddFormula`
- `Action.MultiplyFormula`

Composed of:
- the data shared by all formula subtypes if any

## Action.ConstantFormula
This class is serializable.

Represents a formula that always returns a fixed numeric value.

Composed of:
- constant value

## Action.StatFormula
This class is serializable.

Represents a formula that reads a stat from the source unit or target unit.

Composed of:
- stat source such as source or target
- stat type
- multiplier if relevant

## Action.ResourceConsumedFormula
This class is serializable.

Represents a formula based on how much of a resource was consumed while resolving the action.

Examples:
- damage per MP consumed
- healing per AP spent

Composed of:
- consumed resource type
- multiplier

## Action.StackCountFormula
This class is serializable.

Represents a formula based on the stack count of a unit effect currently present on a unit.

Examples:
- repeat once per poison stack on the target
- increase damage per burn stack on the target

Composed of:
- stack source such as source or target
- referenced unit effect
- multiplier if relevant

## Action.AddFormula
This class is serializable.

Represents a formula that adds the results of several child formulas together.

Composed of:
- child formulas

## Action.MultiplyFormula
This class is serializable.

Represents a formula that multiplies the results of several child formulas together.

Composed of:
- child formulas

## Action.DamageEffect
This class is serializable.

Represents an effect that deals damage to the affected target or targets.

Composed of:
- damage formula
- damage type

## Action.HealEffect
This class is serializable.

Represents an effect that restores health to the affected target or targets.

Composed of:
- heal formula

## Action.ReviveEffect
This class is serializable.

Represents an effect that revives one defeated target if that mechanic exists.

Composed of:
- revived health formula
- optional target filters

## Action.ApplyUnitEffect
This class is serializable.

Represents an effect that applies an ongoing `UnitEffect` to the affected target.

This is the right place for `UnitEffect` like poison, stun, shielded, armor up, resistance down, lifesteal, or other buffs and debuffs when they are being applied by an action.
Applying a unit effect should always add one or more stacks.
If the target does not already have the matching active unit effect, the action should create it first, then add the stacks.

Composed of:
- unit effect
- unit effect duration override if relevant
- stack change if relevant

## Action.RemoveUnitEffect
This class is serializable.

Represents an effect that removes one or several existing unit effects from the affected target.

Composed of:
- unit effect filters
- amount to remove if relevant

## Action.CleanseEffect
This class is serializable.

Represents an effect that removes unwanted unit effects or similar temporary modifiers.

Composed of:
- filters deciding what kinds of unit effects can be removed
- amount to remove if relevant

## Action.ResourceChangeEffect
This class is serializable.

Represents an effect that changes a resource such as action points, movement points, health, shields, or similar battle values.

Composed of:
- resource type
- signed value formula

## Action.CreateBoardEffect
This class is serializable.

Represents an effect that creates an ongoing effect on battle board cells or on an area of the board.

This is the general tool for traps, delayed explosions, poison clouds, healing zones, flame walls, and similar persistent action results that are attached to the board rather than to a unit.

Composed of:
- board effect
- placement or area rule if relevant
- board effect duration override if relevant

## Action.RemoveBoardEffect
This class is serializable.

Represents an effect that removes one or several active board effects from the board.

Composed of:
- board effect filters
- amount to remove if relevant

## Action.MoveUnitEffect
This class is serializable.

Represents an effect that moves one target unit.

This should cover push, pull, knockback, dash, teleport-like forced movement, or similar repositioning mechanics that are still resolved through movement rules.

Composed of:
- movement mode
- distance formula
- destination or direction rule
- collision rule if relevant

## Action.SwapPositionEffect
This class is serializable.

Represents an effect that swaps the positions of two units.

Composed of:
- target filters
- optional placement validation rules

## Action.TeleportEffect
This class is serializable.

Represents an effect that places one target unit directly at one destination board coordinate.

Composed of:
- destination rule
- optional placement validation rules

## Action.RecordPositionEffect
This class is serializable.

Represents an effect that records one board position for later use by another action such as a return or teleport.

Composed of:
- recorded position source
- record target such as self or one target

## Action.StealResourceEffect
This class is serializable.

Represents an effect that transfers one resource from one target to another.

Composed of:
- resource type
- stolen amount formula
- source and destination filters

## Action.ConsumeUnitEffect
This class is serializable.

Represents an effect that consumes one or several unit effects or stacks to produce another result.

Composed of:
- unit effect filters
- consume rule
- child effect or value formula produced by the consumption

## Action.ConditionalEffect
This class is serializable.

Represents an effect that resolves different child effects depending on whether one condition passes.

Composed of:
- condition data
- success effects
- failure effects if relevant

## Action.ChangeFormEffect
This class is serializable.

Represents an effect that changes the current active form of the acting or targeted creature during battle.

This is useful for creatures that switch between forms such as a ranged form and a close-combat form.
Actions that should only be usable in some forms should rely on the action definition's activation conditions, especially form-tag conditions.

Composed of:
- target form
- target filters if relevant

## Action.UnitEffect
This class is serializable.

Represents one ongoing effect that can affect a unit.

This is the general embedded data package used for poison, stun, lifesteal, damage bonuses, resistances, shields, and similar ongoing modifiers.
It should usually be authored directly inside `Action.ApplyUnitEffect`, feat node data, species data, or other owner data rather than as a standalone asset.
It can be granted permanently by species data or feat nodes, or temporarily during battle by an action.
Its runtime application should be represented by `Battle.AppliedUnitEffect`.
Its behavior should come from a list of effect rules that battle systems evaluate at specific hook points.

Examples:
- poison
- stun
- poison immunity
- lifesteal
- healing bonus
- range bonus

Composed of:
- display name
- default unit effect duration
- effect rules

## Action.UnitEffectDuration
This class is serializable.

Represents how long a unit effect should remain active.

This should support permanent effects from species or feats, turn-based effects like poison, and time-based effects like temporary buffs that expire after a number of seconds.

Composed of:
- duration mode such as permanent, turn-based, or time-based
- duration value

## Action.BoardEffect
This class is serializable.

Represents one ongoing effect attached to battle board cells or to a board area.

This is the general embedded data package used for traps, delayed explosions, poison clouds, healing zones, flame walls, and similar persistent board-side modifiers.
It should usually be authored directly inside `Action.CreateBoardEffect` or other owner data rather than as a standalone asset.
Its runtime application should be represented by `Battle.AppliedBoardEffect`.
Its behavior should come from a list of board-effect rules that battle systems evaluate at specific board and timing hook points.

Examples:
- trap
- delayed explosion
- poison cloud
- healing zone
- flame wall

Composed of:
- display name
- default board effect duration
- board effect rules

## Action.BoardEffectDuration
This class is serializable.

Represents how long a board effect should remain active.

This should support board effects that last until triggered, effects that expire after a number of turns, and effects that expire after a number of seconds.

Composed of:
- duration mode such as until-triggered, turn-based, or time-based
- duration value

## Action.BoardEffectRule
This class is serializable.

Represents one rule contained inside a board effect.

This should be the abstract base of a polymorphic board-effect-rule hierarchy, authored with managed-reference serialization and a custom inspector.
Each rule should describe both when it is evaluated and what it does on the board or to units interacting with the board effect.

Examples of concrete board effect rule subclasses:
- `Action.TriggerOnEnterRule`
- `Action.TriggerOnTurnStartInsideRule`
- `Action.TriggerOnTimerEndRule`
- `Action.TriggerOnExitRule`
- `Action.ApplyEffectsToUnitsInAreaRule`
- `Action.DestroyAfterTriggerRule`

Composed of:
- hook point
- filters deciding when the rule applies
- child effects or child actions depending on the rule subtype

## Action.BoardEffectHookPoint
Represents the enum-like list of hook points used by board effect rules.

Examples:
- when the board effect is created
- when its timer ends
- when a unit enters the affected cells
- when a unit starts its turn in the affected cells
- when a unit ends its turn in the affected cells
- when a unit leaves the affected cells
- when the board effect is triggered

## Action.UnitEffectHookPoint
Represents the enum-like list of hook points used by unit effect rules.

Examples:
- before validating whether an action can be used
- before validating targets or affected cells
- before paying a resource cost
- before dealing outgoing damage
- before receiving incoming damage
- before applying an effect
- at turn start
- at turn end
- when moving
- when consuming a resource

## Action.UnitEffectRule
This class is serializable.

Represents one rule contained inside a unit effect.

This should be the abstract base of a polymorphic effect-rule hierarchy, authored with managed-reference serialization and a custom inspector.
Each rule should describe both when it is evaluated and what it changes.
The battle flow should consult active effect rules at the relevant moments, instead of trying to hardcode all ongoing effect behavior in one big damage or action method.

Examples of concrete effect rule subclasses:
- `Action.ModifyOutgoingDamageRule`
- `Action.ModifyIncomingDamageRule`
- `Action.ModifyActionCostRule`
- `Action.ModifyRangeRule`
- `Action.PreventEffectApplicationRule`
- `Action.LifestealRule`
- `Action.TriggerOnEventRule`

Composed of:
- hook point
- filters deciding when the rule applies
- value formula or child effect depending on the rule subtype

# Model.Feat

## Feat.Board
This class is a scriptable object.

Represents the full feat board layout used by a creature line.

This should stay separate from progress data because the board layout is shared by every unit using that board.

If form branches live inside the same board, the board should keep the full tree while the current form choice is stored per unit.
The board should reference the species forms through node data rather than owning a second copy of those forms.
This should be authored as a polymorphic list of nodes with a custom Unity editor, so the creator can choose which concrete node subtype to add to the board.

Composed of:
- nodes
- starting node

## Feat.Node
This class is serializable.

Represents one node on a feat board.

This should stay separate from node progress for the same reason: the node definition is shared, while progress is creature-specific.
This should be the abstract base of a polymorphic node hierarchy, authored with managed-reference serialization and a custom Unity editor.
The custom editor should let the creator choose the concrete node subtype to add when composing the board.
Inside each node, the requirement list should also be authored as a polymorphic managed-reference list so one node can contain one or several concrete requirement subtypes.

Composed of:
- display name
- board position
- requirements
- adjacent nodes

## Feat.ActionNode
This class is serializable.

Represents a feat node that unlocks a new battle action for the creature.

Composed of:
- inherited node data
- action definition to unlock

## Feat.StatNode
This class is serializable.

Represents a feat node that grants bonus stats to the creature.

Composed of:
- inherited node data
- stat bonuses

## Feat.PassiveNode
This class is serializable.

Represents a feat node that grants a permanent unit effect to the creature.

Composed of:
- inherited node data
- unit effect to grant permanently

## Feat.FormNode
This class is serializable.

Represents a feat node that changes the current creature form.

For form-change nodes only, unlock validation should compare the unit's current form tier to the target form tier.
If the current form tier is already greater than or equal to the target form tier, the node cannot be unlocked.
The starting or center node can also be the form node that applies the base form to the unit.

Composed of:
- inherited node data
- form name to apply, resolved through the species

## Feat.Link
This class is serializable.

Represents an explicit connection between two feat nodes.

This can be omitted if adjacency is stored directly on `Feat.Node`, but it is useful if board links later need metadata.

Composed of:
- source node
- target node

## Feat.Requirement
This class is serializable.

Represents a condition needed to complete a feat node.

This is where action-based progression lives.
This should be the abstract base of a polymorphic requirement hierarchy, authored with managed-reference serialization and a custom inspector.
That inspector should let the creator add one or several concrete requirement subtypes inside one node, then configure only the fields relevant to each requirement.
Many requirement subtypes should also use shared enum-like helper types such as requirement scope or damage-type filters, instead of creating a separate concrete requirement class for every "single fight", "single turn", or "single attack" variant.

Composed of:
- the data shared by all requirement subtypes if any

## Feat.RequirementScope
Represents the enum-like scope used by feat requirements.

This is the shared scope type that should let one requirement mean:
- total progress across all battles
- progress within a single battle
- progress within a single turn
- progress within a single action resolution

If a requirement also needs things like consecutive tracking, that should usually be expressed by extra fields on the concrete requirement subtype rather than inside the scope type itself.

Examples:
- lifetime
- single battle
- single turn
- single action

## Feat.DamageTypeFilter
Represents the enum-like filter used by damage-related feat requirements.

Examples:
- physical only
- magical only
- both or any

## Feat.ActionUseRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to use one or several matching actions a certain number of times.

This is the requirement family that should also cover cases like:
- use matching actions 20 times total
- use one matching action 5 times in one battle
- use one matching action 3 times in one turn

Composed of:
- action filters
- target use count
- requirement scope
- optional battle context filters

## Feat.ActionHitRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to hit targets with one or several matching actions a certain number of times.

Composed of:
- action filters
- target hit count
- requirement scope
- optional target filters
- optional battle context filters

## Feat.ActionTargetCountRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to hit or select a certain number of targets or board cells with one action use.

This is useful for area attacks, split attacks, or actions that select several different board cells.

Composed of:
- action filters
- minimum selected or affected target count
- requirement scope
- optional target-selection filters

## Feat.ActionSequenceRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to use a specific ordered sequence of actions.

Composed of:
- ordered action filters
- target sequence count
- requirement scope
- optional maximum delay between sequence steps

## Feat.DamageDealtRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to deal a certain amount of damage.

This is the requirement family that should also cover cases like:
- deal 1000 damage total
- deal 200 damage in a single battle
- deal 80 damage in a single turn
- deal 40 damage in a single attack

Composed of:
- target damage amount
- damage type filter
- requirement scope
- optional action filters
- optional target filters

## Feat.DamageTakenRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to receive a certain amount of damage.

Composed of:
- target damage amount
- damage type filter
- requirement scope
- optional source filters
- optional battle context filters

## Feat.HealingDoneRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to heal a certain amount of health.

Composed of:
- target healing amount
- requirement scope
- optional action filters
- optional target filters

## Feat.ResourceSpentRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to spend a certain amount of one resource such as AP or MP.

Composed of:
- resource type
- target spent amount
- requirement scope
- optional action filters

## Feat.UnitEffectAppliedRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to apply one or several matching unit effects to targets a certain number of times.

Composed of:
- unit effect filters
- target application count
- requirement scope
- optional action filters

## Feat.UnitEffectReceivedRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to receive one or several matching unit effects a certain number of times.

Composed of:
- unit effect filters
- target receive count
- requirement scope
- optional source filters

## Feat.CleanseRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to remove one or several matching unit effects.

Composed of:
- unit effect filters
- target cleanse count
- requirement scope
- optional action filters

## Feat.UnitDefeatedRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to defeat a number of targets.

Composed of:
- target count
- requirement scope
- optional target filters
- optional battle context filters

## Feat.CaptureCountRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to participate in or cause a number of captures.

Composed of:
- target capture count
- requirement scope
- optional target filters

## Feat.BattleCountRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to participate in a number of battles.

Composed of:
- target battle count
- optional battle context filters

## Feat.BattleVictoryRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to win a number of battles.

Composed of:
- target victory count
- optional battle context filters

## Feat.MoveDistanceRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to move a certain distance in battle.

Composed of:
- target moved distance
- requirement scope
- optional battle context filters

## Feat.KeepDistanceRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to stay at least a certain number of cells away from every enemy.

This is the exotic positioning requirement family that should cover cases like:
- stay at least 3 cells away from all enemies for 5 turns
- finish 3 battles while always staying at least 2 cells away from every enemy

Composed of:
- minimum distance from every enemy
- target safe turn count or battle count
- requirement scope
- optional consecutive requirement flag
- optional battle context filters

## Feat.AdjacentToEnemyRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to end turns or perform actions while adjacent to one enemy.

Composed of:
- target adjacency count
- requirement scope
- optional action filters
- optional target filters

## Feat.BoardAreaOccupationRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to spend time inside or outside a matching board area.

Composed of:
- board area filters
- target occupation count or duration
- requirement scope

## Feat.BoardEffectTriggeredRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to trigger one or several matching board effects.

Composed of:
- board effect filters
- target trigger count
- requirement scope
- optional action filters

## Feat.NoDamageBattleRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to finish battles without taking damage.

Composed of:
- target battle count
- optional battle context filters

## Feat.LowHealthSurvivalRequirement
This class is serializable.
This class inherits from `Feat.Requirement`.

Represents a requirement asking the creature to finish battles below a health threshold while still surviving.

Composed of:
- health threshold value
- target battle count
- optional battle context filters

## Feat.BoardProgress
This class is serializable.

Represents one creature's saved progress on its feat board.

This is the creature-specific layer that references `Feat.Board`, rather than duplicating the whole board definition into every owned unit.
It is the source of truth for which feat nodes were completed.
If you prefer not to derive bonuses every time, the owned unit can also cache the resulting additional stats and unlocked effects.

Composed of:
- board
- completed nodes
- active or reachable nodes
- per-node progress entries

## Feat.NodeProgress
This class is serializable.

Represents progress made toward one node on one creature.

This is the creature-specific state attached to one shared `Feat.Node`.

Composed of:
- node
- completion flag
- per-requirement current values

# Model.Encounter

## Encounter.Definition
This class is serializable.

Represents the authored encounter data used by anything that can start a battle.

This should be the single encounter data class referenced by wild zones, trainer entities, rare spots, or other world trigger sources.
Fixed behavior such as capture availability or repeatability should be owned by the surrounding battle context, trainer state, or event system rather than by the encounter definition itself.
This should stay serializable because it is intended to be authored directly inside another world or biome definition, rather than as a standalone `ScriptableObject` asset.

Composed of:
- battle board configuration
- encounter table

## Encounter.Unit
This class inherit from `Creature.Unit`.
This class is serializable.

Represents one authored enemy unit configuration used by encounters.

This allows you to reuse all normal creature data while adding the AI behavior needed for enemy-controlled fights.

Composed of:
- inherited creature unit data
- AI profile

## Encounter.Team
This class is serializable.

Represents one authored enemy team used by encounters.

Composed of:
- units

## Encounter.Table
This class is a scriptable object.

Represents the container of all progression-based encounter tiers for one biome or one special area.

This is the object that maps the player's current badge count to the correct weighted encounter tier.
With a serializable dictionary available in Unity, this should simply be authored as a dictionary from badge count to `Encounter.Tier`.

Composed of:
- serialized dictionary from badge count to encounter tier

## Encounter.Tier
This class is serializable.

Represents the weighted list of possible encounter teams.

This matches the GDD idea of one list per gym count, plus an optional post-Elite-Four list.
Random selection should be weight-based, not percentage-based.

Composed of:
- encounter teams linked to encounter weights

# Model.Battle

Unless explicitly marked otherwise, classes in this namespace should be read as runtime battle classes rather than Unity-authored serializable data.

## Battle.Input
Represents the resolved fight that is currently being played.

This should store the concrete battle input already chosen for this fight, not the full authored encounter source that may have produced it.

Composed of:
- enemy encounter team
- battle type
- source world position
- source voxel area
- capture allowed flag

## Battle.State
Represents the full runtime state of one active battle.

Composed of:
- battle input
- board
- teams
- placement state if battle setup is still running
- turn entries
- active unit
- battle result if finished

## Battle.Team
Represents one side of the battle.

Composed of:
- side
- battle units

## Battle.Unit
Represents one creature as it exists inside a battle.

This is separate from `Creature.Unit` because battle needs temporary values that should not overwrite the persistent creature directly.
This is the in-battle instance built from a source `Creature.Unit`, plus any battle-only state such as current health, current effects, and position.
If the source unit is actually an `Encounter.Unit`, its AI behavior can be read from that authored encounter unit data.

Composed of:
- source creature unit
- current health
- current action points
- current movement points
- board position
- current applied unit effects
- current pending action if any
- blocked or recovering flag if relevant
- alive or defeated flag

## Battle.BoardConfiguration
This class is serializable.

Represents the authored rule used to extract and compose a playable battle board from the current voxel scene.

This is not a separate battle map.
It is the battle-side configuration that an `Encounter.Definition` should embed in order to describe how the current overworld or interior voxels are converted into a combat board.
This is the right place to describe things like open-field boards, tighter cave boards, different board sizes or shapes, and the list of placement policies that are allowed for this encounter.

Composed of:
- board source mode
- board size or bounds rule
- board shape rule if relevant
- walkable surface sampling rule
- source world or interior sampling rule
- trigger bounds or source interaction bounds if relevant
- acceptable player deployment pattern types
- acceptable enemy deployment pattern types
- pattern weights or priorities if relevant
- auto-placement behavior if relevant
- dim outside area flag

## Battle.Board
Represents the actual playable combat area extracted from the current world or interior voxels.

This should be derived from the current voxel scene, not from a separate battle-only area.
It should not duplicate voxel-owned gameplay data into a separate battle cell type.
Things like movement cost or line-of-sight blocking should be resolved from the world voxel data through the board-to-world mapping.
Battle-specific runtime data should stay in `Battle.BoardCell` objects rather than inside the voxel model itself.

Composed of:
- source voxel bounds
- battle board cells indexed by board coordinates

## Battle.BoardCell
Represents the battle-only runtime state attached to one playable board coordinate.

This is the renamed replacement for the old `Battle.Cell` concept.
It should not duplicate voxel-owned gameplay data.
Instead, it should store only the battle-side data needed for masks, occupancy, and board effects.
Its position should come from the `Battle.Board` container that indexes board cells by board coordinates.
This should usually stay as a runtime class rather than a serializable data class, since it is generated by the battle system and does not normally need to be edited in Unity or saved directly.

Composed of:
- list of masks applied to this board cell
- occupying battle unit if any
- board effects affecting this board cell

## Battle.OverlayMaskType
Represents the categories of masks that can be drawn on top of battle board cells.

Examples:
- deployment
- movement range
- action range
- selection
- target preview

## Battle.DeploymentZone
Represents one resolved set of board coordinates where one side is allowed to place creatures at battle start.

This should be the runtime result produced by `Battle.PlacementResolver` from the current board and board configuration.
It should not own the pattern-generation logic itself; it should only store the concrete allowed coordinates that were resolved for this battle.
The chosen pattern type does not need to be stored here once the coordinates are already known.

Composed of:
- side
- allowed board coordinates

## Battle.DeploymentPatternType
Represents the enum-like type of deployment pattern that `Battle.PlacementResolver` can generate.

Examples:
- split : half the spaces for the player, the other for the enemy
- spot : Place some random spaces as "roots" for player and enemy, than grow them to form "patch" of placement spaces
- random : Randomly place space for player and enemy
- quarter : split the board in 4 sections on one axis, give the first tot he player, and the 3th to the enemy
- ambush : Place the player at the center of the board, the enemy around it
- surprise : Place the enemy at the center of the board, the player around it

## Battle.PlacementResolver
Represents the runtime helper that generates deployment zones for the current battle.

This should take the current `Battle.Board` and the `Battle.BoardConfiguration` chosen by the encounter, choose valid deployment pattern types from the configured lists, and resolve them into concrete `Battle.DeploymentZone` results.
This is the right place for the shared board-placement logic, so encounters do not need to reconfigure each pattern by hand.

Composed of:
- shared deployment generation rules if centralized
- pattern validation logic
- deployment zone generation logic

## Battle.PlacementState
Represents the current deployment phase before the first active turn starts.

Composed of:
- resolved deployment zones by side
- current placed battle units during deployment
- enemy placement completion flag
- current setup step

## Battle.TurnEntry
Represents one creature's progress on the stamina turn bar.

Composed of:
- battle unit
- current fill or progress
- ready flag
- paused flag

## Battle.BattleCommand
Represents a chosen command for a unit turn.

This should be the abstract base class for concrete battle command types.

Composed of:
- acting battle unit
- command type

## Battle.TargetSelection
Represents the abstract base runtime payload describing what an action is targeting.

This is the object that should be carried by `Battle.ActionCommand` and `Battle.PendingAction`.
It should support actions that target one board cell, several board cells, one unit, several units, one direction, or one path.

Composed of:
- the data shared by all target-selection subtypes if any

## Battle.BoardCellTargetSelection
Represents one target selection made of one or several selected board coordinates.

This is the runtime target payload to use for actions that can select multiple board cells, such as an action that strikes two different spaces.

Composed of:
- selected board coordinates

## Battle.UnitTargetSelection
Represents one target selection made of one or several selected battle units.

Composed of:
- selected battle units

## Battle.DirectionTargetSelection
Represents one target selection made of a chosen direction from a source board coordinate.

Composed of:
- source board coordinate
- chosen direction

## Battle.PathTargetSelection
Represents one target selection made of a chosen board path.

Composed of:
- path coordinates

## Battle.MoveCommand
This class inherits from `Battle.BattleCommand`.

Represents a movement command.

This should usually store only the intended destination.
The battle system should compute the path and movement cost when validating or resolving the command, then accept or reject the move based on the unit's current movement points.

Composed of:
- inherited command data
- destination board coordinate

## Battle.ActionCommand
This class inherits from `Battle.BattleCommand`.

Represents a combat action command.

Composed of:
- inherited command data
- action definition
- target selection

This command may either resolve immediately or create a `Battle.PendingAction`, depending on the action cast profile.

## Battle.PendingAction
Represents one action that has been chosen but has not resolved yet.

This is the runtime state used for delayed casts, charged attacks, or recovery-style actions that lock the caster across turns or across a timed delay.

Composed of:
- source battle unit
- action definition
- stored target selection if chosen at cast start
- delay duration state
- blocked caster flag
- recovery duration state if relevant
- cancel conditions copied from the action cast profile if needed

## Battle.CaptureCommand
This class inherits from `Battle.BattleCommand`.

Represents a wild creature capture attempt.

Composed of:
- inherited command data
- target battle unit

## Battle.CaptureRule
Represents the data needed to validate and resolve a capture attempt.

Composed of:
- hp threshold rule
- success chance rule
- allowed encounter categories

## Battle.DurationState
Represents one shared runtime duration state used by battle systems.

This should be the common runtime object used for delayed actions, applied unit effects, and applied board effects, instead of duplicating separate remaining-turn and remaining-second fields on each class.
It is the right place to store the current timing state and let battle flow update it consistently.

Composed of:
- duration mode
- remaining turns if turn-based
- remaining seconds if time-based
- expired flag if relevant

## Battle.DurationController
Represents the runtime helper that updates battle duration states.

This is the class that should advance durations during turn flow or time flow, then report or apply expiration when a `Battle.DurationState` reaches its end.
The exact method names can vary, but the controller should expose an API close to:
- `CreateStateFromDuration(...)` if you want the controller to build runtime duration states from authored duration data
- `AdvanceTurn(Battle.DurationState durationState, int turnCount = 1)`
- `AdvanceTime(Battle.DurationState durationState, float seconds)`
- `MarkTriggered(Battle.DurationState durationState)` if some durations end only when triggered
- `IsExpired(Battle.DurationState durationState)`

Composed of:
- duration update logic
- expiration check logic

## Battle.AppliedUnitEffect
Represents one ongoing unit effect currently affecting a battle unit.

This is the runtime state of a unit effect granted by species data, feat progression, or a temporary battle effect.
Battle systems should read the active applied unit effects on a unit and evaluate their effect rules at the relevant hook points during action resolution, damage resolution, effect application, turn flow, movement, or time updates.

Composed of:
- unit effect
- source battle unit if relevant
- duration state
- current stacks if relevant

## Battle.AppliedBoardEffect
Represents one ongoing board effect currently active on battle board cells or on a board area.

This is the runtime state of a board effect created by an action, such as a trap, delayed explosion, poison cloud, healing zone, or flame wall.
Instances of this class should be created when an `Action.CreateBoardEffect` resolves.
Battle systems should update these instances during turn flow, time flow, and board interaction checks, then evaluate their board-effect rules at the relevant hook points.

Composed of:
- board effect
- source battle unit
- affected board coordinates
- duration state
- triggered flag if relevant

## Battle.Result
Represents the outcome of a finished battle.

This should only store the result data that still matters after the battle ends.
Since your battle damage and defeats do not persist outside the fight, it does not need to keep a list of defeated units.

Composed of:
- winning side
- captured unit if any
- feat progress summary
- clear flags to apply after battle

## Battle.AI
Namespace grouping battle AI data.

### Battle.AI.Profile
This class is a scriptable object.

Represents the behavior script assigned to one enemy creature or trainer-controlled creature.

Composed of:
- ordered rules

### Battle.AI.Rule
This class is serializable.

Represents one top-down decision rule.

Rules should usually be evaluated in the order they appear inside `Battle.AI.Profile`, so a separate priority field is not necessary unless you later want a more dynamic sorting system.
Its condition list should usually be stored as a polymorphic managed-reference list with a custom inspector, so each rule can combine different condition subtypes cleanly.

Composed of:
- conditions
- action choice data

### Battle.AI.Condition
This class is serializable.

Represents one test used by an AI rule.

This should be the abstract base of a polymorphic AI-condition hierarchy, authored with managed-reference serialization and a custom inspector.
That inspector should let you choose the condition subtype you want to add, then expose only the fields relevant to that subtype.

Composed of:
- the data shared by all AI condition subtypes if any

### Battle.AI.EnemyWithinRangeCondition
This class is serializable.
This class inherits from `Battle.AI.Condition`.

Represents a condition checking whether one enemy can currently be reached or targeted in range.

Composed of:
- action or range source if relevant
- target filters

### Battle.AI.AllyHealthBelowCondition
This class is serializable.
This class inherits from `Battle.AI.Condition`.

Represents a condition checking whether one ally is below a health threshold.

Composed of:
- health threshold value
- target filters

### Battle.AI.ResourceAtLeastCondition
This class is serializable.
This class inherits from `Battle.AI.Condition`.

Represents a condition checking whether one resource is at least a required value, such as enough AP or MP to use something.

Composed of:
- resource type
- minimum required value

### Battle.AI.TargetHasUnitEffectCondition
This class is serializable.
This class inherits from `Battle.AI.Condition`.

Represents a condition checking whether a possible target has or does not have one unit effect.

Composed of:
- unit effect filters
- presence or absence rule
- target filters

### Battle.AI.CanUseActionCondition
This class is serializable.
This class inherits from `Battle.AI.Condition`.

Represents a condition checking whether one action is currently usable.

Composed of:
- action filters
- optional target filters

### Battle.AI.ActionChoice
This class is serializable.

Represents the action to take if an AI rule passes.

Composed of:
- action type
- preferred action if relevant
- target preference
- movement preference

# Model.Progression

## RunProgress
This class is serializable.

Represents persistent progression and world state for the current run.

Composed of:
- source states by stable source id
- unlocked milestone flags

## Progression.SourceState
This class is serializable.

Represents the saved state of one persistent world source.

This should cover trainers, destroyed world obstacles, switches, traps, puzzle elements, or other resettable exploration objects.

Composed of:
- source id
- source kind
- enabled flag
- cleared flag if relevant
- integer state value if relevant

## Progression.Badge
This class is a scriptable object.

Represents one gym badge that can be awarded to the player.

This should be its own authored asset so gyms can reference it directly and the player state can store the obtained badges.

Composed of:
- display name
- icon or visual asset
- description if needed
- optional progression tags or metadata

## Progression.MilestoneFlag
This class is a scriptable object.

Represents one progression flag that can be unlocked to gate later dialogue, events, or world interactions.

Composed of:
- display name
- description if needed

# Model.Trainer

## Trainer.Definition
This class is a scriptable object.

Represents one authored trainer that can be referenced by world or interior placements.

This is the class that should hold the trainer's identity, the encounter data used for its battle, the dialogues around the fight, and the rewards granted when it is defeated.
Gym bosses should just be stronger trainers that include a badge reward.
Elite Four opponents should also just be normal trainers, while the "defeat all 4 in a row" logic should be tracked by progression state and milestone unlocks rather than by a separate opponent class.

Composed of:
- display name
- encounter definition
- start battle dialogue
- player victory dialogue
- player defeat dialogue
- rewards

## Trainer.Reward
This class is serializable.

Represents one reward granted when a trainer is defeated.

This should be the abstract base of a polymorphic trainer-reward hierarchy, authored with managed-reference serialization and a custom inspector.

Composed of:
- the data shared by all trainer reward subtypes if any

## Trainer.GiveMoneyReward
This class is serializable.
This class inherits from `Trainer.Reward`.

Represents a trainer reward that gives money to the player.

Composed of:
- amount

## Trainer.GiveItemReward
This class is serializable.
This class inherits from `Trainer.Reward`.

Represents a trainer reward that gives one item or a stack of items to the player.

Composed of:
- item definition
- amount

## Trainer.GiveBadgeReward
This class is serializable.
This class inherits from `Trainer.Reward`.

Represents a trainer reward that gives one badge to the player.

Composed of:
- badge

## Trainer.UnlockMilestoneReward
This class is serializable.
This class inherits from `Trainer.Reward`.

Represents a trainer reward that unlocks one milestone flag for later events, dialogue, or progression checks.

Composed of:
- milestone flag

# Model.Save

## SaveData
This class is serializable.

Represents the full data needed to persist and restore a run.

Composed of:
- `Core.GameRun run`
- save timestamp
- save slot id if needed

# View.Animation

## View.Animation.Recipe
This class is a scriptable object.

Represents one fake animation made of sequential phases.

This should be authored against `View.Animation.LogicalPart`, not exact transform names.
It should be reusable across several creatures that share a similar animation intent.

Composed of:
- display name
- phases
- loop flag if relevant

## View.Animation.Set
This class is a scriptable object.

Represents the full set of animation recipes used by one creature form, one archetype, or one model prefab.

This is likely the main asset you will assign to a `Creature.Form`.
It should act as a named collection of recipes that the animation system can query by string name.
That matches a runtime flow like "play animation `AttackMelee` on creature view X" without introducing a dedicated request class in the model.
Every creature should have an animation set, and every set should at minimum contain the standard animations needed by the game flow.
Those mandatory names should be:
- `Idle`
- `TakeDamage`
- `Death`

Action definitions that request caster animations should use names expected to exist inside this set.

Composed of:
- serialized dictionary from animation name to recipe
- idle animation name, to allow the animation to return to a specific animation loop when ending the others

## View.Animation.Phase
This class is serializable.

Represents one sequential phase of a recipe.

All steps inside one phase should run in parallel.
Phases themselves should run one after another.

Composed of:
- duration
- steps

## View.Animation.Step
This class is serializable.

Represents one primitive fake-animation operation.

This should be the abstract base of a polymorphic step hierarchy, authored with managed-reference serialization and a custom inspector.
The custom inspector should let the creator choose the concrete step subtype, then expose only the fields relevant to that subtype.

Examples of concrete step subclasses:
- `View.Animation.MoveLocalStep`
- `View.Animation.RotateLocalStep`
- `View.Animation.ScaleStep`
- `View.Animation.ShakeStep`
- `View.Animation.FlashStep`
- `View.Animation.WaitStep`
- `View.Animation.SpawnVfxStep`
- `View.Animation.PlaySoundStep`

Composed of:
- the data shared by all step subtypes if any

## View.Animation.MoveLocalStep
This class is serializable.

Represents a local position offset applied to one logical part.

Composed of:
- target logical part
- local offset
- easing curve
- additive flag if relevant

## View.Animation.RotateLocalStep
This class is serializable.

Represents a local rotation offset applied to one logical part.

Composed of:
- target logical part
- local rotation offset
- easing curve
- additive flag if relevant

## View.Animation.ScaleStep
This class is serializable.

Represents a local scale offset applied to one logical part.

This is especially useful for blob, squash, or stretch-style fake animation.

Composed of:
- target logical part
- local scale multiplier or offset
- easing curve
- additive flag if relevant

## View.Animation.ShakeStep
This class is serializable.

Represents a short shake or jitter applied to one logical part or to the whole rig.

Composed of:
- target logical part
- amplitude
- frequency
- easing curve

## View.Animation.FlashStep
This class is serializable.

Represents a temporary visual flash on the model.

This is useful for hit reacts, charge-up cues, or ability feedback.

Composed of:
- target scope such as whole rig or specific part
- color or flash style
- intensity
- easing curve

## View.Animation.WaitStep
This class is serializable.

Represents a phase step that only consumes time without modifying transforms.

Composed of:
- optional note or label if relevant

## View.Animation.SpawnVfxStep
This class is serializable.

Represents a step that spawns a VFX on a logical part or world anchor during a recipe.

Composed of:
- target logical part or anchor
- VFX reference
- local offset if relevant

## View.Animation.PlaySoundStep
This class is serializable.

Represents a step that plays a sound during a recipe.

Composed of:
- sound reference
- target logical part or world anchor if relevant

## View.Animation.LogicalPart
Represents the enum-like list of logical targets used by fake animations instead of directly targeting concrete transform names.

This is what makes one recipe reusable across different creatures.
Animations should be authored against these logical parts, not against exact transform paths inside one prefab.

Examples:
- root
- body
- head
- front
- rear
- dominant limb
- off limb
- weapon
- jaw
- tail
- whole rig

## View.Animation.Rig
This class is a MonoBehaviour.

Represents the runtime component attached to the creature model prefab.

This is the object that maps logical animation parts to real transforms.
With a serializable dictionary available in Unity, this can directly store a dictionary from `LogicalPart` to `Transform`.
If a logical part is not present in that dictionary, it simply means that this model does not expose that part for animation, and any step targeting it should be skipped.
It should also capture the default local pose of each bound part so fake animations can return cleanly to rest.
That is what allows the same recipe to work on different body types without a separate capability system.

Composed of:
- serialized dictionary from logical part to transform

## View.Animation.Animator
This class is a MonoBehaviour.

Represents the runtime component that executes animation recipes on an instantiated creature view.

This should read the rig, resolve logical parts, apply steps relative to the captured rest pose, and restore parts cleanly when phases end.
It should stay simple at first:
- one main animation channel
- optional one additive overlay channel for hit flashes, recoil, or other short reactions

Board movement itself should usually stay outside this recipe system.
The creature view can be moved from tile to tile by a separate movement tween, while the animation animator only adds body bob, lunge, recoil, squash, and similar fake-animation offsets.

Composed of:
- animation rig
- animation set
- current main recipe state
- current overlay recipe state if relevant
- runtime pose offsets
