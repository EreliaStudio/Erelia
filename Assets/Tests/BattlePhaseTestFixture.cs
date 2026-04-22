using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

internal sealed class BattlePhaseTestFixture : IDisposable
{
	public BattleContext BattleContext { get; private set; }
	public CreatureUnit[] PlayerSources { get; private set; }
	public EncounterUnit[] EnemySources { get; private set; }
	public BattleUnit[] PlayerUnits => ToArray(BattleContext?.PlayerUnits);
	public BattleUnit[] EnemyUnits => ToArray(BattleContext?.EnemyUnits);

	private readonly List<UnityEngine.Object> ownedAssets = new List<UnityEngine.Object>();

	public static BattlePhaseTestFixture Create(
		int playerCount = 2,
		int enemyCount = 1,
		float[] playerRecoveries = null,
		float[] enemyRecoveries = null,
		int defaultHealth = 10,
		int defaultActionPoints = 2,
		int defaultMovement = 2)
	{
		BattlePhaseTestFixture fixture = new BattlePhaseTestFixture();
		fixture.Build(
			playerCount,
			enemyCount,
			playerRecoveries,
			enemyRecoveries,
			defaultHealth,
			defaultActionPoints,
			defaultMovement);
		return fixture;
	}

	public BattleOrchestrator CreateInitializedOrchestrator(int randomSeed = 12345)
	{
		UnityEngine.Random.InitState(randomSeed);
		BattleOrchestrator orchestrator = new BattleOrchestrator();
		orchestrator.Initialize(null, BattleContext);
		return orchestrator;
	}

	public TPhase GetPhase<TPhase>(BattleOrchestrator orchestrator, BattlePhaseType phaseType)
		where TPhase : class, IBattlePhase
	{
		Assert.That(orchestrator.TryGetPhase(phaseType, out IBattlePhase phase), Is.True);
		Assert.That(phase, Is.Not.Null);
		Assert.That(phase, Is.TypeOf<TPhase>());
		return phase as TPhase;
	}

	public PlacementPhase GetPlacementPhase(BattleOrchestrator orchestrator)
	{
		return GetPhase<PlacementPhase>(orchestrator, BattlePhaseType.Placement);
	}

	public PlayerTurnPhase GetPlayerTurnPhase(BattleOrchestrator orchestrator)
	{
		return GetPhase<PlayerTurnPhase>(orchestrator, BattlePhaseType.PlayerTurn);
	}

	public void CompletePlacement(
		BattleOrchestrator orchestrator,
		float[] playerTurnBars = null,
		float[] enemyTurnBars = null)
	{
		PlacementPhase placementPhase = GetPlacementPhase(orchestrator);
		PlaceAllPlayers(placementPhase);
		SetTurnBars(playerTurnBars, enemyTurnBars);
		Assert.That(placementPhase.TryCompletePlacement(), Is.True);
	}

	public void PlaceAllPlayers(PlacementPhase placementPhase)
	{
		HashSet<Vector3Int> usedCells = new HashSet<Vector3Int>();
		for (int index = 0; index < PlayerSources.Length; index++)
		{
			IReadOnlyList<Vector3Int> validCells = placementPhase.GetValidPlacementCells(PlayerSources[index]);
			Vector3Int chosen = FindFirstUnused(validCells, usedCells);
			Assert.That(placementPhase.TryPlaceUnit(PlayerSources[index], chosen), Is.True);
			usedCells.Add(chosen);
		}
	}

	public void SetTurnBars(float[] playerTurnBars = null, float[] enemyTurnBars = null)
	{
		ApplyTurnBars(PlayerUnits, playerTurnBars);
		ApplyTurnBars(EnemyUnits, enemyTurnBars);
	}

	public void SetResources(BattleUnit unit, int actionPoints, int movementPoints)
	{
		Assert.That(unit, Is.Not.Null);
		unit.BattleAttributes.ActionPoints.Set(actionPoints, actionPoints, true);
		unit.BattleAttributes.MovementPoints.Set(movementPoints, movementPoints, true);
	}

