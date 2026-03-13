using UnityEngine;

namespace Erelia.Exploration.Player
{
	/// <summary>
	/// Emits a battle request when the player steps on an encounter-enabled cell.
	/// Receives player motion events, resolves the cell encounter id, checks the encounter chance,
	/// then builds the battle board and emits a battle scene request.
	/// </summary>
	public sealed class EncounterTriggerEmitter : MonoBehaviour
	{
		/// <summary>
		/// World presenter used to resolve the world model.
		/// </summary>
		[SerializeField] private Erelia.Exploration.World.Presenter worldPresenter;
		
		/// <summary>
		/// Unity callback invoked when the component is enabled.
		/// </summary>
		private void OnEnable()
		{
			// Listen for player motion events.
			Erelia.Core.Event.Bus.Subscribe<Erelia.Core.Event.PlayerMotion>(OnPlayerMotion);
		}

		/// <summary>
		/// Unity callback invoked when the component is disabled.
		/// </summary>
		private void OnDisable()
		{
			// Stop listening for player motion events.
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Core.Event.PlayerMotion>(OnPlayerMotion);
		}

		/// <summary>
		/// Handles player motion to detect encounter triggers.
		/// </summary>
		/// <param name="evt">Player motion event.</param>
		private void OnPlayerMotion(Erelia.Core.Event.PlayerMotion evt)
		{
			// Ignore invalid events.
			if (evt == null)
			{
				return;
			}

			// Ensure world model is available.
			if (worldPresenter == null || worldPresenter.WorldModel == null)
			{
				return;
			}

			Erelia.Exploration.World.Model worldModel = worldPresenter.WorldModel;
			Erelia.Exploration.Player.Model playerModel = Erelia.Core.Context.Instance.ExplorationData?.PlayerModel;

			// Resolve chunk and local cell position.
			Vector3 worldPosition = evt.WorldPosition;
			Vector3Int cell = evt.CellPosition;
			Erelia.Exploration.World.Chunk.Coordinates chunkCoords = Erelia.Exploration.World.Chunk.Coordinates.FromWorld(worldPosition);

			if (playerModel != null)
			{
				if (playerModel.IsEncounterLockedAt(cell))
				{
					return;
				}

				playerModel.ClearEncounterLockCell(cell);
			}

			if (!worldModel.Chunks.TryGetValue(chunkCoords, out Erelia.Exploration.World.Chunk.Model chunk))
			{
				return;
			}

			int localX = cell.x - (chunkCoords.X * Erelia.Exploration.World.Chunk.Model.SizeX);
			int localZ = cell.z - (chunkCoords.Z * Erelia.Exploration.World.Chunk.Model.SizeZ);
			int localY = cell.y;

			if (localX < 0 || localX >= Erelia.Exploration.World.Chunk.Model.SizeX ||
				localY < 0 || localY >= Erelia.Exploration.World.Chunk.Model.SizeY ||
				localZ < 0 || localZ >= Erelia.Exploration.World.Chunk.Model.SizeZ)
			{
				return;
			}

			// Fetch encounter id from the chunk cell.
			int encounterId = chunk.GetEncounterId(localX, localY, localZ);
			if (encounterId == Erelia.Exploration.World.Chunk.Model.NoEncounterId)
			{
				return;
			}

			// Resolve encounter table for this id.
			if (!Erelia.Core.Encounter.EncounterTableRegistry.TryGetTable(encounterId, out Erelia.Core.Encounter.EncounterTable table))
			{
				return;
			}

			// Roll encounter chance.
			float encounterChance = Mathf.Clamp01(table.EncounterChance);
			if (encounterChance <= 0f || (encounterChance < 1f && Random.value > encounterChance))
			{
				return;
			}

			if (!table.TryLoadRandomTeam(out Erelia.Core.Creature.Team enemyTeam))
			{
				return;
			}

			// Build battle board and emit battle events.
			Erelia.Battle.Board.Model battleBoard = Erelia.Battle.Board.Constructor.ExportArea(table, worldModel, worldPosition);
			if (battleBoard == null)
			{
				return;
			}

			playerModel?.SetWorldPosition(worldPosition);
			playerModel?.SetEncounterLockCell(cell);

			Debug.Log($"Encounter trigger: id={encounterId} world={worldPosition} cell=({localX},{localY},{localZ})");
			Erelia.Core.Event.Bus.Emit(new Erelia.Core.Event.EncounterTriggerEvent(enemyTeam, battleBoard));
			Erelia.Core.Event.Bus.Emit(new Erelia.Core.Event.BattleSceneDataRequest(enemyTeam, battleBoard));
		}
	}
}
