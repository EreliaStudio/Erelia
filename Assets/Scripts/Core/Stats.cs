using UnityEngine;

namespace Erelia.Core.Stats
{
	/// <summary>
	/// Serializable container holding creature stat values.
	/// </summary>
	[System.Serializable]
	public struct Values
	{
		/// <summary>
		/// Time required before the creature becomes ready to act.
		/// Lower values mean turns come back faster.
		/// </summary>
		[Min(0f)]
		public float Stamina;

		public Values(float stamina)
		{
			Stamina = Mathf.Max(0f, stamina);
		}

		public static Values operator +(Values left, Values right)
		{
			return new Values(
				Mathf.Max(0f, left.Stamina) +
				Mathf.Max(0f, right.Stamina));
		}
	}
}
