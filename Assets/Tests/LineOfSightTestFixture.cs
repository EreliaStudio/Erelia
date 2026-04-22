using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Fixture for line-of-sight tests.
// Floor at y=0: Obstacle voxels (ID=1) — create navigation nodes.
// Standing level y=1: Walkable voxels (ID=2) by default — transparent to LoS.
// WithWall(x, z) places an Obstacle voxel (ID=1) at y=1, blocking LoS.
// After Build(), nav nodes are manually injected at y=1 for every non-wall cell
// so that TryPlaceUnit succeeds at standing level.
internal sealed class LineOfSightTestFixture : IDisposable
{
	public const int FloorY = 0;
	public const int StandY = 1;

	private const int VoxelObstacle = 1;
	private const int VoxelWalkable = 2;

	public BattleContext BattleContext { get; private set; }
	public BattleUnit PlayerUnit { get; private set; }
	public BattleUnit EnemyUnit { get; private set; }

	private readonly int _sizeX;
	private readonly int _sizeZ;
	private readonly BoardTerrainLayer _terrain;
	private readonly VoxelRegistry _voxelRegistry;
	private readonly List<UnityEngine.Object> _ownedAssets = new();
	private readonly HashSet<(int x, int z)> _wallCells = new();
	private BoardData _board;

	private LineOfSightTestFixture(int sizeX, int sizeZ)
	{
		_sizeX = sizeX;
		_sizeZ = sizeZ;

		_voxelRegistry = BuildVoxelRegistry();
		_terrain = new BoardTerrainLayer(sizeX, 4, sizeZ);
		_terrain.AssignVoxelRegistry(_voxelRegistry);

		for (int x = 0; x < sizeX; x++)
		{
			for (int z = 0; z < sizeZ; z++)
			{
				// Solid floor so the nav builder creates nodes at y=0
				_terrain.Cells[x, FloorY, z] = new VoxelCell(VoxelObstacle);
				// Walkable space at standing level — transparent to LoS
				_terrain.Cells[x, StandY, z] = new VoxelCell(VoxelWalkable);
			}
		}
	}

	public static LineOfSightTestFixture Create(int sizeX = 12, int sizeZ = 12)
	{
		return new LineOfSightTestFixture(sizeX, sizeZ);
	}

	// Place an Obstacle wall at standing level. Call before Build().
	public LineOfSightTestFixture WithWall(int x, int z)
	{
		_terrain.Cells[x, StandY, z] = new VoxelCell(VoxelObstacle);
		_wallCells.Add((x, z));
		return this;
	}

	// Finalise the board and create the BattleContext with one player and one enemy unit.
	public LineOfSightTestFixture Build()
	{
		BoardNavigationLayer navLayer = new BoardNavigationLayer();
		_board = new BoardData(_terrain, navLayer, new BoardRuntimeRegistry());
		_board.AssignVoxelRegistry(_voxelRegistry);
		_board.RebuildNavigation();
		_board.AssignBorderLocalCells(Array.Empty<Vector3Int>());

		// Inject nav nodes at StandY for every non-wall cell.
		// The nav builder only creates nodes where IsSolid(y) is true (Obstacle),
		// which means auto-built nodes land at y=0, not y=1. We add y=1 nodes
		// manually so TryPlaceUnit can register units at standing level.
		VoxelTraversalGraph graph = navLayer.Graph;
		if (graph != null)
		{
			for (int x = 0; x < _sizeX; x++)
			{
				for (int z = 0; z < _sizeZ; z++)
				{
					if (!_wallCells.Contains((x, z)))
					{
						graph.CreateNode(new Vector3Int(x, StandY, z));
					}
				}
			}
		}

		CreatureUnit playerSource = BuildCreatureUnit("LoS_Player");
		EncounterUnit enemySource = BuildEncounterUnit("LoS_Enemy");

		BattleContext = new BattleContext(
			new[] { playerSource },
			new[] { enemySource },
			_board,
			PlacementStyle.HalfBoard,
			Vector3.zero);

		PlayerUnit = BattleContext.PlayerUnits[0];
		EnemyUnit = BattleContext.EnemyUnits[0];
		return this;
	}

	// Place a unit at (x, StandY, z). Returns true if placement succeeded.
	public bool PlaceUnit(BattleUnit unit, int x, int z)
	{
		return PlaceUnitAt(unit, new Vector3Int(x, StandY, z));
	}

	public bool PlaceUnitAt(BattleUnit unit, Vector3Int cell)
	{
		return BattleContext.TryPlaceUnit(unit, cell);
	}

