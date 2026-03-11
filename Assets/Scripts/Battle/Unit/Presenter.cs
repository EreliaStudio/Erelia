using System;
using UnityEngine;

namespace Erelia.Battle.Unit
{
	public sealed class Presenter
	{
		private readonly Erelia.Battle.Unit.Model model;
		private readonly Erelia.Battle.Unit.View view;
		private Vector3 stagedWorldPosition;
		private bool hasStagedWorldPosition;
		private event Action<Erelia.Battle.Unit.Snapshot> snapshotChanged;

		public Presenter(Erelia.Battle.Unit.Model model, Transform parent, GameObject healthBarPrefab = null)
		{
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			GameObject viewObject = new GameObject("BattleUnit");
			if (parent != null)
			{
				viewObject.transform.SetParent(parent, false);
			}

			view = viewObject.AddComponent<Erelia.Battle.Unit.View>();
			RefreshVisual();
			view.SetHealthBarPrefab(healthBarPrefab);
			view.SetVisible(true);
			EmitSnapshot();
		}

		public Erelia.Battle.Unit.Model Model => model;
		public Erelia.Battle.Unit.View View => view;
		public Erelia.Core.Creature.Instance.Model Creature => model.Creature;
		public Erelia.Battle.Unit.LiveStats LiveStats => model.LiveStats;
		public Erelia.Core.Creature.Stats Stats => model.Stats;
		public Erelia.Battle.Side Side => model.Side;
		public Vector3Int Cell => model.Cell;
		public bool IsPlaced => model.IsPlaced;
		public int MaxHealth => model.MaxHealth;
		public int CurrentHealth => model.CurrentHealth;
		public bool IsAlive => model.IsAlive;
		public float CurrentStaminaSeconds => model.CurrentStaminaSeconds;
		public float StaminaProgress01 => model.StaminaProgress01;
		public bool IsTakingTurn => model.IsTakingTurn;
		public bool IsReadyForTurn => model.IsReadyForTurn;
		public Erelia.Battle.Unit.Snapshot Snapshot => CreateSnapshot();

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
			model.Place(cell);
			if (view != null)
			{
				view.SetVisible(true);
				view.SetWorldPosition(worldPosition);
			}

			EmitSnapshot();
		}

		public void Stage(Vector3 worldPosition)
		{
			stagedWorldPosition = worldPosition;
			hasStagedWorldPosition = true;

			if (view != null && !model.IsPlaced)
			{
				view.SetVisible(true);
				view.SetWorldPosition(worldPosition);
			}
		}

		public void Unplace()
		{
			if (!model.IsPlaced)
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
			if (model.IsTakingTurn)
			{
				return;
			}

			model.BeginTurn();
			EmitSnapshot();
		}

		public void EndTurn()
		{
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
			float previousCountdown = model.CurrentStaminaSeconds;
			model.ResetStamina();

			if (!Mathf.Approximately(previousCountdown, model.CurrentStaminaSeconds))
			{
				EmitSnapshot();
			}
		}

		public bool SetCurrentHealth(int value)
		{
			if (!model.SetCurrentHealth(value))
			{
				return false;
			}

			EmitSnapshot();
			return true;
		}

		public bool ChangeHealth(int delta)
		{
			if (!model.ChangeHealth(delta))
			{
				return false;
			}

			EmitSnapshot();
			return true;
		}

		public bool ApplyDamage(int amount)
		{
			if (!model.ApplyDamage(amount))
			{
				return false;
			}

			EmitSnapshot();
			return true;
		}

		public bool RestoreHealth(int amount)
		{
			if (!model.RestoreHealth(amount))
			{
				return false;
			}

			EmitSnapshot();
			return true;
		}

		public void Dispose()
		{
			snapshotChanged = null;

			if (view == null)
			{
				return;
			}

			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(view.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(view.gameObject);
			}
		}

		private void RefreshVisual()
		{
			if (view == null)
			{
				return;
			}

			view.gameObject.name = string.IsNullOrEmpty(model.DisplayName) ? "BattleUnit" : model.DisplayName;

			if (!model.TryGetSpecies(out Erelia.Core.Creature.Species species))
			{
				Debug.LogWarning("[Erelia.Battle.Unit.Presenter] Failed to resolve unit species.");
				return;
			}

			if (species.Prefab == null)
			{
				Debug.LogWarning($"[Erelia.Battle.Unit.Presenter] Species '{species.DisplayName}' has no prefab.");
				return;
			}

			view.SetVisualPrefab(species.Prefab);
			if (view.TryGetCreaturePresenter(out Erelia.Core.Creature.Instance.Presenter creaturePresenter))
			{
				creaturePresenter.SetModel(model.Creature);
			}
		}

		private Erelia.Battle.Unit.Snapshot CreateSnapshot()
		{
			Sprite icon = null;
			if (model.TryGetSpecies(out Erelia.Core.Creature.Species species))
			{
				icon = species.Icon;
			}

			return new Erelia.Battle.Unit.Snapshot(
				icon,
				model.DisplayName,
				model.IsPlaced,
				model.IsAlive,
				model.CurrentHealth,
				model.MaxHealth,
				model.CurrentStaminaSeconds,
				model.StaminaProgress01,
				model.IsTakingTurn);
		}

		private void EmitSnapshot()
		{
			Erelia.Battle.Unit.Snapshot snapshot = CreateSnapshot();
			view?.ApplySnapshot(snapshot);
			snapshotChanged?.Invoke(snapshot);
		}
	}
}
