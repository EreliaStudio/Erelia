using UnityEngine;

namespace Erelia.Core.UI
{
	public class CreatureCardGroupElement : MonoBehaviour
	{
		[SerializeField] private CreatureCardElement cardElementPrefab;
		private CreatureCardElement[] cardElements = System.Array.Empty<CreatureCardElement>();

		protected CreatureCardElement[] CardElements => cardElements;

		public virtual void PopulateUnits(System.Collections.Generic.IReadOnlyList<Erelia.Battle.Unit.Presenter> units)
		{
			EnsureCardElements();
			if (cardElements == null)
			{
				return;
			}

			for (int i = 0; i < cardElements.Length; i++)
			{
				if (cardElements[i] == null)
				{
					continue;
				}

				Erelia.Battle.Unit.Presenter unit =
					units != null && i < units.Count ? units[i] : null;
				cardElements[i].LinkUnit(unit);
			}
		}

		public bool ContainsLinkedUnit(Erelia.Battle.Unit.Presenter unit)
		{
			EnsureCardElements();
			if (unit == null || cardElements == null)
			{
				return false;
			}

			for (int i = 0; i < cardElements.Length; i++)
			{
				if (cardElements[i] != null && ReferenceEquals(cardElements[i].LinkedUnit, unit))
				{
					return true;
				}
			}

			return false;
		}

		private void EnsureCardElements()
		{
			if (cardElementPrefab == null)
			{
				CacheDirectChildCardElements();
				return;
			}

			if (!Application.isPlaying)
			{
				return;
			}

			if (HasExpectedCardElements())
			{
				return;
			}

			RebuildCardElements();
		}

		private bool HasExpectedCardElements()
		{
			if (cardElements == null || cardElements.Length != Erelia.Core.Creature.Team.DefaultSize)
			{
				return false;
			}

			for (int i = 0; i < cardElements.Length; i++)
			{
				CreatureCardElement element = cardElements[i];
				if (element == null || element.transform.parent != transform)
				{
					return false;
				}

				if (cardElementPrefab != null &&
					(element.GetType() != cardElementPrefab.GetType() ||
					!element.gameObject.name.StartsWith(cardElementPrefab.gameObject.name, System.StringComparison.Ordinal)))
				{
					return false;
				}
			}

			return true;
		}

		private void CacheDirectChildCardElements()
		{
			var cachedElements = new System.Collections.Generic.List<CreatureCardElement>();
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if (child == null || !child.TryGetComponent(out CreatureCardElement element))
				{
					continue;
				}

				cachedElements.Add(element);
			}

			cardElements = cachedElements.ToArray();
		}

		private void RebuildCardElements()
		{
			ClearDirectChildCardElements();

			cardElements = new CreatureCardElement[Erelia.Core.Creature.Team.DefaultSize];
			for (int i = 0; i < cardElements.Length; i++)
			{
				CreatureCardElement element = InstantiateCardElement(i);
				cardElements[i] = element;
			}
		}

		private void ClearDirectChildCardElements()
		{
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				Transform child = transform.GetChild(i);
				if (child == null || !child.TryGetComponent(out CreatureCardElement element))
				{
					continue;
				}

				if (Application.isPlaying)
				{
					Destroy(child.gameObject);
				}
				else
				{
					DestroyImmediate(child.gameObject);
				}
			}
		}

		private CreatureCardElement InstantiateCardElement(int index)
		{
			GameObject instance = Instantiate(cardElementPrefab.gameObject, transform, false);

			if (instance == null || !instance.TryGetComponent(out CreatureCardElement element))
			{
				Debug.LogWarning("[Erelia.Core.UI.CreatureCardGroupElement] Failed to instantiate creature card prefab.");
				return null;
			}

			instance.name = $"{cardElementPrefab.gameObject.name} ({index + 1})";
			return element;
		}
	}
}
