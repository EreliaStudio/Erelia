using System.Collections.Generic;
using UnityEngine;

public sealed class BattleUnitManager
{
	private readonly Dictionary<BattleUnit, BattleUnitPresenter> presentersByUnit = new();
	private readonly Transform playerTeamRoot;
	private readonly Transform enemyTeamRoot;
	private readonly GameObject battleUnitPrefab;
	private readonly BattleContext battleContext;

	public BattleUnitManager(Transform p_playerTeamRoot, Transform p_enemyTeamRoot, GameObject p_battleUnitPrefab, BattleContext p_battleContext)
	{
		playerTeamRoot = p_playerTeamRoot;
		enemyTeamRoot = p_enemyTeamRoot;
		battleUnitPrefab = p_battleUnitPrefab;
		battleContext = p_battleContext;

		if (battleContext != null)
		{
			battleContext.UnitRegistered += OnUnitRegistered;
			battleContext.UnitRemoved += OnUnitRemoved;
		}
	}

	public void Dispose()
	{
		if (battleContext != null)
		{
			battleContext.UnitRegistered -= OnUnitRegistered;
			battleContext.UnitRemoved -= OnUnitRemoved;
		}

		foreach (KeyValuePair<BattleUnit, BattleUnitPresenter> pair in presentersByUnit)
		{
			DestroyPresenter(pair.Value);
		}

		presentersByUnit.Clear();
	}

	private void OnUnitRegistered(BattleUnit p_unit)
	{
		if (p_unit == null)
		{
			return;
		}

		GetOrCreatePresenter(p_unit);
	}

	private void OnUnitRemoved(BattleUnit p_unit)
	{
		if (p_unit == null || !presentersByUnit.TryGetValue(p_unit, out BattleUnitPresenter presenter))
		{
			return;
		}

		presentersByUnit.Remove(p_unit);
		DestroyPresenter(presenter);
	}

	private BattleUnitPresenter GetOrCreatePresenter(BattleUnit p_unit)
	{
		if (presentersByUnit.TryGetValue(p_unit, out BattleUnitPresenter existingPresenter) && existingPresenter != null)
		{
			return existingPresenter;
		}

		if (battleUnitPrefab == null)
		{
			Logger.LogError("[BattleUnitManager] Cannot create presenter: battleUnitPrefab is null.", Logger.Severity.Critical);
			return null;
		}

		Transform parent = p_unit.Side == BattleSide.Player ? playerTeamRoot : enemyTeamRoot;
		GameObject instance = Object.Instantiate(battleUnitPrefab, parent);
		instance.name = ResolveUnitName(p_unit);
		if (!instance.TryGetComponent(out BattleUnitPresenter presenter))
		{
			Object.Destroy(instance);
			return null;
		}

		presenter.Bind(p_unit, battleContext?.Board);
		presentersByUnit[p_unit] = presenter;
		return presenter;
	}

	private static string ResolveUnitName(BattleUnit p_unit)
	{
		if (p_unit.SourceUnit.TryGetForm(out CreatureForm form) && !string.IsNullOrEmpty(form.DisplayName))
		{
			return form.DisplayName;
		}

		return p_unit.SourceUnit.Species != null ? p_unit.SourceUnit.Species.name : "Unit";
	}

	private static void DestroyPresenter(BattleUnitPresenter p_presenter)
	{
		if (p_presenter == null)
		{
			return;
		}

		if (Application.isPlaying)
		{
			Object.Destroy(p_presenter.gameObject);
		}
		else
		{
			Object.DestroyImmediate(p_presenter.gameObject);
		}
	}
}
