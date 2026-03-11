using TMPro;
using UnityEngine;
namespace Erelia.Battle.UI
{
	public sealed class BattleCreatureCardElement : Erelia.Core.UI.CreatureCardElement
	{
		[SerializeField] private Erelia.Core.UI.ProgressBarView staminaBar;
		[SerializeField] private TMP_Text healthValueText;
		[SerializeField] private Color staminaBarColor = new Color(0.2f, 0.72f, 1f, 1f);
		[SerializeField] private Color activeTurnColor = new Color(1f, 0.82f, 0.2f, 0.9f);
		[SerializeField] private Color knockedOutColor = new Color(0.35f, 0.18f, 0.18f, 0.95f);

		private bool isAlive;
		private bool isTurnActive;
		private int currentHealth;
		private int maxHealth;
		private float currentStaminaSeconds;

		protected override void Awake()
		{
			base.Awake();
			RefreshStaminaBar(0f, false);
			RefreshHealthValueText(false);
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			if (LinkedUnit == null)
			{
				ResetBattleState();
				return;
			}

			RefreshHealthValueText(true);
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
			isAlive = snapshot.IsAlive;
			isTurnActive = snapshot.IsTurnActive && snapshot.IsAlive;
			currentHealth = snapshot.CurrentHealth;
			maxHealth = snapshot.MaxHealth;
			currentStaminaSeconds = snapshot.CurrentStaminaSeconds;
			base.ApplySnapshot(snapshot);
			RefreshStaminaBar(snapshot.StaminaProgress01, true);
			RefreshHealthValueText(true);
		}

		protected override bool TryGetOverrideBackgroundColor(out Color color)
		{
			if (!isAlive)
			{
				color = knockedOutColor;
				return true;
			}

			if (isTurnActive)
			{
				color = activeTurnColor;
				return true;
			}

			return base.TryGetOverrideBackgroundColor(out color);
		}

		private void ResetBattleState()
		{
			isAlive = false;
			isTurnActive = false;
			currentHealth = 0;
			maxHealth = 0;
			currentStaminaSeconds = 0f;
			RefreshStaminaBar(0f, false);
			RefreshHealthValueText(false);
			RefreshBackgroundColor();
		}

		private void RefreshStaminaBar(float progress01, bool hasUnit)
		{
			bool showStaminaBar = hasUnit && isAlive;
			if (staminaBar != null && staminaBar.gameObject.activeSelf != showStaminaBar)
			{
				staminaBar.gameObject.SetActive(showStaminaBar);
			}

			if (staminaBar == null)
			{
				return;
			}

			staminaBar.SetFillColor(staminaBarColor);
			staminaBar.SetProgress(showStaminaBar ? progress01 : 0f);
			staminaBar.SetLabel(showStaminaBar
				? BuildStaminaValueLabel(currentStaminaSeconds)
				: string.Empty);
		}

		private void RefreshHealthValueText(bool hasUnit)
		{
			if (healthValueText == null)
			{
				return;
			}

			healthValueText.text = hasUnit
				? BuildHealthValueLabel(currentHealth, maxHealth, isAlive)
				: string.Empty;
		}

		private static string BuildHealthValueLabel(int currentHealth, int maxHealth, bool isAlive)
		{
			if (!isAlive)
			{
				return "Health : KO";
			}

			return $"Health : {Mathf.Max(0, currentHealth)} / {Mathf.Max(0, maxHealth)}";
		}

		private static string BuildStaminaValueLabel(float currentStaminaSeconds)
		{
			return $"{Mathf.Max(0f, currentStaminaSeconds):0.0} seconds";
		}
	}
}
