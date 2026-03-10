using UnityEngine;
using UnityEngine.UI;

namespace Erelia.Battle.Phase.Core
{
	/// <summary>
	/// Optional stamina-bar view that can be attached to a creature card.
	/// </summary>
	[RequireComponent(typeof(Erelia.Battle.Phase.Core.UI.CreatureCardElement))]
	public sealed class CreatureCardStaminaBarView : Erelia.Battle.Unit.View
	{
		[SerializeField] private Erelia.Battle.Phase.Core.UI.CreatureCardElement cardElement;
		[SerializeField] private bool overridePreferredHeights = true;
		[SerializeField] private float collapsedPreferredHeightOverride = 96f;
		[SerializeField] private float expandedPreferredHeightOverride = 184f;
		[SerializeField] private Color staminaBarBackgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.85f);
		[SerializeField] private Color staminaBarFillColor = new Color(0.35f, 0.85f, 0.55f, 0.95f);
		[SerializeField] private float staminaBarHeight = 8f;
		[SerializeField] private float staminaBarBottomPadding = 8f;
		[SerializeField] private float staminaBarHorizontalPadding = 8f;

		private const string BackgroundObjectName = "StaminaBarBackground";
		private const string FillObjectName = "StaminaBarFill";

		private Image staminaBarBackgroundImage;
		private Image staminaBarFillImage;

		private void Awake()
		{
			EnsureCardElement();
			ApplyPreferredHeightOverride();
			EnsureStaminaBar();
			Refresh();
		}

		private void OnEnable()
		{
			EnsureCardElement();
			EnsureStaminaBar();
			Refresh();
		}

		private void OnValidate()
		{
			EnsureCardElement();
			ApplyPreferredHeightOverride();
			EnsureStaminaBar();
			ApplyStaminaBarStyle();
			Refresh();
		}

		public override void Refresh()
		{
			EnsureStaminaBar();
			ApplyStaminaBarStyle();

			if (staminaBarFillImage == null)
			{
				return;
			}

			float fillAmount = Presenter != null
				? Presenter.StaminaProgressNormalized
				: 0f;
			staminaBarFillImage.fillAmount = fillAmount;
		}

		private void EnsureCardElement()
		{
			if (cardElement == null)
			{
				cardElement = GetComponent<Erelia.Battle.Phase.Core.UI.CreatureCardElement>();
			}
		}

		private void ApplyPreferredHeightOverride()
		{
			if (!overridePreferredHeights || cardElement == null)
			{
				return;
			}

			cardElement.SetPreferredHeights(
				collapsedPreferredHeightOverride,
				expandedPreferredHeightOverride);
		}

		private void EnsureStaminaBar()
		{
			if (staminaBarBackgroundImage == null)
			{
				Transform backgroundTransform = transform.Find(BackgroundObjectName);
				if (backgroundTransform != null)
				{
					staminaBarBackgroundImage = backgroundTransform.GetComponent<Image>();
				}
			}

			if (staminaBarFillImage == null && staminaBarBackgroundImage != null)
			{
				Transform fillTransform = staminaBarBackgroundImage.transform.Find(FillObjectName);
				if (fillTransform != null)
				{
					staminaBarFillImage = fillTransform.GetComponent<Image>();
				}
			}

			if (staminaBarBackgroundImage == null)
			{
				GameObject backgroundObject = new GameObject(
					BackgroundObjectName,
					typeof(RectTransform),
					typeof(CanvasRenderer),
					typeof(Image));
				backgroundObject.transform.SetParent(transform, false);

				RectTransform backgroundRectTransform = backgroundObject.transform as RectTransform;
				if (backgroundRectTransform != null)
				{
					backgroundRectTransform.anchorMin = new Vector2(0f, 0f);
					backgroundRectTransform.anchorMax = new Vector2(1f, 0f);
					backgroundRectTransform.offsetMin = new Vector2(staminaBarHorizontalPadding, staminaBarBottomPadding);
					backgroundRectTransform.offsetMax = new Vector2(-staminaBarHorizontalPadding, staminaBarBottomPadding + staminaBarHeight);
					backgroundRectTransform.pivot = new Vector2(0.5f, 0f);
				}

				staminaBarBackgroundImage = backgroundObject.GetComponent<Image>();
			}

			if (staminaBarFillImage != null)
			{
				return;
			}

			GameObject fillObject = new GameObject(
				FillObjectName,
				typeof(RectTransform),
				typeof(CanvasRenderer),
				typeof(Image));
			fillObject.transform.SetParent(staminaBarBackgroundImage.transform, false);

			RectTransform fillRectTransform = fillObject.transform as RectTransform;
			if (fillRectTransform != null)
			{
				fillRectTransform.anchorMin = Vector2.zero;
				fillRectTransform.anchorMax = Vector2.one;
				fillRectTransform.offsetMin = Vector2.zero;
				fillRectTransform.offsetMax = Vector2.zero;
			}

			staminaBarFillImage = fillObject.GetComponent<Image>();
			staminaBarFillImage.type = Image.Type.Filled;
			staminaBarFillImage.fillMethod = Image.FillMethod.Horizontal;
			staminaBarFillImage.fillOrigin = 0;
		}

		private void ApplyStaminaBarStyle()
		{
			if (staminaBarBackgroundImage != null)
			{
				staminaBarBackgroundImage.color = staminaBarBackgroundColor;
			}

			if (staminaBarFillImage != null)
			{
				staminaBarFillImage.color = staminaBarFillColor;
			}
		}
	}
}
