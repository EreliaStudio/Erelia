using System.Collections;
using UnityEngine;

namespace Erelia.UI
{
	public abstract class ScreenTransitionEffect : MonoBehaviour
	{
		public abstract IEnumerator PlayOn();
		public abstract IEnumerator PlayOff();
	}
}
