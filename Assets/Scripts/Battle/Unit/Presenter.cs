using System;
using UnityEngine;

namespace Erelia.Battle.Unit
{
	public sealed class Presenter : MonoBehaviour
	{
		[SerializeField] private Erelia.Battle.Unit.View view;

		private Erelia.Battle.Unit.Model model;
		private Vector3 stagedWorldPosition;
		private bool hasStagedWorldPosition;
		private event Action<Erelia.Battle.Unit.Snapshot> snapshotChanged;

		public Erelia.Battle.Unit.Model Model => model;
		public Erelia.Battle.Unit.Model Unit => model;
		public Erelia.Battle.Unit.View View => view;
		public Erelia.Core.Creature.Instance.Model Creature => model != null ? model.Creature : null;
		public Erelia.Battle.Unit.LiveStats LiveStats => model != null ? model.LiveStats : null;
		public Erelia.Core.Creature.Stats Stats => model != null ? model.Stats : null;
		public Erelia.Battle.Side Side => model != null ? model.Side : default;
		public Vector3Int Cell => model != null ? model.Cell : default;
		public bool IsPlaced => model != null && model.IsPlaced;
		public int MaxHealth => model != null ? model.MaxHealth : 0;
		public int CurrentHealth => model != null ? model.CurrentHealth : 0;
		public bool IsAlive => model != null && model.IsAlive;
		public float CurrentStaminaSeconds => model != null ? model.CurrentStaminaSeconds : 0f;
		public float StaminaProgress01 => model != null ? model.StaminaProgress01 : 0f;
		public bool IsTakingTurn => model != null && model.IsTakingTurn;
		public bool IsReadyForTurn => model != null && model.IsReadyForTurn;
		public Erelia.Battle.Unit.Snapshot Snapshot => CreateSnapshot();

		private void Awake()
		{
			ResolveView();
		}

		public void SetHealthBarPrefab(GameObject healthBarPrefab)
		{
			ResolveView();
			view?.SetHealthBarPrefab(healthBarPrefab);
		}

		public void SetUnit(Erelia.Battle.Unit.Model battleUnitModel)
		{
			model = battleUnitModel ?? throw new ArgumentNullException(nameof(battleUnitModel));
			ResolveView();

			if (view == null)
			{
				throw new InvalidOperationException("[Erelia.Battle.Unit.Presenter] Unit presenter requires a Battle.Unit.View.");
			}

			view.gameObject.name = string.IsNullOrEmpty(model.DisplayName) ? "BattleUnit" : model.DisplayName;
			view.SetVisible(true);
			view.SetUnit(model);

			EmitSnapshot();
		}

		public void Subscribe(Erelia.Battle.Unit.UIView uiView)
		{
			if (uiView == null)
			{
				return;
			}

			snapshotChanged -= uiView.ApplySnapshot;
			snapshotChanged += uiView.ApplySnapshot;
			uiView.ApplySnapshot(CreateSnapshot());
		}

		public void Unsubscribe(Erelia.Battle.Unit.UIView uiView)
		{
			if (uiView == null)
			{
				return;
			}

			snapshotChanged -= uiView.ApplySnapshot;
		}

		public void Place(Vector3Int cell, Vector3 worldPosition)
		{
			if (model == null)
			{
				return;
			}

			model.Place(cell);
			view?.SetVisible(true);
			view?.SetWorldPosition(worldPosition);
			EmitSnapshot();
		}

		public void Stage(Vector3 worldPosition)
		{
			stagedWorldPosition = worldPosition;
			hasStagedWorldPosition = true;

			if (view != null && !IsPlaced)
			{
				view.SetVisible(true);
				view.SetWorldPosition(worldPosition);
			}
		}

		public void Unplace()
		{
			if (model == null || !model.IsPlaced)
			{
				return;
			}

			model.Unplace();
			if (view != null)
			{
				view.SetVisible(true);
				if (hasStagedWorldPosition)
				{
					view.SetWorldPosition(stagedWorldPosition);
				}
			}

			EmitSnapshot();
		}

