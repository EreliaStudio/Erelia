using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Phase.Core.UI
{
	public class CreatureCardGroupElement : MonoBehaviour
	{
		[SerializeField] private CreatureCardElement cardPrefab;
		[SerializeField] private int displayedCardCount = Erelia.Core.Creature.Team.DefaultSize;
		[System.NonSerialized] private readonly List<CreatureCardElement> runtimeCardElements =
			new List<CreatureCardElement>();

		public IReadOnlyList<CreatureCardElement> GetCardElements()
		{
			return runtimeCardElements;
		}

		public void SetCardPrefab(CreatureCardElement prefab)
		{
			cardPrefab = prefab;
		}

		public virtual void PopulateUnits(IReadOnlyList<Erelia.Battle.Unit.Presenter> units)
		{
			ClearUnits();

			int boundUnitCount = units != null ? units.Count : 0;
			for (int i = 0; i < boundUnitCount; i++)
			{
				AddUnit(units[i]);
			}

			int totalCardCount = Mathf.Max(boundUnitCount, Mathf.Max(0, displayedCardCount));
			for (int i = boundUnitCount; i < totalCardCount; i++)
			{
				AddUnit(null);
			}
		}

		public virtual void ClearUnits()
		{
			ReleaseRuntimeCards();
			HideUnboundChildCards();
		}

		public virtual CreatureCardElement AddUnit(Erelia.Battle.Unit.Presenter unit)
		{
			return AddDynamicUnit(unit);
		}

		public bool ContainsLinkedUnit(Erelia.Battle.Unit.Presenter unit)
		{
			IReadOnlyList<CreatureCardElement> elements = GetCardElements();
			if (unit == null || elements == null)
			{
				return false;
			}

			for (int i = 0; i < elements.Count; i++)
			{
				if (elements[i] != null && ReferenceEquals(elements[i].LinkedUnit, unit))
				{
					return true;
				}
			}

			return false;
		}

		public bool ContainsLinkedCreature(Erelia.Core.Creature.Instance.Model creature)
		{
			IReadOnlyList<CreatureCardElement> elements = GetCardElements();
			if (creature == null || elements == null)
			{
				return false;
			}

			for (int i = 0; i < elements.Count; i++)
			{
				if (elements[i] != null && ReferenceEquals(elements[i].LinkedCreature, creature))
				{
					return true;
				}
			}

			return false;
		}

		private CreatureCardElement AddDynamicUnit(Erelia.Battle.Unit.Presenter unit)
		{
			if (cardPrefab == null)
			{
				return null;
			}

			CreatureCardElement cardElement = Object.Instantiate(cardPrefab, transform);
			cardElement.transform.SetParent(transform, false);
			cardElement.transform.SetSiblingIndex(runtimeCardElements.Count);
			cardElement.LinkUnit(unit);
			runtimeCardElements.Add(cardElement);
			return cardElement;
		}

		private void ReleaseRuntimeCards()
		{
			for (int i = runtimeCardElements.Count - 1; i >= 0; i--)
			{
				CreatureCardElement cardElement = runtimeCardElements[i];
				if (cardElement == null)
				{
					continue;
				}

				cardElement.LinkUnit(null);

				if (Application.isPlaying)
				{
					Object.Destroy(cardElement.gameObject);
				}
				else
				{
					Object.DestroyImmediate(cardElement.gameObject);
				}
			}

			runtimeCardElements.Clear();
		}

		private void HideUnboundChildCards()
		{
			CreatureCardElement[] childCards = GetComponentsInChildren<CreatureCardElement>(true);
			for (int i = 0; i < childCards.Length; i++)
			{
				CreatureCardElement cardElement = childCards[i];
				if (cardElement == null ||
					cardElement.LinkedUnit != null ||
					runtimeCardElements.Contains(cardElement))
				{
					continue;
				}

				cardElement.gameObject.SetActive(false);
			}
		}
	}
}
