# World Generation Ideas

This document proposes a technical direction for generating a **finite** procedural world for Erelia.

The target is:

- one continent-sized world with a predictable overall size
- optional islands
- up to 8 major cities acting as biome anchors
- smaller towns and villages around those major cities
- roads between settlements
- bridges when crossing rivers
- ports plus sea routes when connecting across larger bodies of water
- tunnel routes when crossing mountains or other difficult terrain
- randomized biome order while keeping the world scale relatively stable between runs

## Recommendation

I would use a **two-layer generation model**:

1. A **macro world plan** generated once per run
2. A **voxel realization layer** that stamps chunks from that plan

This is the important design choice.
Do not try to discover the whole world structure directly from chunk-local noise only.
Instead, generate the continent, settlements, biome regions, and transport graph first, then let chunk generation read that higher-level plan.

## Layer 1: Macro World Plan

The macro world plan is the data you generate once and save with the run seed.

It should contain at least:

- world bounds
- landmass mask or continent mask
- height and river seeds
- major city list
- minor settlement list
- biome-region assignment
- road graph
- bridge placements
- port placements
- sea-route links
- tunnel entrance placements
- tunnel links

This layer is what makes the world finite, coherent, and replayable.

## Step 1: Generate The Landmass

Use a fixed world size, for example one square world with a stable width and height.

Recommended method:

- start from a large radial or elliptical falloff so the world naturally becomes an island-like continent
- add low-frequency noise to distort the coastline
- add a second weaker noise pass to create bays, peninsulas, and smaller islands
- threshold the resulting field into land and water

This gives you:

- a predictable global size
- a coastline that changes every run
- room for islands without making the world infinite

If you want more control, keep one main continent guaranteed and let secondary islands appear only in some seeds.

## Step 2: Generate Height And Rivers

After the land/water mask exists:

- generate a height field on land
- smooth broad regions so the map stays traversable
- carve mountains and hills from separate noise layers
- generate rivers by following downhill flow from higher regions to the sea

Rivers are important because they directly affect road generation.
They are also the feature that decides when a road should become a bridge.

## Step 3: Place The 8 Major Cities

The major cities should be placed after the continent shape exists, not before.

Recommended placement constraints:

- city center must be on land
- city center must have enough flat terrain around it
- city centers should keep a minimum distance from each other
- some city centers can be forced near the coast to allow ports

Use a Poisson-disk-like sampling pass or repeated candidate scoring until you get the required number of major cities.

Each major city becomes:

- a biome anchor
- a progression landmark
- a center for nearby villages

## Step 4: Assign Biomes To Major Cities

This is where you randomize the biome order.

Recommended method:

- prepare the list of biome types available for a run
- shuffle them
- assign one biome type to each major city anchor

Then grow biome regions outward from those city anchors using a weighted Voronoi partition over land.

This gives you:

- 8 stable macro regions
- a different biome order each run
- a world that still keeps similar scale and pacing

You can later smooth the borders with noise so they do not look too geometric.

## Step 5: Add Villages And Small Towns

Once each major city owns a biome region, add secondary settlements around it.

Recommended rule:

- every major city gets a target number of satellites
- satellites are chosen on good terrain inside the same biome region
- satellites stay within a travel radius of their major city

This creates a clear regional structure:

- one main city
- several smaller attached locations

That usually feels much better than trying to distribute all settlements uniformly over the continent.

## Step 6: Build A Transport Graph

After all settlements exist, build a graph first and the physical roads second.

Recommended graph strategy:

- connect major cities with a minimal spanning tree so the whole world is traversable
- add a few extra edges for loops and optional routes
- connect each village to its nearest parent city or nearest road hub

Each graph edge should then be classified as one of:

- land road
- river crossing
- sea crossing
- tunnel crossing

This classification is what later decides whether to stamp:

- a normal road
- a bridge
- a port plus sea route
- a tunnel entrance plus tunnel interior plus tunnel exit

## Step 7: Turn Graph Edges Into Paths

For each graph edge, run a pathfinding pass on a terrain cost map.

Suggested terrain costs:

- low cost on flat land
- medium cost on mild slopes
- high cost on steep terrain
- very high cost on mountains
- forbidden or near-forbidden on deep water for normal roads
- special crossing rules for rivers

Then:

- if the path crosses a narrow river, insert a bridge segment
- if the path would cross a mountain barrier or a very expensive rocky area, allow the edge to switch to a tunnel crossing instead of forcing a long detour
- if the path would cross a wide water body or connect to an island, do not force a land road
- instead, split the edge into road -> port -> sea route -> port -> road

For tunnel crossings, I would recommend this rule:

- choose one entrance point on the near side of the mountain
- choose one exit point on the far side
- keep the surface path only up to the entrance and from the exit
- generate the tunnel interior as a separate interior map rather than as literal carved voxels through the whole mountain

That means the effective transport chain becomes:

- road -> tunnel entrance -> tunnel interior -> tunnel exit -> road

This gives you a clean technical rule:

- roads bridge rivers
- roads can become tunnels when mountains would make the surface route bad
- roads do not pretend to cross the sea
- islands are connected by transport hubs instead of ugly artificial causeways

The tunnel interior does not need to match the exact length or width of the surface mountain mass.
It can be a separate authored-or-generated cave-like route whose entrances are tied to the two world-side access points.

## Step 8: Generate Ports And Sea Routes

When an island or coastal region needs external connectivity:

- choose a coastal settlement or coastal road endpoint
- place a port structure there
- connect ports with a sea-route edge

The sea route itself can stay abstract at the macro layer.
It does not need to be a full drivable voxel road.

At gameplay level, it can later become:

- a sailing interAbility
- a ferry
- a fast-travel route
- a world transition point

## Step 9: Generate Tunnel Routes

When one transport edge is classified as a tunnel crossing:

- place an entrance structure near the start-side road endpoint
- place an exit structure near the end-side road endpoint
- create a tunnel link between those two entrances
- generate a separate interior map for the tunnel when needed

The tunnel interior can be:

- a short transit corridor
- a natural cave
- a dungeon-like route with branches
- a themed underground passage tied to the biome or mountain region

This is a strong source of world variety because tunnels can double as:

- traversal shortcuts
- optional exploration spaces
- item or encounter locations
- controlled biome transitions

Technically, this is very similar to your port-and-sea-route idea:

- the overworld only needs the entrance and exit anchors
- the traversed space itself can live in a separate generated map
- the macro graph still treats it as one connectivity edge

## Step 10: Realize The Plan Into Voxels

Only after the macro plan is complete should chunk generation build the voxel world.

Chunk generation should query the macro plan for:

- which biome owns this area
- whether a road passes here
- whether this coordinate belongs to a bridge
- whether a port or settlement occupies this area
- whether a tunnel entrance occupies this area
- whether a river crosses this zone

That means chunk generation becomes deterministic and local, while the world still follows one coherent global plan.
