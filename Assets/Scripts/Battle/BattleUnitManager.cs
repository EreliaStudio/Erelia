using System.Collections.Generic;
using UnityEngine;

public sealed class BattleUnitManager
{
	private readonly Dictionary<BattleUnit, BattleUnitPresenter> presentersByUnit = new();
	private readonly Transform parentTransform;
	private readonly GameObject battleUnitPrefab;
	private readonly CreatureModelRegistry creatureModelRegistry;
	private readonly BattleContext battleContext;

	public BattleUnitManager(Transform p_parentTransform, GameObject p_battleUnitPrefab, CreatureModelRegistry p_creatureModelRegistry, BattleContext p_battleContext)
	{
		parentTransform = p_parentTransform;
		battleUnitPrefab = p_battleUnitPrefab;
		creatureModelRegistry = p_creatureModelRegistry;
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
			return null;
		}

		GameObject instance = Object.Instantiate(battleUnitPrefab, parentTransform);
		if (!instance.TryGetComponent(out BattleUnitPresenter presenter))
		{
			Object.Destroy(instance);
			return null;
		}

		presenter.Bind(p_unit, creatureModelRegistry, battleContext?.Board);
		presentersByUnit[p_unit] = presenter;
		return presenter;
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
