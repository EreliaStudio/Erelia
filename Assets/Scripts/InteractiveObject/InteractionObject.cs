using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewInteractionObject", menuName = "Game/Interaction Object")]
public class InteractionObject : ScriptableObject
{
	public TargetProfile TargetProfile = TargetProfile.Everything;
	public List<string> Tags = new List<string>();
	[SerializeReference] public List<Effect> EffectsOnEnter = new List<Effect>();
	[SerializeReference] public List<Effect> EffectsOnStay = new List<Effect>();
	[SerializeReference] public List<Effect> EffectsOnLeave = new List<Effect>();
}
