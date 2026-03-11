using UnityEngine;

namespace Erelia.Battle.Unit
{
	public readonly struct Snapshot
	{
		public Snapshot(
			Sprite icon,
			string displayName,
			bool isPlaced,
			Erelia.Battle.Side side,
			Vector3Int cell,
			float baseStaminaSeconds,
			float currentStaminaSeconds,
			float staminaProgress01,
			bool isTurnActive)
		{
			Icon = icon;
			DisplayName = displayName;
			IsPlaced = isPlaced;
			Side = side;
			Cell = cell;
			BaseStaminaSeconds = baseStaminaSeconds;
			CurrentStaminaSeconds = currentStaminaSeconds;
			StaminaProgress01 = staminaProgress01;
			IsTurnActive = isTurnActive;
		}

		public Sprite Icon { get; }
		public string DisplayName { get; }
		public bool IsPlaced { get; }
		public Erelia.Battle.Side Side { get; }
		public Vector3Int Cell { get; }
		public float BaseStaminaSeconds { get; }
		public float CurrentStaminaSeconds { get; }
		public float StaminaProgress01 { get; }
		public bool IsTurnActive { get; }
	}
}
