using UnityEngine;

namespace Erelia.Core.Creature.Instance
{
	public sealed class View : MonoBehaviour
	{
		[SerializeField] private Transform pivot;

		public Transform Pivot => pivot != null ? pivot : transform;
	}
}
