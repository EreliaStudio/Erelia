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

		public Presenter(Erelia.Battle.Unit.Model model, Transform parent)
		{
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			GameObject viewObject = new GameObject("BattleUnit");
			if (parent != null)
			{
				viewObject.transform.SetParent(parent, false);
			}

			view = viewObject.AddComponent<Erelia.Battle.Unit.View>();
			RefreshVisual();
			view.SetVisible(true);
		}

		public Erelia.Battle.Unit.Model Model => model;
		public Erelia.Battle.Unit.View View => view;
		public Erelia.Core.Creature.Instance.Model Creature => model.Creature;
		public Erelia.Battle.Side Side => model.Side;
		public Vector3Int Cell => model.Cell;
		public bool IsPlaced => model.IsPlaced;
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
				model.Side,
				model.Cell);
		}

		private void EmitSnapshot()
		{
			snapshotChanged?.Invoke(CreateSnapshot());
		}
	}
}
