using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(VerticalLayoutGroup))]
[ExecuteAlways]
public sealed class CreatureTeamView : ExecuteAlwaysView
{
	[SerializeField] private CreatureCardView cardPrefab;

	private readonly List<CreatureCardView> cardInstances = new();
	private IReadOnlyList<BattleUnit> boundTeam;
	private Object lastPrefabReference;

	private void Reset()
	{
		EnsureCards();
	}

	private void Awake()
	{
		EnsureCards();
	}

	private void OnEnable()
	{
		EnsureCards();
		RefreshBindings();
	}

	private void OnValidate()
	{
#if UNITY_EDITOR
		QueueEditorRefresh();
#else
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

	public void Clear()
	{
		boundTeam = null;
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

	public static bool TryResolveSceneTeamViews(out CreatureTeamView playerTeamView, out CreatureTeamView enemyTeamView)
	{
		playerTeamView = null;
		enemyTeamView = null;

		CreatureTeamView[] views = FindObjectsByType<CreatureTeamView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		if (views == null || views.Length < 2)
		{
			return false;
		}

		for (int index = 0; index < views.Length; index++)
		{
			CreatureTeamView view = views[index];
			if (view == null)
			{
				continue;
			}

			if (playerTeamView == null || view.transform.position.x < playerTeamView.transform.position.x)
			{
				playerTeamView = view;
			}

			if (enemyTeamView == null || view.transform.position.x > enemyTeamView.transform.position.x)
			{
				enemyTeamView = view;
			}
		}

		return playerTeamView != null && enemyTeamView != null && playerTeamView != enemyTeamView;
	}

	private void EnsureCards()
	{
		if (UiViewUtility.IsPersistentAssetContext(this))
		{
			return;
		}

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

		if (cardInstances.Count != GameRule.TeamMemberCount || transform.childCount != cardInstances.Count)
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
			if (instance != null)
			{
				cardInstances.Add(instance);
			}
		}

		lastPrefabReference = cardPrefab;
		RefreshBindings();
	}

	private CreatureCardView CreateCardInstance(int index)
	{
		if (UiViewUtility.IsPersistentAssetContext(this))
		{
			return null;
		}

		GameObject instanceObject = InstantiateCardPrefab();
		if (instanceObject == null)
		{
			return null;
		}

		instanceObject.name = $"{cardPrefab.name} {index + 1}";
		if (instanceObject.transform.parent != transform)
		{
			instanceObject.transform.SetParent(transform, false);
		}

		if (!instanceObject.TryGetComponent(out CreatureCardView card))
		{
			UiViewUtility.DestroyGeneratedObject(instanceObject);
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
		if (UiViewUtility.IsPersistentAssetContext(this))
		{
			return null;
		}

		if (!Application.isPlaying)
		{
			Object prefabSource = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(cardPrefab.gameObject);
			Object prefabObject = prefabSource != null ? prefabSource : cardPrefab.gameObject;
			return UnityEditor.PrefabUtility.InstantiatePrefab(prefabObject, transform) as GameObject;
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
			UiViewUtility.DestroyGeneratedObject(transform.GetChild(index).gameObject);
		}
	}

#if UNITY_EDITOR
	protected override void OnEditorRefresh()
	{
		EnsureCards();
		RefreshBindings();
	}
#endif
}
