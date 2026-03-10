using UnityEngine;

namespace Erelia.Battle.Unit
{
	public interface IView
	{
		Erelia.Battle.Unit.Presenter Presenter { get; }
		void Bind(Erelia.Battle.Unit.Presenter unitPresenter);
		void Unbind();
		void Refresh();
	}

	/// <summary>
	/// Base class for battle views bound to a unit presenter.
	/// </summary>
	public abstract class View : MonoBehaviour, IView
	{
		private Erelia.Battle.Unit.Presenter presenter;

		public Erelia.Battle.Unit.Presenter Presenter => presenter;

		public virtual void Bind(Erelia.Battle.Unit.Presenter unitPresenter)
		{
			if (ReferenceEquals(unitPresenter, presenter))
			{
				Refresh();
				return;
			}

			presenter?.UnsubscribeView(this);
			if (unitPresenter != null)
			{
				unitPresenter.SubscribeView(this);
				return;
			}

			Refresh();
		}

		public virtual void Unbind()
		{
			if (presenter != null)
			{
				presenter.UnsubscribeView(this);
			}

			Refresh();
		}

		public void BindHierarchy(Erelia.Battle.Unit.Presenter unitPresenter)
		{
			Erelia.Battle.Unit.View[] hierarchyViews = GetComponentsInChildren<Erelia.Battle.Unit.View>(true);
			for (int i = 0; i < hierarchyViews.Length; i++)
			{
				hierarchyViews[i]?.Bind(unitPresenter);
			}
		}

		public void UnbindHierarchy()
		{
			Erelia.Battle.Unit.View[] hierarchyViews = GetComponentsInChildren<Erelia.Battle.Unit.View>(true);
			for (int i = hierarchyViews.Length - 1; i >= 0; i--)
			{
				hierarchyViews[i]?.Unbind();
			}
		}

		internal virtual void SetPresenter(Erelia.Battle.Unit.Presenter unitPresenter)
		{
			presenter = unitPresenter;
		}

		public virtual void Refresh()
		{
		}

		protected virtual void OnDestroy()
		{
			presenter?.UnsubscribeView(this);
		}
	}
}
