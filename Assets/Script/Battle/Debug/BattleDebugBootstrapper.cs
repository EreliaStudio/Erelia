using UnityEngine;

namespace Battle.Debugging
{
#if UNITY_EDITOR
	[DefaultExecutionOrder(-10000)]
	public sealed class BattleDebugBootstrapper : MonoBehaviour
	{
		private void Awake()
		{
			BattleDebugBootstrap.TryBootstrap();
		}
	}
#endif
}
