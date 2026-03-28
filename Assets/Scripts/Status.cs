using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStatus", menuName = "Game/Status")]
public class Status : ScriptableObject
{
	public StatusHookPoint HookPoint = StatusHookPoint.TurnStart;
	[SerializeReference] public List<Effect> Effects = new List<Effect>();
}
