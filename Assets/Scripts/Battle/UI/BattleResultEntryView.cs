using TMPro;
using UnityEngine;

namespace Erelia.Battle.UI
{
	public sealed class BattleResultEntryView : MonoBehaviour
	{
		[SerializeField] private TMP_Text creatureNameText;
		[SerializeField] private TMP_Text detailLineAText;
		[SerializeField] private TMP_Text detailLineBText;

		public void Apply(string creatureName, string detailLineA, string detailLineB)
		{
			if (creatureNameText != null)
			{
				creatureNameText.text = string.IsNullOrEmpty(creatureName) ? "Unknown creature" : creatureName;
			}

			if (detailLineAText != null)
			{
				detailLineAText.text = detailLineA ?? string.Empty;
			}

			if (detailLineBText != null)
			{
				detailLineBText.text = detailLineB ?? string.Empty;
			}
		}
	}
}
