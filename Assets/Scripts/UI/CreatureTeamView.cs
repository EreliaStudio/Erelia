using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
[RequireComponent(typeof(VerticalLayoutGroup))]
[ExecuteAlways]
public sealed class CreatureTeamView : MonoBehaviour
{
	[SerializeField] private CreatureCardView cardPrefab;

	private readonly List<CreatureCardView> cardInstances = new();
	private IReadOnlyList<BattleUnit> boundTeam;
	private Object lastPrefabReference;
#if UNITY_EDITOR
	private bool editorRefreshQueued;
#endif

	private void Reset()
	{
		EnsureLayout();
		EnsureCards();
	}

	private void Awake()
	{
		EnsureLayout();
		EnsureCards();
	}

	private void OnEnable()
	{
		EnsureLayout();
		EnsureCards();
		RefreshBindings();
	}

	private void OnValidate()
	{
#if UNITY_EDITOR
		QueueEditorRefresh();
#else
		EnsureLayout();
		EnsureCards();
		RefreshBindings();
#endif
	}

	public void Bind(IReadOnlyList<BattleUnit> team)
	{
		boundTeam = team;
		EnsureCards();
		RefreshBindings();
	}

	public int GetCardCount()
	{
		EnsureCards();
		return cardInstances.Count;
	}

	public CreatureCardView GetCard(int index)
	{
		EnsureCards();

		if (index < 0 || index >= cardInstances.Count)
		{
			return null;
		}

		return cardInstances[index];
	}

	private void EnsureCards()
	{
		CollectCards();

		if (!NeedsRebuild())
		{
			return;
		}

		RebuildCards();
	}

	private void CollectCards()
	{
		cardInstances.Clear();

		for (int index = 0; index < transform.childCount; index++)
		{
			Transform child = transform.GetChild(index);
			if (child != null && child.TryGetComponent(out CreatureCardView card))
			{
				cardInstances.Add(card);
			}
		}
	}

	private bool NeedsRebuild()
	{
		if (cardPrefab == null)
		{
			return transform.childCount > 0;
		}

		if (!ReferenceEquals(lastPrefabReference, cardPrefab))
		{
			return true;
		}

		if (cardInstances.Count != GameRule.TeamMemberCount)
		{
			return true;
		}

		if (transform.childCount != cardInstances.Count)
		{
			return true;
		}

		for (int index = 0; index < cardInstances.Count; index++)
		{
			if (cardInstances[index] == null)
			{
				return true;
			}
		}

		return false;
	}

	private void RebuildCards()
	{
		DestroyAllChildren();
		cardInstances.Clear();

		if (cardPrefab == null)
		{
			lastPrefabReference = null;
			return;
		}

		for (int index = 0; index < GameRule.TeamMemberCount; index++)
		{
			CreatureCardView instance = CreateCardInstance(index);
			if (instance == null)
			{
				continue;
			}

			cardInstances.Add(instance);
		}

		lastPrefabReference = cardPrefab;
		RefreshBindings();
	}

	private void EnsureLayout()
	{
		if (!TryGetComponent(out VerticalLayoutGroup layoutGroup))
		{
			return;
		}

		layoutGroup.childAlignment = TextAnchor.UpperCenter;
		layoutGroup.spacing = 8f;
		layoutGroup.childControlWidth = true;
		layoutGroup.childControlHeight = false;
		layoutGroup.childScaleWidth = false;
		layoutGroup.childScaleHeight = false;
		layoutGroup.childForceExpandWidth = true;
		layoutGroup.childForceExpandHeight = false;
	}

	private CreatureCardView CreateCardInstance(int index)
	{
		GameObject instanceObject = InstantiateCardPrefab();
		if (instanceObject == null)
		{
			return null;
		}

		instanceObject.name = $"{cardPrefab.name} {index + 1}";
		instanceObject.transform.SetParent(transform, false);

		if (!instanceObject.TryGetComponent(out CreatureCardView card))
		{
			DestroyObject(instanceObject);
			return null;
		}

		return card;
	}

	private GameObject InstantiateCardPrefab()
	{
		if (cardPrefab == null)
		{
			return null;
		}

#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			Object prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(cardPrefab.gameObject);
			Object prefabObject = prefabSource != null ? prefabSource : cardPrefab.gameObject;
			return PrefabUtility.InstantiatePrefab(prefabObject, transform) as GameObject;
		}
#endif

		return Instantiate(cardPrefab.gameObject, transform);
	}

	private void RefreshBindings()
	{
		for (int index = 0; index < cardInstances.Count; index++)
		{
			CreatureCardView card = cardInstances[index];
			if (card == null)
			{
				continue;
			}

			BattleUnit unit = boundTeam != null && index < boundTeam.Count ? boundTeam[index] : null;
			card.Bind(unit);
		}
	}

	private void DestroyAllChildren()
	{
		for (int index = transform.childCount - 1; index >= 0; index--)
		{
			DestroyObject(transform.GetChild(index).gameObject);
		}
	}

	private static void DestroyObject(Object target)
	{
		if (target == null)
		{
			return;
		}

		if (Application.isPlaying)
		{
			Destroy(target);
		}
		else
		{
			DestroyImmediate(target);
		}
	}

#if UNITY_EDITOR
	private void QueueEditorRefresh()
	{
		if (editorRefreshQueued)
		{
			return;
		}

		editorRefreshQueued = true;
		EditorApplication.delayCall += ApplyEditorRefresh;
	}

	private void ApplyEditorRefresh()
	{
		editorRefreshQueued = false;

		if (this == null)
		{
			return;
		}

		EnsureLayout();
		EnsureCards();
		RefreshBindings();
	}
#endif
}
