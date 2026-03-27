using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Unit
{
	public sealed class Presenter : MonoBehaviour
	{
		[SerializeField] private Erelia.Battle.Unit.View view;
		[SerializeField] private float movementSecondsPerCell = 0.15f;

		private Erelia.Battle.Unit.BattleUnit model;
		private Vector3 stagedWorldPosition;
		private bool hasStagedWorldPosition;
		private Coroutine movementRoutine;
		private event Action<Erelia.Battle.Unit.BattleUnitSnapshot> snapshotChanged;

		public Erelia.Battle.Unit.BattleUnit Unit => model;
		public Erelia.Battle.Unit.View View => view;
		public Erelia.Core.Creature.Instance.CreatureInstance Creature => model != null ? model.Creature : null;
		public Erelia.Battle.Unit.LiveStats LiveStats => model != null ? model.LiveStats : null;
		public Erelia.Core.Creature.Stats Stats => model != null ? model.Stats : null;
		public Erelia.Battle.Side Side => model != null ? model.Side : default;
		public Vector3Int Cell => model != null ? model.Cell : default;
		public bool IsPlaced => model != null && model.IsPlaced;
		public int MaxHealth => model != null ? model.MaxHealth : 0;
		public int CurrentHealth => model != null ? model.CurrentHealth : 0;
		public bool IsAlive => model != null && model.IsAlive;
		public int ActionPoints => model != null ? model.ActionPoints : 0;
		public int RemainingActionPoints => model != null ? model.RemainingActionPoints : 0;
		public int MovementPoints => model != null ? model.MovementPoints : 0;
		public int RemainingMovementPoints => model != null ? model.RemainingMovementPoints : 0;
		public System.Collections.Generic.IReadOnlyList<Erelia.Battle.Attack> Attacks =>
			model != null ? model.Attacks : System.Array.Empty<Erelia.Battle.Attack>();
		public float CurrentStaminaSeconds => model != null ? model.CurrentStaminaSeconds : 0f;
		public float StaminaProgress01 => model != null ? model.StaminaProgress01 : 0f;
		public bool IsTakingTurn => model != null && model.IsTakingTurn;
		public bool IsReadyForTurn => model != null && model.IsReadyForTurn;
		public bool IsMoving => movementRoutine != null;
		public Erelia.Battle.Unit.BattleUnitSnapshot Snapshot => CreateSnapshot();

		private void Awake()
		{
			ResolveView();
		}

		public void SetHealthBarPrefab(GameObject healthBarPrefab)
		{
			ResolveView();
			view?.SetHealthBarPrefab(healthBarPrefab);
		}

		public void SetUnit(Erelia.Battle.Unit.BattleUnit battleUnitModel)
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

			StopMovement();
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

			StopMovement();
			model.Unplace();
			MoveViewToStage();

			EmitSnapshot();
		}

		public void MoveAlongPath(
			IReadOnlyList<Vector3Int> path,
			Erelia.Battle.Board.BattleBoardState board,
			Erelia.Battle.Board.Presenter boardPresenter,
			Action onCompleted = null)
		{
			if (model == null)
			{
				onCompleted?.Invoke();
				return;
			}

			if (path == null || path.Count == 0)
			{
				onCompleted?.Invoke();
				return;
			}

			StopMovement();

			if (!Application.isPlaying)
			{
				MoveImmediatelyAlongPath(path, board, boardPresenter);
				onCompleted?.Invoke();
				return;
			}

			movementRoutine = StartCoroutine(MoveAlongPathRoutine(path, board, boardPresenter, onCompleted));
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

		public void ResetMovementPoints()
		{
			if (model == null)
			{
				return;
			}

			int previousRemainingMovementPoints = model.RemainingMovementPoints;
			model.ResetMovementPoints();
			if (previousRemainingMovementPoints != model.RemainingMovementPoints)
			{
				EmitSnapshot();
			}
		}

		public void ResetActionPoints()
		{
			if (model == null)
			{
				return;
			}

			int previousRemainingActionPoints = model.RemainingActionPoints;
			model.ResetActionPoints();
			if (previousRemainingActionPoints != model.RemainingActionPoints)
			{
				EmitSnapshot();
			}
		}

		public bool TryConsumeMovementPoints(int amount)
		{
			if (model == null || !model.TryConsumeMovementPoints(amount))
			{
				return false;
			}

			EmitSnapshot();
			return true;
		}

		public bool TryConsumeActionPoints(int amount)
		{
			if (model == null || !model.TryConsumeActionPoints(amount))
			{
				return false;
			}

			if (amount > 0)
			{
				EmitSnapshot();
			}

			return true;
		}

		public bool ChangeRemainingActionPoints(int delta)
		{
			if (model == null || !model.ChangeRemainingActionPoints(delta))
			{
				return false;
			}

			EmitSnapshot();
			return true;
		}

		public bool ChangeRemainingMovementPoints(int delta)
		{
			if (model == null || !model.ChangeRemainingMovementPoints(delta))
			{
				return false;
			}

			EmitSnapshot();
			return true;
		}

		public bool SetCurrentHealth(int value)
		{
			if (model == null || !model.SetCurrentHealth(value))
			{
				return false;
			}

			HandleKnockoutState();
			EmitSnapshot();
			return true;
		}

		public bool ChangeHealth(int delta)
		{
			if (model == null || !model.ChangeHealth(delta))
			{
				return false;
			}

			HandleKnockoutState();
			EmitSnapshot();
			return true;
		}

		public bool ApplyDamage(int amount)
		{
			if (model == null || !model.ApplyDamage(amount))
			{
				return false;
			}

			HandleKnockoutState();
			EmitSnapshot();
			return true;
		}

		public bool RestoreHealth(int amount)
		{
			if (model == null || !model.RestoreHealth(amount))
			{
				return false;
			}

			HandleKnockoutState();
			EmitSnapshot();
			return true;
		}

		public void Dispose()
		{
			StopMovement();
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

		private Erelia.Battle.Unit.BattleUnitSnapshot CreateSnapshot()
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
				icon = model.Icon;
				displayName = model.DisplayName;
				isPlaced = model.IsPlaced;
				isAlive = model.IsAlive;
				currentHealth = model.CurrentHealth;
				maxHealth = model.MaxHealth;
				currentStaminaSeconds = model.CurrentStaminaSeconds;
				staminaProgress01 = model.StaminaProgress01;
				isTakingTurn = model.IsTakingTurn;
			}

			return new Erelia.Battle.Unit.BattleUnitSnapshot(
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
			Erelia.Battle.Unit.BattleUnitSnapshot snapshot = CreateSnapshot();
			view?.ApplySnapshot(snapshot);
			snapshotChanged?.Invoke(snapshot);
		}

		private void StopMovement()
		{
			if (movementRoutine == null)
			{
				return;
			}

			StopCoroutine(movementRoutine);
			movementRoutine = null;
		}

		private void HandleKnockoutState()
		{
			if (model == null || model.IsAlive)
			{
				return;
			}

			StopMovement();
			if (model.IsPlaced)
			{
				model.Unplace();
			}

			MoveViewToStage();
		}

		private void MoveViewToStage()
		{
			if (view == null)
			{
				return;
			}

			view.SetVisible(true);
			if (hasStagedWorldPosition)
			{
				view.SetWorldPosition(stagedWorldPosition);
			}
		}

		private void MoveImmediatelyAlongPath(
			IReadOnlyList<Vector3Int> path,
			Erelia.Battle.Board.BattleBoardState board,
			Erelia.Battle.Board.Presenter boardPresenter)
		{
			Vector3Int currentCell = model.Cell;
			for (int i = 0; i < path.Count; i++)
			{
				Vector3Int nextCell = path[i];
				if (!Erelia.Battle.Board.UnitPlacementUtility.TryResolveMovementStepWorldPositions(
						board,
						boardPresenter,
						currentCell,
						nextCell,
						out Vector3 nextEntryWorldPosition,
						out Vector3 nextStationaryWorldPosition))
				{
					continue;
				}

				view?.SetVisible(true);
				view?.SetWorldPosition(nextEntryWorldPosition);
				model.Place(nextCell);
				view?.SetVisible(true);
				view?.SetWorldPosition(nextStationaryWorldPosition);
				currentCell = nextCell;
			}

			EmitSnapshot();
		}

		private IEnumerator MoveAlongPathRoutine(
			IReadOnlyList<Vector3Int> path,
			Erelia.Battle.Board.BattleBoardState board,
			Erelia.Battle.Board.Presenter boardPresenter,
			Action onCompleted)
		{
			float secondsPerSegment = Mathf.Max(0.01f, movementSecondsPerCell * 0.5f);
			Vector3Int currentCell = model.Cell;
			try
			{
				for (int i = 0; i < path.Count; i++)
				{
					Vector3Int nextCell = path[i];
					if (!Erelia.Battle.Board.UnitPlacementUtility.TryResolveMovementStepWorldPositions(
							board,
							boardPresenter,
							currentCell,
							nextCell,
							out Vector3 nextEntryWorldPosition,
							out Vector3 nextStationaryWorldPosition))
					{
						continue;
					}

					Vector3 startWorldPosition = view != null ? view.Pivot.position : transform.position;
					yield return MoveBetweenWorldPositions(startWorldPosition, nextEntryWorldPosition, secondsPerSegment);
					yield return MoveBetweenWorldPositions(nextEntryWorldPosition, nextStationaryWorldPosition, secondsPerSegment);

					model.Place(nextCell);
					view?.SetVisible(true);
					view?.SetWorldPosition(nextStationaryWorldPosition);
					currentCell = nextCell;
				}
			}
			finally
			{
				movementRoutine = null;
				EmitSnapshot();
				onCompleted?.Invoke();
			}
		}

		private IEnumerator MoveBetweenWorldPositions(
			Vector3 startWorldPosition,
			Vector3 targetWorldPosition,
			float durationSeconds)
		{
			if (durationSeconds <= 0f || Vector3.SqrMagnitude(targetWorldPosition - startWorldPosition) <= Mathf.Epsilon)
			{
				view?.SetVisible(true);
				view?.SetWorldPosition(targetWorldPosition);
				yield break;
			}

			float elapsed = 0f;
			while (elapsed < durationSeconds)
			{
				elapsed += Time.deltaTime;
				float progress = Mathf.Clamp01(elapsed / durationSeconds);
				view?.SetVisible(true);
				view?.SetWorldPosition(Vector3.Lerp(startWorldPosition, targetWorldPosition, progress));
				yield return null;
			}
		}
	}
}


