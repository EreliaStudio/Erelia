using UnityEngine;

namespace Erelia.Battle
{
	[DefaultExecutionOrder(-100)]
	public sealed class DebugLoader : MonoBehaviour
	{
		private const int DebugEnemyTeamWeight = 50;
		private const string DebugEnemyTeamPathA = "Encounter/Teams/WildEncounterA";
		private const string DebugEnemyTeamPathB = "Encounter/Teams/WildEncounterB";
		private const int DebugPlayerSpeciesIdA = 1;
		private const string DebugPlayerNicknameA = "UnitACustomName";
		private const int DebugPlayerSpeciesIdB = 2;
		private const string DebugPlayerNicknameB = "UnitBCustomName";
		private const int DebugPlayerAttackIdA = 0;
		private const int DebugPlayerAttackIdB = 1;
		private const int DebugSlabVoxelId = 1;
		private const int DebugSlopeVoxelId = 2;
		private const int DebugStairVoxelId = 3;

		[SerializeField] private bool debugMode;

		[SerializeField] private Vector3Int debugBoardSize = new Vector3Int(21, 8, 21);

		[SerializeField] private int debugGroundHeight = 1;

		[SerializeField] private int debugGroundVoxelId = 0;

		[SerializeField] private int debugAirVoxelId = -1;

		[SerializeField] private int debugTargetPlayerMovementPoints = 3;

		private void Awake()
		{
			InitializeDebugContext();
		}

		private void InitializeDebugContext()
		{
			if (!debugMode)
			{
				return;
			}

			Erelia.Core.Context context = Erelia.Core.Context.Instance;
			EnsurePlayerTeam(context.SystemData);

			if (HasValidBattleData(context.BattleData))
			{
				return;
			}

			Erelia.Core.Creature.Team enemyTeam = CreateDebugEnemyTeam();
			if (enemyTeam == null)
			{
				Debug.LogWarning("[Erelia.Battle.DebugLoader] Failed to create debug enemy team.");
				return;
			}

			Erelia.Battle.Board.Model board = CreateDebugBoard();
			context.SetBattle(enemyTeam, board);
			Debug.Log("[Erelia.Battle.DebugLoader] Debug battle context initialized.");
		}

		private void EnsurePlayerTeam(Erelia.Core.SystemData systemData)
		{
			if (systemData == null || systemData.PlayerTeam != null)
			{
				return;
			}

			systemData.SetPlayerTeam(CreateDebugPlayerTeam());
		}

		private static bool HasValidBattleData(Erelia.Battle.Data data)
		{
			return data != null &&
				data.Board != null &&
				data.EnemyTeam != null;
		}

		private static Erelia.Core.Creature.Team CreateDebugEnemyTeam()
		{
			var encounterTable = new Erelia.Core.Encounter.EncounterTable
			{
				Teams = new[]
				{
					new Erelia.Core.Encounter.EncounterTable.TeamEntry
					{
						TeamPath = DebugEnemyTeamPathA,
						Weight = DebugEnemyTeamWeight
					},
					new Erelia.Core.Encounter.EncounterTable.TeamEntry
					{
						TeamPath = DebugEnemyTeamPathB,
						Weight = DebugEnemyTeamWeight
					}
				}
			};

			encounterTable.TryLoadRandomTeam(out Erelia.Core.Creature.Team team);
			return team;
		}

		private Erelia.Core.Creature.Team CreateDebugPlayerTeam()
		{
			var team = new Erelia.Core.Creature.Team();
			Erelia.Core.Creature.Instance.Model[] slots = team.Slots;
			Erelia.Battle.Attack.Definition[] defaultAttacks = CreateDebugPlayerAttacks();

			slots[0] = new Erelia.Core.Creature.Instance.Model(
				DebugPlayerSpeciesIdA,
				DebugPlayerNicknameA,
				CreateDebugPlayerBonusStats(DebugPlayerSpeciesIdA),
				defaultAttacks);
			slots[1] = new Erelia.Core.Creature.Instance.Model(
				DebugPlayerSpeciesIdB,
				DebugPlayerNicknameB,
				CreateDebugPlayerBonusStats(DebugPlayerSpeciesIdB),
				defaultAttacks);

			return team;
		}

