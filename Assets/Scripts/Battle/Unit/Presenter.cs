using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Unit
{
	/// <summary>
	/// Runtime presenter that owns a battle unit model and refreshes subscribed views.
	/// </summary>
	public sealed class Presenter
	{
		private readonly List<Erelia.Battle.Unit.View> subscribedViews = new List<Erelia.Battle.Unit.View>();
		private readonly List<Erelia.Battle.Unit.View> reusableViews = new List<Erelia.Battle.Unit.View>();
		private readonly List<Erelia.Battle.Unit.View> releaseBuffer = new List<Erelia.Battle.Unit.View>();

		public Erelia.Battle.Unit.Model Model { get; }
		public Erelia.Battle.Unit.ObjectView ObjectView { get; }

		public Erelia.Core.Creature.Instance.Model Creature => Model?.Creature;
		public bool IsAlive => Model != null && Model.IsAlive;
		public bool IsPlaced => Model != null && Model.HasCell;
		public bool HasWorldPosition => Model != null && Model.HasWorldPosition;
		public float StaminaProgressNormalized => Model != null ? Model.StaminaProgressNormalized : 0f;

		public Presenter(Erelia.Battle.Unit.Model model, Erelia.Battle.Unit.ObjectView objectView)
		{
			Model = model ?? throw new System.ArgumentNullException(nameof(model));
			ObjectView = objectView;
			ObjectView?.BindHierarchy(this);
		}

		public void SubscribeView(Erelia.Battle.Unit.View view)
		{
			PruneMissingViews();

			if (view == null)
			{
				return;
			}

			if (!ReferenceEquals(view.Presenter, this))
			{
				view.Presenter?.UnsubscribeView(view);
			}

			if (!subscribedViews.Contains(view))
			{
				subscribedViews.Add(view);
			}

			view.SetPresenter(this);
			view.Refresh();
		}

		public void UnsubscribeView(Erelia.Battle.Unit.View view)
		{
			if (view == null)
			{
				return;
			}

			subscribedViews.Remove(view);
			if (ReferenceEquals(view.Presenter, this))
			{
				view.SetPresenter(null);
			}
		}

		public void ReleaseViews()
		{
			PruneMissingViews();
			releaseBuffer.Clear();
			releaseBuffer.AddRange(subscribedViews);

			for (int i = releaseBuffer.Count - 1; i >= 0; i--)
			{
				releaseBuffer[i]?.Unbind();
			}

			releaseBuffer.Clear();
		}

		public void Dispose()
		{
			ReleaseViews();
		}

		public bool TryGetCardDisplay(out string displayName, out Sprite icon)
		{
			displayName = null;
			icon = null;

			if (Creature == null || Creature.IsEmpty || !TryResolveSpecies(out Erelia.Core.Creature.Species species))
			{
				return false;
			}

			displayName = string.IsNullOrEmpty(Creature.Nickname)
				? species.DisplayName
				: Creature.Nickname;
			icon = species.Icon;
			return true;
		}

		public bool TryGetWorldPosition(out Vector3 worldPosition)
		{
			worldPosition = default;
			if (Model == null || !Model.HasWorldPosition)
			{
				return false;
			}

			worldPosition = Model.WorldPosition;
			return true;
		}

		public void Stage(Vector3 worldPosition)
		{
			Model.Stage(worldPosition);
			RefreshViews();
		}

		public void Place(Vector3Int cell, Vector3 worldPosition)
		{
			Model.Place(cell, worldPosition);
			RefreshViews();
		}

		public bool TickStamina(float deltaTime)
		{
			bool isReady = Model.TickStamina(deltaTime);
			RefreshViews();
			return isReady;
		}

		public void ResetTurnProgress()
		{
			Model.ResetTurnProgress();
			RefreshViews();
		}

		private bool TryResolveSpecies(out Erelia.Core.Creature.Species species)
		{
			species = null;
			Erelia.Core.Creature.SpeciesRegistry registry = Erelia.Core.Creature.SpeciesRegistry.Instance;
			return registry != null &&
				Creature != null &&
				!Creature.IsEmpty &&
				registry.TryGet(Creature.SpeciesId, out species) &&
				species != null;
		}

		private void RefreshViews()
		{
			PruneMissingViews();

			for (int i = 0; i < subscribedViews.Count; i++)
			{
				subscribedViews[i]?.Refresh();
			}
		}

		private void PruneMissingViews()
		{
			reusableViews.Clear();
			for (int i = 0; i < subscribedViews.Count; i++)
			{
				if (subscribedViews[i] != null)
				{
					reusableViews.Add(subscribedViews[i]);
				}
			}

			if (reusableViews.Count == subscribedViews.Count)
			{
				return;
			}

			subscribedViews.Clear();
			subscribedViews.AddRange(reusableViews);
		}
	}
}
