using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.UI
{
	public sealed class FeatCardGroupView : MonoBehaviour
	{
		[SerializeField] private RectTransform contentRoot;
		[SerializeField] private Erelia.Battle.UI.FeatCardElement featCardPrefab;

		private readonly List<Erelia.Battle.UI.FeatCardElement> spawnedCards =
			new List<Erelia.Battle.UI.FeatCardElement>();

		public void Show(IReadOnlyList<Erelia.Battle.BattleResultEntry> entries)
		{
			Clear();
			if (contentRoot == null || featCardPrefab == null || entries == null)
			{
				return;
			}

			RemoveUnmanagedChildren();

			for (int i = 0; i < entries.Count; i++)
			{
				Erelia.Battle.UI.FeatCardElement card = Instantiate(featCardPrefab, contentRoot, false);
				card.gameObject.name = $"FeatCardElement ({i + 1})";
				card.gameObject.SetActive(true);
				NormalizeCardRect(card.transform as RectTransform);
				card.Apply(entries[i]);
				spawnedCards.Add(card);
			}
		}

		public void Clear()
		{
			for (int i = 0; i < spawnedCards.Count; i++)
			{
				Erelia.Battle.UI.FeatCardElement card = spawnedCards[i];
				if (card == null)
				{
					continue;
				}

				if (Application.isPlaying)
				{
					Destroy(card.gameObject);
				}
				else
				{
					DestroyImmediate(card.gameObject);
				}
			}

			spawnedCards.Clear();
		}

		private void RemoveUnmanagedChildren()
		{
			if (contentRoot == null)
			{
				return;
			}

			for (int i = contentRoot.childCount - 1; i >= 0; i--)
			{
				Transform child = contentRoot.GetChild(i);
				if (child == null || child.GetComponent<Erelia.Battle.UI.FeatCardElement>() != null)
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

		private static void NormalizeCardRect(RectTransform rectTransform)
		{
			if (rectTransform == null)
			{
				return;
			}

			rectTransform.anchorMin = new Vector2(0f, 1f);
			rectTransform.anchorMax = new Vector2(1f, 1f);
			rectTransform.pivot = new Vector2(0.5f, 1f);
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.localScale = Vector3.one;
		}
	}
}