		private Erelia.Battle.Board.Model CreateDebugBoard()
		{
			int sizeX = Mathf.Max(1, debugBoardSize.x);
			int sizeY = Mathf.Max(2, debugBoardSize.y);
			int sizeZ = Mathf.Max(1, debugBoardSize.z);
			int groundHeight = Mathf.Clamp(debugGroundHeight, 1, sizeY - 1);

			Erelia.Battle.Voxel.Cell[,,] cells = Erelia.Battle.Voxel.Cell.CreatePack(
				sizeX,
				sizeY,
				sizeZ,
				new Erelia.Battle.Voxel.Cell(debugAirVoxelId));

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < groundHeight; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						cells[x, y, z] = new Erelia.Battle.Voxel.Cell(debugGroundVoxelId);
					}
				}
			}

			BuildDebugMovementTestArena(cells, sizeX, sizeY, sizeZ, groundHeight);

			Vector3Int origin = Vector3Int.zero;
			Vector3Int center = new Vector3Int(sizeX / 2, groundHeight - 1, sizeZ / 2);
			return new Erelia.Battle.Board.Model(cells, origin, center);
		}

		private Erelia.Core.Creature.Stats CreateDebugPlayerBonusStats(int speciesId)
		{
			int baseMovementPoints = 0;
			Erelia.Core.Creature.SpeciesRegistry registry = Erelia.Core.Creature.SpeciesRegistry.Instance;
			if (registry != null &&
				registry.TryGet(speciesId, out Erelia.Core.Creature.Species species) &&
				species != null)
			{
				baseMovementPoints = species.Stats.MovementPoints;
			}

			int movementBonus = Mathf.Max(0, debugTargetPlayerMovementPoints - baseMovementPoints);
			return new Erelia.Core.Creature.Stats(0, 0f, movementBonus);
		}

		private static Erelia.Battle.Attack.Definition[] CreateDebugPlayerAttacks()
		{
			var attacks = new Erelia.Battle.Attack.Definition[Erelia.Core.Creature.Instance.Model.MaxAttackCount];
			Erelia.Battle.Attack.AttackRegistry registry = Erelia.Battle.Attack.AttackRegistry.Instance;
			if (registry == null)
			{
				return attacks;
			}

			TryAssignAttack(registry, DebugPlayerAttackIdA, attacks, 0);
			TryAssignAttack(registry, DebugPlayerAttackIdB, attacks, 1);
			return attacks;
		}

		private static void TryAssignAttack(
			Erelia.Battle.Attack.AttackRegistry registry,
			int attackId,
			Erelia.Battle.Attack.Definition[] attacks,
			int slotIndex)
		{
			if (registry == null ||
				attacks == null ||
				slotIndex < 0 ||
				slotIndex >= attacks.Length)
			{
				return;
			}

			if (registry.TryGet(attackId, out Erelia.Battle.Attack.Definition attack))
			{
				attacks[slotIndex] = attack;
			}
		}

		private void BuildDebugMovementTestArena(
			Erelia.Battle.Voxel.Cell[,,] cells,
			int sizeX,
			int sizeY,
			int sizeZ,
			int groundHeight)
		{
			int platformY = groundHeight;
			int upperPlatformY = groundHeight + 1;
			int wallTopY = groundHeight + 2;
			int centerX = sizeX / 2;
			int centerZ = sizeZ / 2;

			FillRect(cells, centerX - 2, platformY, centerZ - 2, centerX + 2, centerZ + 2, debugGroundVoxelId);
			TrySetCell(cells, centerX, platformY, centerZ - 3, DebugStairVoxelId, Erelia.Core.VoxelKit.Orientation.PositiveX);
			TrySetCell(cells, centerX, platformY, centerZ + 3, DebugStairVoxelId, Erelia.Core.VoxelKit.Orientation.NegativeX);
			TrySetCell(cells, centerX - 3, platformY, centerZ, DebugStairVoxelId, Erelia.Core.VoxelKit.Orientation.PositiveZ);
			TrySetCell(cells, centerX + 3, platformY, centerZ, DebugStairVoxelId, Erelia.Core.VoxelKit.Orientation.NegativeZ);

			FillRect(cells, centerX - 1, upperPlatformY, centerZ - 1, centerX + 1, centerZ + 1, debugGroundVoxelId);
			TrySetCell(cells, centerX, upperPlatformY, centerZ - 2, DebugStairVoxelId, Erelia.Core.VoxelKit.Orientation.PositiveX);
			TrySetCell(cells, centerX, upperPlatformY, centerZ + 2, DebugStairVoxelId, Erelia.Core.VoxelKit.Orientation.NegativeX);
			TrySetCell(cells, centerX - 2, upperPlatformY, centerZ, DebugStairVoxelId, Erelia.Core.VoxelKit.Orientation.PositiveZ);
			TrySetCell(cells, centerX + 2, upperPlatformY, centerZ, DebugStairVoxelId, Erelia.Core.VoxelKit.Orientation.NegativeZ);

			FillRect(cells, centerX - 8, platformY, centerZ - 7, centerX - 6, centerZ - 7, DebugSlabVoxelId);
			FillRect(cells, centerX - 5, platformY, centerZ - 7, centerX - 3, centerZ - 7, debugGroundVoxelId);

			TrySetCell(cells, centerX + 5, platformY, centerZ - 7, DebugSlopeVoxelId, Erelia.Core.VoxelKit.Orientation.PositiveX);
			TrySetCell(cells, centerX + 6, platformY, centerZ - 6, DebugSlopeVoxelId, Erelia.Core.VoxelKit.Orientation.PositiveZ);
			FillRect(cells, centerX + 7, platformY, centerZ - 6, centerX + 8, centerZ - 5, debugGroundVoxelId);

			for (int y = platformY; y <= wallTopY; y++)
			{
				FillRect(cells, centerX + 6, y, centerZ - 2, centerX + 6, centerZ + 2, debugGroundVoxelId);
			}
		}

		private static void FillRect(
			Erelia.Battle.Voxel.Cell[,,] cells,
			int minX,
			int y,
			int minZ,
			int maxX,
			int maxZ,
			int voxelId,
			Erelia.Core.VoxelKit.Orientation orientation = Erelia.Core.VoxelKit.Orientation.PositiveX,
			Erelia.Core.VoxelKit.FlipOrientation flipOrientation = Erelia.Core.VoxelKit.FlipOrientation.PositiveY)
		{
			if (cells == null)
			{
				return;
			}

			for (int x = Mathf.Min(minX, maxX); x <= Mathf.Max(minX, maxX); x++)
			{
				for (int z = Mathf.Min(minZ, maxZ); z <= Mathf.Max(minZ, maxZ); z++)
				{
					TrySetCell(cells, x, y, z, voxelId, orientation, flipOrientation);
				}
			}
		}

		private static void TrySetCell(
			Erelia.Battle.Voxel.Cell[,,] cells,
			int x,
			int y,
			int z,
			int voxelId,
			Erelia.Core.VoxelKit.Orientation orientation = Erelia.Core.VoxelKit.Orientation.PositiveX,
			Erelia.Core.VoxelKit.FlipOrientation flipOrientation = Erelia.Core.VoxelKit.FlipOrientation.PositiveY)
		{
			if (cells == null ||
				x < 0 || x >= cells.GetLength(0) ||
				y < 0 || y >= cells.GetLength(1) ||
				z < 0 || z >= cells.GetLength(2))
			{
				return;
			}

			cells[x, y, z] = new Erelia.Battle.Voxel.Cell(voxelId, orientation, flipOrientation);
		}
	}
}
