using UnityEngine;

namespace Erelia.Player
{
	public sealed class EncounterTriggerEmitter : MonoBehaviour
	{
		[SerializeField] private Erelia.World.Presenter worldPresenter;
		private bool awaitingScreenHided;

		private void OnEnable()
		{
			Erelia.Event.Bus.Subscribe<Erelia.Event.PlayerMotion>(OnPlayerMotion);
			Erelia.Event.Bus.Subscribe<Erelia.Event.ScreenHided>(OnScreenHided);
		}

		private void OnDisable()
		{
			Erelia.Event.Bus.Unsubscribe<Erelia.Event.PlayerMotion>(OnPlayerMotion);
			Erelia.Event.Bus.Unsubscribe<Erelia.Event.ScreenHided>(OnScreenHided);
		}

		private void OnPlayerMotion(Erelia.Event.PlayerMotion evt)
		{
			if (evt == null)
			{
				return;
			}

			if (awaitingScreenHided)
			{
				return;
			}

			Erelia.World.Model worldModel = worldPresenter.WorldModel;

			Vector3 worldPosition = evt.WorldPosition;
			Erelia.World.Chunk.Coordinates chunkCoords = Erelia.World.Chunk.Coordinates.FromWorld(worldPosition);

			if (!worldModel.Chunks.TryGetValue(chunkCoords, out Erelia.World.Chunk.Model chunk))
			{
				return;
			}

			Vector3Int cell = WorldToCell(worldPosition);
			int localX = cell.x - (chunkCoords.X * Erelia.World.Chunk.Model.SizeX);
			int localZ = cell.z - (chunkCoords.Z * Erelia.World.Chunk.Model.SizeZ);
			int localY = cell.y;

			if (localX < 0 || localX >= Erelia.World.Chunk.Model.SizeX ||
				localY < 0 || localY >= Erelia.World.Chunk.Model.SizeY ||
				localZ < 0 || localZ >= Erelia.World.Chunk.Model.SizeZ)
			{
				return;
			}

			int encounterId = chunk.GetEncounterId(localX, localY, localZ);
			if (encounterId == Erelia.World.Chunk.Model.NoEncounterId)
			{
				return;
			}

			if (!Erelia.EncounterTableRegistry.TryGetTable(encounterId, out Erelia.Encounter.EncounterTable table))
			{
				return;
			}

			float encounterChance = Mathf.Clamp01(table.EncounterChance);
			if (encounterChance <= 0f || (encounterChance < 1f && Random.value > encounterChance))
			{
				return;
			}

			Erelia.Battle.Board.Model battleBoard = Erelia.Battle.BattleBoardConstructor.ExportArea(table, worldModel, worldPosition);

			Debug.Log($"Encounter trigger: id={encounterId} world={worldPosition} cell=({localX},{localY},{localZ})");
			Erelia.Event.EncounterTriggerEvent encounterEvent = new Erelia.Event.EncounterTriggerEvent(table, battleBoard);
			Erelia.Encounter.EncounterContext.SetEncounter(encounterEvent);
			Erelia.Event.Bus.Emit(encounterEvent);

			awaitingScreenHided = true;
			Erelia.Event.Bus.Emit(new Erelia.Event.EnterTransitionOn());
		}

		private void OnScreenHided(Erelia.Event.ScreenHided evt)
		{
			if (!awaitingScreenHided)
			{
				return;
			}

			awaitingScreenHided = false;
			Erelia.SceneBootstrapper.LoadScene(Erelia.SceneKind.Battle);
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