		public bool TickStamina(float deltaTime)
		{
			if (model == null)
			{
				return false;
			}

			float previousCountdown = model.CurrentStaminaSeconds;
			bool previousTurnState = model.IsTakingTurn;
			bool isReady = model.TickStamina(deltaTime);

			if (!Mathf.Approximately(previousCountdown, model.CurrentStaminaSeconds) ||
				previousTurnState != model.IsTakingTurn)
			{
				EmitSnapshot();
			}

			return isReady;
		}

		public void BeginTurn()
		{
			if (model == null || model.IsTakingTurn)
			{
				return;
			}

			model.BeginTurn();
			EmitSnapshot();
		}

		public void EndTurn()
		{
			if (model == null)
			{
				return;
			}

			bool previousTurnState = model.IsTakingTurn;
			float previousCountdown = model.CurrentStaminaSeconds;
			model.EndTurn();

			if (!Mathf.Approximately(previousCountdown, model.CurrentStaminaSeconds) ||
				previousTurnState != model.IsTakingTurn)
			{
				EmitSnapshot();
			}
		}

		public void ResetStamina()
		{
			if (model == null)
			{
				return;
			}

			float previousCountdown = model.CurrentStaminaSeconds;
			model.ResetStamina();

			if (!Mathf.Approximately(previousCountdown, model.CurrentStaminaSeconds))
			{
				EmitSnapshot();
			}
		}

		public bool SetCurrentHealth(int value)
		{
			if (model == null || !model.SetCurrentHealth(value))
			{
				return false;
			}

			EmitSnapshot();
			return true;
		}

		public bool ChangeHealth(int delta)
		{
			if (model == null || !model.ChangeHealth(delta))
			{
				return false;
			}

			EmitSnapshot();
			return true;
		}

		public bool ApplyDamage(int amount)
		{
			if (model == null || !model.ApplyDamage(amount))
			{
				return false;
			}

			EmitSnapshot();
			return true;
		}

		public bool RestoreHealth(int amount)
		{
			if (model == null || !model.RestoreHealth(amount))
			{
				return false;
			}

			EmitSnapshot();
			return true;
		}

		public void Dispose()
		{
			snapshotChanged = null;

			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
		}

		private void ResolveView()
		{
			if (view != null)
			{
				return;
			}

			view = GetComponent<Erelia.Battle.Unit.View>();
			if (view == null)
			{
				Debug.LogWarning("[Erelia.Battle.Unit.Presenter] Unit prefab is missing Battle.Unit.View. Adding a fallback view component.");
				view = gameObject.AddComponent<Erelia.Battle.Unit.View>();
			}
		}

		private Erelia.Battle.Unit.Snapshot CreateSnapshot()
		{
			Sprite icon = null;
			string displayName = string.Empty;
			bool isPlaced = false;
			bool isAlive = false;
			int currentHealth = 0;
			int maxHealth = 0;
			float currentStaminaSeconds = 0f;
			float staminaProgress01 = 0f;
			bool isTakingTurn = false;

			if (model != null)
			{
				if (model.TryGetSpecies(out Erelia.Core.Creature.Species species))
				{
					icon = species.Icon;
				}

				displayName = model.DisplayName;
				isPlaced = model.IsPlaced;
				isAlive = model.IsAlive;
				currentHealth = model.CurrentHealth;
				maxHealth = model.MaxHealth;
				currentStaminaSeconds = model.CurrentStaminaSeconds;
				staminaProgress01 = model.StaminaProgress01;
				isTakingTurn = model.IsTakingTurn;
			}

			return new Erelia.Battle.Unit.Snapshot(
				icon,
				displayName,
				isPlaced,
				isAlive,
				currentHealth,
				maxHealth,
				currentStaminaSeconds,
				staminaProgress01,
				isTakingTurn);
		}

		private void EmitSnapshot()
		{
			Erelia.Battle.Unit.Snapshot snapshot = CreateSnapshot();
			view?.ApplySnapshot(snapshot);
			snapshotChanged?.Invoke(snapshot);
		}
	}
}
