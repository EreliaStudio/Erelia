using UnityEngine;

namespace Erelia.Battle.Unit
{
	/// <summary>
	/// Battle runtime model for a creature combatant.
	/// </summary>
	[System.Serializable]
	public sealed class Model
	{
		private static readonly Vector3Int UnassignedCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

		[SerializeField] private Erelia.Core.Creature.Instance.Model creature;
		[SerializeField] private Erelia.Battle.Unit.Team team;
		[SerializeField] private int teamIndex;
		[SerializeField] private Erelia.Core.Stats.Values liveStats;
		[SerializeField] private float currentStamina;
		[SerializeField] private Vector3Int cell = UnassignedCell;
		[SerializeField] private Vector3 worldPosition;
		[SerializeField] private bool hasWorldPosition;

		public Erelia.Core.Creature.Instance.Model Creature => creature;
		public Erelia.Battle.Unit.Team Team => team;
		public int TeamIndex => teamIndex;
		public Erelia.Core.Stats.Values LiveStats => liveStats;
		public float CurrentStamina => currentStamina;
		public Vector3Int Cell => cell;
		public Vector3 WorldPosition => worldPosition;
		public bool HasCell => cell != UnassignedCell;
		public bool HasWorldPosition => hasWorldPosition;
		public bool IsAlive => true;
		public bool IsReady => CurrentStamina <= 0f;
		public float StaminaRequirement => Mathf.Max(0.0001f, liveStats.Stamina);
		public float StaminaProgressNormalized => 1f - Mathf.Clamp01(currentStamina / StaminaRequirement);

		public Model(
			Erelia.Core.Creature.Instance.Model creature,
			Erelia.Battle.Unit.Team team,
			int teamIndex,
			Erelia.Core.Stats.Values liveStats)
		{
			this.creature = creature;
			this.team = team;
			this.teamIndex = Mathf.Max(0, teamIndex);
			this.liveStats = liveStats;
			currentStamina = StaminaRequirement;
			cell = UnassignedCell;
			worldPosition = default;
			hasWorldPosition = false;
		}

		public void Stage(Vector3 position)
		{
			cell = UnassignedCell;
			worldPosition = position;
			hasWorldPosition = true;
		}

		public void Place(Vector3Int coordinate, Vector3 position)
		{
			cell = coordinate;
			worldPosition = position;
			hasWorldPosition = true;
		}

		public bool TickStamina(float deltaTime)
		{
			if (!IsAlive || !HasCell || IsReady)
			{
				return IsReady;
			}

			currentStamina = Mathf.Max(0f, currentStamina - Mathf.Max(0f, deltaTime));
			return IsReady;
		}

		public void ResetTurnProgress()
		{
			currentStamina = StaminaRequirement;
		}
	}
}