	// Primary LoS query at standing level.
	public bool HasLoS(int fromX, int fromZ, int toX, int toZ)
	{
		return BattleLineOfSightRules.HasLineOfSight(
			BattleContext,
			new Vector3Int(fromX, StandY, fromZ),
			new Vector3Int(toX, StandY, toZ));
	}

	public bool HasLoS(Vector3Int from, Vector3Int to)
	{
		return BattleLineOfSightRules.HasLineOfSight(BattleContext, from, to);
	}

	// Convenience: create an ability and evaluate cast legality between player and enemy.
	public AbilityCastLegality GetAbilityCastLegality(bool requireLoS, int range = 20)
	{
		Ability ability = ScriptableObject.CreateInstance<Ability>();
		ability.Cost = new AbilityCost { Ability = 0, Movement = 0 };
		ability.Range = new Ability.RangeDefinition
		{
			Type = Ability.RangeDefinition.Shape.Circle,
			Value = range,
			RequireLineOfSight = requireLoS
		};
		ability.AreaOfEffect = new Ability.AreaOfEffectDefinition
		{
			Type = Ability.AreaOfEffectDefinition.Shape.Circle,
			Value = 0
		};
		ability.TargetProfile = TargetProfile.Enemy;
		ability.Effects = new List<Effect>();
		_ownedAssets.Add(ability);

		TurnContext turn = new TurnContext();
		turn.Begin(PlayerUnit);
		PlayerUnit.BattleAttributes.ActionPoints.Set(10, 10, true);

		return BattleActionValidator.GetCastLegality(BattleContext, turn, ability, EnemyUnit.BoardPosition);
	}

	public void Dispose()
	{
		foreach (UnityEngine.Object asset in _ownedAssets)
		{
			if (asset != null)
				UnityEngine.Object.DestroyImmediate(asset);
		}

		_ownedAssets.Clear();
		BattleContext = null;
		PlayerUnit = null;
		EnemyUnit = null;
	}

	private VoxelRegistry BuildVoxelRegistry()
	{
		VoxelRegistry registry = ScriptableObject.CreateInstance<VoxelRegistry>();

		VoxelDefinition obstacleVoxel = ScriptableObject.CreateInstance<VoxelDefinition>();
		VoxelCubeShape cubeShape = new VoxelCubeShape();
		cubeShape.Initialize();
		SetPrivateField(obstacleVoxel, "data", new VoxelData { Traversal = VoxelTraversal.Obstacle });
		SetPrivateField(obstacleVoxel, "shape", cubeShape);
		obstacleVoxel.Initialize();

		VoxelDefinition walkableVoxel = ScriptableObject.CreateInstance<VoxelDefinition>();
		VoxelCubeShape walkableShape = new VoxelCubeShape();
		walkableShape.Initialize();
		SetPrivateField(walkableVoxel, "data", new VoxelData { Traversal = VoxelTraversal.Walkable });
		SetPrivateField(walkableVoxel, "shape", walkableShape);
		walkableVoxel.Initialize();

		registry.Voxels.Add(VoxelObstacle, obstacleVoxel);
		registry.Voxels.Add(VoxelWalkable, walkableVoxel);

		_ownedAssets.Add(registry);
		_ownedAssets.Add(obstacleVoxel);
		_ownedAssets.Add(walkableVoxel);
		return registry;
	}

	private CreatureUnit BuildCreatureUnit(string name)
	{
		CreatureSpecies species = ScriptableObject.CreateInstance<CreatureSpecies>();
		species.name = name;
		species.Attributes = new Attributes { Health = 10, ActionPoints = 10, Movement = 0, Recovery = 4f };
		_ownedAssets.Add(species);

		return new CreatureUnit
		{
			Species = species,
			Attributes = new Attributes { Health = 10, ActionPoints = 10, Movement = 0, Recovery = 4f },
			Abilities = new List<Ability>(),
			PermanentPassives = new List<Status>()
		};
	}

	private EncounterUnit BuildEncounterUnit(string name)
	{
		CreatureSpecies species = ScriptableObject.CreateInstance<CreatureSpecies>();
		species.name = name;
		species.Attributes = new Attributes { Health = 10, ActionPoints = 10, Movement = 0, Recovery = 4f };
		_ownedAssets.Add(species);

		return new EncounterUnit
		{
			Species = species,
			Attributes = new Attributes { Health = 10, ActionPoints = 10, Movement = 0, Recovery = 4f },
			Abilities = new List<Ability>(),
			PermanentPassives = new List<Status>()
		};
	}

	private static void SetPrivateField(object target, string fieldName, object value)
	{
		FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
		if (field == null)
			throw new InvalidOperationException($"Missing private field '{fieldName}' on {target.GetType().Name}.");

		field.SetValue(target, value);
	}
}
