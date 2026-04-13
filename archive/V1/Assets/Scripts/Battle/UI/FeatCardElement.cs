using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Erelia.Battle.UI
{
	public sealed class FeatCardElement : MonoBehaviour
	{
		[SerializeField] private Image backgroundImage;
		[SerializeField] private Image iconImage;
		[SerializeField] private TMP_Text titleText;
		[SerializeField] private TMP_Text descriptionText;
		[SerializeField] private Erelia.Core.UI.ProgressBarView progressBar;
		[SerializeField] private Sprite defaultIconSprite;

		public void Apply(Erelia.Battle.BattleResultEntry data)
		{
			if (iconImage != null)
			{
				if (defaultIconSprite != null)
				{
					iconImage.sprite = defaultIconSprite;
				}

				iconImage.color = data.AccentColor;
			}

			if (titleText != null)
			{
				titleText.text = string.IsNullOrWhiteSpace(data.Title) ? "Unknown feat" : data.Title;
			}

			if (descriptionText != null)
			{
				descriptionText.text = data.Description ?? string.Empty;
			}

			if (progressBar != null)
			{
				progressBar.SetFillColor(data.AccentColor);
				progressBar.SetProgress(data.Progress01);
				progressBar.SetLabel(data.ProgressLabel ?? string.Empty);
			}
		}
	}
}

