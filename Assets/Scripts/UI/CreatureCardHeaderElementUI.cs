using UnityEngine;

public class CreatureCardHeaderElementUI : MonoBehaviour
{
	private CreatureCardHeaderNameElementUI nameElementUI;
	private CreatureCardHeaderIconElementUI iconElementUI;

	private void Awake()
	{
		ResolveElements();
	}

	public void Bind(BattleUnit p_battleUnit)
	{
		ResolveElements();
		CreatureUnit source = p_battleUnit?.SourceUnit;
		nameElementUI.Bind(source);
		iconElementUI.Bind(source);
	}

	public void Clear()
	{
		ResolveElements();
		nameElementUI.Clear();
		iconElementUI.Clear();
	}

	private void ResolveElements()
	{
		nameElementUI ??= GetComponentInChildren<CreatureCardHeaderNameElementUI>(true);
		iconElementUI ??= GetComponentInChildren<CreatureCardHeaderIconElementUI>(true);
	}
}