	public Ability CreateDamageAbility(
		int baseDamage,
		int actionPointCost = 1,
		int movementPointCost = 0,
		int range = 10,
		bool requireLineOfSight = false,
		TargetProfile targetProfile = TargetProfile.Enemy)
	{
		Ability ability = ScriptableObject.CreateInstance<Ability>();
		ability.Cost = new AbilityCost
		{
			Ability = actionPointCost,
			Movement = movementPointCost
		};
		ability.Range = new Ability.RangeDefinition
		{
			Type = Ability.RangeDefinition.Shape.Circle,
			Value = range,
			RequireLineOfSight = requireLineOfSight
		};
		ability.AreaOfEffect = new Ability.AreaOfEffectDefinition
		{
			Type = Ability.AreaOfEffectDefinition.Shape.Circle,
			Value = 0
		};
		ability.TargetProfile = targetProfile;
		ability.Effects = new List<Effect>
		{
			new DamageTargetEffect
			{
				Input = new MathFormula.DamageInput
				{
					BaseDamage = baseDamage,
					DamageKind = MathFormula.DamageInput.Kind.Physical,
					AttackRatio = 0f,
					MagicRatio = 0f
				}
			}
		};

		ownedAssets.Add(ability);
		return ability;
	}

	public bool HasPlacementMask(Vector3Int cell)
	{
		return BattleContext.Board.Terrain.MaskLayer.TryGetMaskCell(cell, out VoxelMaskCell maskCell) &&
			maskCell != null &&
			maskCell.Masks.Contains(VoxelMask.Placement);
	}

	public void Dispose()
	{
		for (int index = 0; index < ownedAssets.Count; index++)
		{
			if (ownedAssets[index] != null)
			{
				UnityEngine.Object.DestroyImmediate(ownedAssets[index]);
			}
		}

		ownedAssets.Clear();
		BattleContext = null;
		PlayerSources = null;
		EnemySources = null;
	}

	private void Build(
		int playerCount,
		int enemyCount,
		float[] playerRecoveries,
		float[] enemyRecoveries,
		int defaultHealth,
		int defaultActionPoints,
		int defaultMovement)
	{
		VoxelRegistry voxelRegistry = CreateWalkableBoardVoxelRegistry();
		BoardData board = CreateBoard(voxelRegistry, 4, 3, 6);

		PlayerSources = BuildCreatureUnits(
			Math.Max(1, playerCount),
			playerRecoveries,
			defaultHealth,
			defaultActionPoints,
			defaultMovement);
		EnemySources = BuildEncounterUnits(
			Math.Max(1, enemyCount),
			enemyRecoveries,
			defaultHealth,
			defaultActionPoints,
			defaultMovement);

		BattleContext = new BattleContext(
			PlayerSources,
			EnemySources,
			board,
			PlacementStyle.HalfBoard,
			Vector3.zero);
	}

	private BoardData CreateBoard(VoxelRegistry voxelRegistry, int sizeX, int sizeY, int sizeZ)
	{
		BoardTerrainLayer terrain = new BoardTerrainLayer(sizeX, sizeY, sizeZ);
		for (int x = 0; x < sizeX; x++)
		{
			for (int z = 0; z < sizeZ; z++)
			{
				terrain.Cells[x, 0, z] = new VoxelCell(1);
			}
		}

		BoardData board = new BoardData(terrain, new BoardNavigationLayer(), new BoardRuntimeRegistry());
		board.AssignVoxelRegistry(voxelRegistry);
		board.RebuildNavigation();
		board.AssignBorderLocalCells(Array.Empty<Vector3Int>());
		return board;
	}

	private CreatureUnit[] BuildCreatureUnits(
		int count,
		float[] recoveries,
		int defaultHealth,
		int defaultActionPoints,
		int defaultMovement)
	{
		CreatureUnit[] units = new CreatureUnit[count];
		for (int index = 0; index < count; index++)
		{
			float recovery = ResolveRecovery(recoveries, index);
			units[index] = new CreatureUnit
			{
				Species = CreateSpecies($"PlayerSpecies_{index}", recovery, defaultHealth, defaultActionPoints, defaultMovement),
				Attributes = CreateAttributes(recovery, defaultHealth, defaultActionPoints, defaultMovement),
				Abilities = new List<Ability>(),
				PermanentPassives = new List<Status>()
			};
		}

		return units;
	}

