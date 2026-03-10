using UnityEngine;

namespace Erelia.Battle.Unit
{
	public sealed class View : MonoBehaviour
	{
		private GameObject visualInstance;
		private Transform pivot;

		public Transform Pivot => pivot != null ? pivot : transform;

		public void SetVisualPrefab(GameObject prefab)
		{
			DestroyVisualInstance();

			if (prefab == null)
			{
				pivot = transform;
				return;
			}

			visualInstance = Object.Instantiate(prefab, transform);
			visualInstance.name = prefab.name;
			pivot = ResolvePivot(visualInstance);
		}

		public void SetVisible(bool value)
		{
			if (gameObject.activeSelf == value)
			{
				return;
			}

			gameObject.SetActive(value);
		}

		public void SetWorldPosition(Vector3 worldPosition)
		{
			Transform root = transform;
			Transform currentPivot = Pivot;
			Vector3 pivotOffset = currentPivot.position - root.position;
			root.position = worldPosition - pivotOffset;
		}

		public bool TryGetCreaturePresenter(out Erelia.Core.Creature.Instance.Presenter presenter)
		{
			if (visualInstance == null)
			{
				presenter = null;
				return false;
			}

			presenter =
				visualInstance.GetComponent<Erelia.Core.Creature.Instance.Presenter>() ??
				visualInstance.GetComponentInChildren<Erelia.Core.Creature.Instance.Presenter>(true);
			return presenter != null;
		}

		private void DestroyVisualInstance()
		{
			if (visualInstance == null)
			{
				return;
			}

			if (Application.isPlaying)
			{
				Object.Destroy(visualInstance);
			}
			else
			{
				Object.DestroyImmediate(visualInstance);
			}

			visualInstance = null;
			pivot = transform;
		}

		private static Transform ResolvePivot(GameObject viewObject)
		{
			if (viewObject == null)
			{
				return null;
			}

			Erelia.Core.Creature.Instance.View creatureView =
				viewObject.GetComponent<Erelia.Core.Creature.Instance.View>() ??
				viewObject.GetComponentInChildren<Erelia.Core.Creature.Instance.View>(true);
			return creatureView != null ? creatureView.Pivot : viewObject.transform;
		}
	}
}
