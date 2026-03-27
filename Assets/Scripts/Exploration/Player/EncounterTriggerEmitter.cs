using UnityEngine;

namespace Erelia.Exploration.Player
{
	public sealed class EncounterTriggerEmitter : MonoBehaviour
	{
		[SerializeField] private Erelia.Exploration.World.Presenter worldPresenter;
		
		private void OnEnable()
		{
			Erelia.Core.Event.Bus.Subscribe<Erelia.Core.Event.PlayerMotion>(OnPlayerMotion);
		}

		private void OnDisable()
		{
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Core.Event.PlayerMotion>(OnPlayerMotion);
		}

		private void OnPlayerMotion(Erelia.Core.Event.PlayerMotion evt)
		{
			if (evt == null)
			{
				return;
			}

			if (worldPresenter == null || worldPresenter.World == null)
			{
				return;
			}

			Erelia.Exploration.World.WorldState world = worldPresenter.World;
			Erelia.Exploration.Player.ExplorationPlayerState player = Erelia.Core.GameContext.Instance.Exploration?.Player;

			Vector3 worldPosition = evt.WorldPosition;
			Vector3Int cell = evt.CellPosition;
			Erelia.Exploration.World.Chunk.Coordinates chunkCoords = Erelia.Exploration.World.Chunk.Coordinates.FromWorld(worldPosition);

			if (player != null)
			{
				if (player.IsEncounterLockedAt(cell))
				{
					return;
				}

				player.ClearEncounterLockCell(cell);
			}

			if (!world.Chunks.TryGetValue(chunkCoords, out Erelia.Exploration.World.Chunk.ChunkData chunk))
			{
				return;
			}

			int localX = cell.x - (chunkCoords.X * Erelia.Exploration.World.Chunk.ChunkData.SizeX);
			int localZ = cell.z - (chunkCoords.Z * Erelia.Exploration.World.Chunk.ChunkData.SizeZ);
			int localY = cell.y;

			if (localX < 0 || localX >= Erelia.Exploration.World.Chunk.ChunkData.SizeX ||
				localY < 0 || localY >= Erelia.Exploration.World.Chunk.ChunkData.SizeY ||
				localZ < 0 || localZ >= Erelia.Exploration.World.Chunk.ChunkData.SizeZ)
			{
				return;
			}

			int encounterId = chunk.GetEncounterId(localX, localY, localZ);
			if (encounterId == Erelia.Exploration.World.Chunk.ChunkData.NoEncounterId)
			{
				return;
			}

			if (!Erelia.Core.Encounter.EncounterTableRegistry.TryGetTable(encounterId, out Erelia.Core.Encounter.EncounterTable table))
			{
				return;
			}

			float encounterChance = Mathf.Clamp01(table.EncounterChance);
			if (encounterChance <= 0f || (encounterChance < 1f && Random.value > encounterChance))
			{
				return;
			}

			if (!table.TryLoadRandomTeam(out Erelia.Core.Creature.Team enemyTeam))
			{
				return;
			}

			Erelia.Battle.Board.BattleBoardState battleBoard = Erelia.Battle.Board.BattleBoardFactory.ExportArea(table, world, worldPosition);
			if (battleBoard == null)
			{
				return;
			}

			player?.SetWorldPosition(worldPosition);
			player?.SetEncounterLockCell(cell);

			Debug.Log($"Encounter trigger: id={encounterId} world={worldPosition} cell=({localX},{localY},{localZ})");
			Erelia.Core.Event.Bus.Emit(new Erelia.Core.Event.EncounterTriggerEvent(enemyTeam, battleBoard));
			Erelia.Core.Event.Bus.Emit(new Erelia.Core.Event.BattleSceneDataRequest(enemyTeam, battleBoard));
		}
	}
}