	private EncounterUnit[] BuildEncounterUnits(
		int count,
		float[] recoveries,
		int defaultHealth,
		int defaultActionPoints,
		int defaultMovement)
	{
		EncounterUnit[] units = new EncounterUnit[count];
		for (int index = 0; index < count; index++)
		{
			float recovery = ResolveRecovery(recoveries, index);
			units[index] = new EncounterUnit
			{
				Species = CreateSpecies($"EnemySpecies_{index}", recovery, defaultHealth, defaultActionPoints, defaultMovement),
				Attributes = CreateAttributes(recovery, defaultHealth, defaultActionPoints, defaultMovement),
				Abilities = new List<Ability>(),
				PermanentPassives = new List<Status>()
			};
		}

		return units;
	}

	private CreatureSpecies CreateSpecies(string name, float recovery, int health, int actionPoints, int movement)
	{
		CreatureSpecies species = ScriptableObject.CreateInstance<CreatureSpecies>();
		species.name = name;
		species.Attributes = CreateAttributes(recovery, health, actionPoints, movement);
		ownedAssets.Add(species);
		return species;
	}

	private static Attributes CreateAttributes(float recovery, int health, int actionPoints, int movement)
	{
		return new Attributes
		{
			Health = health,
			ActionPoints = actionPoints,
			Movement = movement,
			Recovery = recovery
		};
	}

	private VoxelRegistry CreateWalkableBoardVoxelRegistry()
	{
		VoxelRegistry registry = ScriptableObject.CreateInstance<VoxelRegistry>();
		VoxelDefinition solidVoxel = ScriptableObject.CreateInstance<VoxelDefinition>();
		VoxelCubeShape cubeShape = new VoxelCubeShape();
		cubeShape.Initialize();

		SetPrivateField(solidVoxel, "data", new VoxelData { Traversal = VoxelTraversal.Obstacle });
		SetPrivateField(solidVoxel, "shape", cubeShape);
		solidVoxel.Initialize();

		registry.Voxels.Add(1, solidVoxel);
		ownedAssets.Add(registry);
		ownedAssets.Add(solidVoxel);
		return registry;
	}

	private static float ResolveRecovery(float[] recoveries, int index)
	{
		if (recoveries == null || recoveries.Length == 0)
		{
			return 4f;
		}

		return index < recoveries.Length ? recoveries[index] : recoveries[recoveries.Length - 1];
	}

	private static void ApplyTurnBars(BattleUnit[] units, float[] values)
	{
		if (units == null || values == null)
		{
			return;
		}

		for (int index = 0; index < units.Length && index < values.Length; index++)
		{
			BattleUnit unit = units[index];
			if (unit == null)
			{
				continue;
			}

			unit.BattleAttributes.TurnBar.SetCurrent(Mathf.Clamp(values[index], 0f, unit.BattleAttributes.TurnBar.Max), true);
		}
	}

	private static Vector3Int FindFirstUnused(IReadOnlyList<Vector3Int> cells, HashSet<Vector3Int> usedCells)
	{
		for (int index = 0; index < cells.Count; index++)
		{
			if (!usedCells.Contains(cells[index]))
			{
				return cells[index];
			}
		}

		throw new InvalidOperationException("No unused placement cell was found.");
	}

	private static void SetPrivateField(object target, string fieldName, object value)
	{
		FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
		if (field == null)
		{
			throw new InvalidOperationException($"Missing private field '{fieldName}' on type {target.GetType().Name}.");
		}

		field.SetValue(target, value);
	}

	private static BattleUnit[] ToArray(IReadOnlyList<BattleUnit> units)
	{
		if (units == null)
		{
			return Array.Empty<BattleUnit>();
		}

		BattleUnit[] result = new BattleUnit[units.Count];
		for (int index = 0; index < units.Count; index++)
		{
			result[index] = units[index];
		}

		return result;
	}
}
