using UnityEngine;

namespace Erelia.Battle
{
	/// <summary>
	/// Battle unit wrapper for a creature, its grid cell, and its spawned view.
	/// Keeps the creature, position, and view grouped for placement and battle logic.
	/// </summary>
	public sealed class Unit
	{
		private static readonly Vector3Int UnplacedCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

		/// <summary>
		/// Creature instance represented by this unit.
		/// </summary>
		public Erelia.Core.Creature.Instance.Model Creature { get; }
		/// <summary>
		/// Current grid cell occupied by the unit.
		/// </summary>
		public Vector3Int Cell { get; private set; }
		/// <summary>
		/// Spawned view GameObject associated with the unit.
		/// </summary>
		public GameObject View { get; }
		/// <summary>
		/// Transform used as the logical placement pivot for the unit view.
		/// </summary>
		public Transform Pivot { get; }
		/// <summary>
		/// Whether the unit is currently placed on the board.
		/// </summary>
		public bool IsPlaced { get; private set; }

		/// <summary>
		/// Creates a unit wrapper from a creature, its starting cell, and its view.
		/// </summary>
		public Unit(Erelia.Core.Creature.Instance.Model creature, Vector3Int cell, GameObject view)
		{
			// Store the unit data.
			Creature = creature;
			Cell = cell;
			View = view;
			Pivot = ResolvePivot(view);
			IsPlaced = true;
		}

		public void Place(Vector3Int cell, Vector3 worldPosition)
		{
			Cell = cell;
			IsPlaced = true;
			SetViewActive(true);
			SetWorldPosition(worldPosition);
		}

		public void Unplace()
		{
			Cell = UnplacedCell;
			IsPlaced = false;
			SetViewActive(false);
		}

		private void SetWorldPosition(Vector3 worldPosition)
		{
			if (View == null)
			{
				return;
			}

			Transform root = View.transform;
			Transform pivot = Pivot != null ? Pivot : root;
			Vector3 pivotOffset = pivot.position - root.position;
			root.position = worldPosition - pivotOffset;
		}

		private void SetViewActive(bool value)
		{
			if (View == null || View.activeSelf == value)
			{
				return;
			}

			View.SetActive(value);
		}

		private static Transform ResolvePivot(GameObject view)
		{
			if (view == null)
			{
				return null;
			}

			Erelia.Core.Creature.Instance.View creatureView =
				view.GetComponent<Erelia.Core.Creature.Instance.View>() ??
				view.GetComponentInChildren<Erelia.Core.Creature.Instance.View>(true);
			if (creatureView != null)
			{
				return creatureView.Pivot;
			}

			return view.transform;
		}
	}
}
