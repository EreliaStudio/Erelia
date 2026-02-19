using UnityEngine;

namespace Battle.Agent.Model
{
	[System.Serializable]
	public class Data
	{
		[SerializeReference] private PlacementPolicyBase placementPolicy = null;
		public PlacementPolicyBase PlacementPolicy => placementPolicy;
	}
}
