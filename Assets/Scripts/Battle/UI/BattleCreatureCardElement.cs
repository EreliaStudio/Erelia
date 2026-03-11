using UnityEngine;
using UnityEngine.UI;

namespace Erelia.Battle.UI
{
	public sealed class BattleCreatureCardElement : Erelia.Core.UI.CreatureCardElement
	{
		[SerializeField] private Image staminaTrackImage;
		[SerializeField] private Image staminaFillImage;
		[SerializeField] private Color playerStaminaColor = new Color(0.2f, 0.72f, 1f, 1f);
		[SerializeField] private Color enemyStaminaColor = new Color(1f, 0.48f, 0.2f, 1f);
		[SerializeField] private Color activeTurnColor = new Color(1f, 0.82f, 0.2f, 0.9f);

		private bool isTurnActive;
		private Erelia.Battle.Side side;
		private RectTransform staminaFillRect;
		private Vector2 baseFillOffsetMin;
		private Vector2 baseFillOffsetMax;
		private float displayedProgress01;
		private bool showStaminaBar;
		private bool hasCachedFillLayout;

		protected override void Awake()
		{
			base.Awake();
			CacheFillLayout();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			CacheFillLayout();

			if (LinkedUnit == null)
			{
				ResetBattleState();
			}
		}

		public override void LinkUnit(Erelia.Battle.Unit.Presenter presenter)
		{
			if (presenter == null)
			{
				ResetBattleState();
			}

			base.LinkUnit(presenter);
		}

		public override void ApplySnapshot(Erelia.Battle.Unit.Snapshot snapshot)
		{
			isTurnActive = snapshot.IsTurnActive;
			side = snapshot.Side;
			base.ApplySnapshot(snapshot);
			RefreshStaminaBar(snapshot.StaminaProgress01, true);
		}

		protected override bool TryGetOverrideBackgroundColor(out Color color)
		{
			if (isTurnActive)
			{
				color = activeTurnColor;
				return true;
			}

			return base.TryGetOverrideBackgroundColor(out color);
		}

		private void ResetBattleState()
		{
			isTurnActive = false;
			side = default;
			RefreshStaminaBar(0f, false);
			RefreshBackgroundColor();
		}

		private void RefreshStaminaBar(float progress01, bool hasUnit)
		{
			displayedProgress01 = Mathf.Clamp01(progress01);
			showStaminaBar = hasUnit;

			if (staminaTrackImage != null)
			{
				staminaTrackImage.type = Image.Type.Sliced;
				staminaTrackImage.enabled = hasUnit;
			}

			if (staminaFillImage == null)
			{
				return;
			}

			CacheFillLayout();
			staminaFillImage.enabled = hasUnit;
			staminaFillImage.type = Image.Type.Sliced;
			staminaFillImage.color = side == Erelia.Battle.Side.Enemy
				? enemyStaminaColor
				: playerStaminaColor;
			ApplyFillWidth();
		}

		private void OnRectTransformDimensionsChange()
		{
			ApplyFillWidth();
		}

		private void CacheFillLayout()
		{
			if (hasCachedFillLayout || staminaFillImage == null)
			{
				return;
			}

			staminaFillRect = staminaFillImage.rectTransform;
			if (staminaFillRect == null)
			{
				return;
			}

			baseFillOffsetMin = staminaFillRect.offsetMin;
			baseFillOffsetMax = staminaFillRect.offsetMax;
			hasCachedFillLayout = true;
		}

		private void ApplyFillWidth()
		{
			if (!hasCachedFillLayout || staminaFillRect == null)
			{
				return;
			}

			RectTransform parentRect = staminaFillRect.parent as RectTransform;
			if (parentRect == null)
			{
				return;
			}

			staminaFillRect.offsetMin = baseFillOffsetMin;

			float availableWidth = Mathf.Max(
				0f,
				parentRect.rect.width + baseFillOffsetMax.x - baseFillOffsetMin.x);
			float hiddenWidth = (1f - displayedProgress01) * availableWidth;

			staminaFillRect.offsetMax = new Vector2(
				baseFillOffsetMax.x - hiddenWidth,
				baseFillOffsetMax.y);
			staminaFillImage.enabled = showStaminaBar;
		}
	}
}
