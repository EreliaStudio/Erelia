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

			if (worldPresenter == null || worldPresenter.WorldModel == null)
			{
				return;
			}

			Erelia.Exploration.World.Model worldModel = worldPresenter.WorldModel;

			Vector3 worldPosition = evt.WorldPosition;
			Erelia.Exploration.World.Chunk.Coordinates chunkCoords = Erelia.Exploration.World.Chunk.Coordinates.FromWorld(worldPosition);

			if (!worldModel.Chunks.TryGetValue(chunkCoords, out Erelia.Exploration.World.Chunk.Model chunk))
			{
				return;
			}

			Vector3Int cell = WorldToCell(worldPosition);
			int localX = cell.x - (chunkCoords.X * Erelia.Exploration.World.Chunk.Model.SizeX);
			int localZ = cell.z - (chunkCoords.Z * Erelia.Exploration.World.Chunk.Model.SizeZ);
			int localY = cell.y;

			if (localX < 0 || localX >= Erelia.Exploration.World.Chunk.Model.SizeX ||
				localY < 0 || localY >= Erelia.Exploration.World.Chunk.Model.SizeY ||
				localZ < 0 || localZ >= Erelia.Exploration.World.Chunk.Model.SizeZ)
			{
				return;
			}

			int encounterId = chunk.GetEncounterId(localX, localY, localZ);
			if (encounterId == Erelia.Exploration.World.Chunk.Model.NoEncounterId)
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

			Erelia.Battle.Board.Model battleBoard = Erelia.Battle.Board.Constructor.ExportArea(table, worldModel, worldPosition);

			Debug.Log($"Encounter trigger: id={encounterId} world={worldPosition} cell=({localX},{localY},{localZ})");
			Erelia.Core.Context.Instance.SetBattle(table, battleBoard);
			var battleRequest = new Erelia.Core.Event.BattleSceneDataRequest();
			Erelia.Core.Event.Bus.Emit(battleRequest);
		}

		private static Vector3Int WorldToCell(Vector3 worldPosition)
		{
			return new Vector3Int(
				Mathf.FloorToInt(worldPosition.x),
				Mathf.FloorToInt(worldPosition.y),
				Mathf.FloorToInt(worldPosition.z));
		}

	}
}
