using UnityEngine;

namespace Erelia.Battle.Unit
{
	/// <summary>
	/// World-space unit view that represents the creature model on or around the board.
	/// </summary>
	public sealed class ObjectView : Erelia.Battle.Unit.View
	{
		[SerializeField] private Erelia.Core.Creature.Instance.View creatureView;

		public Transform Pivot => creatureView != null ? creatureView.Pivot : transform;

		private void Awake()
		{
			if (creatureView == null)
			{
				creatureView =
					GetComponent<Erelia.Core.Creature.Instance.View>() ??
					GetComponentInChildren<Erelia.Core.Creature.Instance.View>(true);
			}
		}

		public override void Refresh()
		{
			if (Presenter == null)
			{
				SetVisible(false);
				return;
			}

			if (Presenter.TryGetWorldPosition(out Vector3 worldPosition))
			{
				SetWorldPosition(worldPosition);
			}

			SetVisible(Presenter.IsAlive && Presenter.HasWorldPosition);
		}

		public void SetWorldPosition(Vector3 worldPosition)
		{
			Transform root = transform;
			Transform pivot = Pivot != null ? Pivot : root;
			Vector3 pivotOffset = pivot.position - root.position;
			root.position = worldPosition - pivotOffset;
		}

		public void SetVisible(bool value)
		{
			if (gameObject.activeSelf == value)
			{
				return;
			}

			gameObject.SetActive(value);
		}
	}
}
