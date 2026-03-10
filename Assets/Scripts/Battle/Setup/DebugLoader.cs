using UnityEngine;

namespace Erelia.Battle
{
	/// <summary>
	/// Optional scene bootstrapper used to start the battle scene directly in debug.
	/// It prepares the runtime context before the normal battle loader binds the board.
	/// </summary>
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

		/// <summary>
		/// Enables debug battle bootstrapping when the battle scene is opened directly.
		/// </summary>
		[SerializeField] private bool debugMode;

		/// <summary>
		/// Size of the generated debug board.
		/// </summary>
		[SerializeField] private Vector3Int debugBoardSize = new Vector3Int(21, 8, 21);

		/// <summary>
		/// Number of solid ground layers generated from the bottom of the board.
		/// </summary>
		[SerializeField] private int debugGroundHeight = 1;

		/// <summary>
		/// Voxel id used for debug ground cells.
		/// </summary>
		[SerializeField] private int debugGroundVoxelId = 0;

		/// <summary>
		/// Voxel id used for debug air cells. Negative values are treated as empty.
		/// </summary>
		[SerializeField] private int debugAirVoxelId = -1;

		/// <summary>
		/// Unity callback invoked on object initialization.
		/// </summary>
		private void Awake()
		{
			InitializeDebugContext();
		}

		/// <summary>
		/// Bootstraps a debug battle context when the scene is launched directly.
		/// </summary>
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

		/// <summary>
		/// Ensures the global player team exists for debug scene launches.
		/// </summary>
		private void EnsurePlayerTeam(Erelia.Core.SystemData systemData)
		{
			if (systemData == null || systemData.PlayerTeam != null)
			{
				return;
			}

			systemData.SetPlayerTeam(CreateDebugPlayerTeam());
		}

		/// <summary>
		/// Returns whether the battle context is already ready for the battle scene.
		/// </summary>
		private static bool HasValidBattleData(Erelia.Battle.Data data)
		{
			return data != null &&
				data.Board != null &&
				data.EnemyTeam != null;
		}

		/// <summary>
		/// Creates the enemy team used by debug mode.
		/// </summary>
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

		/// <summary>
		/// Creates the player team used by debug mode.
		/// </summary>
		private static Erelia.Core.Creature.Team CreateDebugPlayerTeam()
		{
			var team = new Erelia.Core.Creature.Team();
			Erelia.Core.Creature.Instance.Model[] slots = team.Slots;

			if (slots != null && slots.Length > 0)
			{
				slots[0] = new Erelia.Core.Creature.Instance.Model(DebugPlayerSpeciesIdA, DebugPlayerNicknameA);
			}

			if (slots != null && slots.Length > 1)
			{
				slots[1] = new Erelia.Core.Creature.Instance.Model(DebugPlayerSpeciesIdB, DebugPlayerNicknameB);
			}

			return team;
		}

		/// <summary>
		/// Builds a simple deterministic board for debug scene launches.
		/// </summary>
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

			Vector3Int origin = Vector3Int.zero;
			Vector3Int center = new Vector3Int(sizeX / 2, groundHeight - 1, sizeZ / 2);
			return new Erelia.Battle.Board.Model(cells, origin, center);
		}
	}
}
