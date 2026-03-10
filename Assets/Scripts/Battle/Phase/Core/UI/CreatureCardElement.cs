using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Erelia.Battle.Phase.Core.UI
{
	public class CreatureCardElement :
		Erelia.Battle.Unit.View,
		IPointerEnterHandler,
		IPointerExitHandler
	{
		[SerializeField] private Image image;
		[SerializeField] private Sprite noCreatureSprite;

		[SerializeField] private TMP_Text creatureShownName;
		[SerializeField] private string noCreatureName;

		[SerializeField] private Image backgroundImage;
		[SerializeField] private LayoutElement layoutElement;
		[SerializeField] private Color idleColor = Color.white;
		[SerializeField] private Color placedColor = Color.gray;
		[SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.85f);

		[SerializeField] private float collapsedPreferredHeight = 50f;
		[SerializeField] private float expandedPreferredHeight = 150f;

		private RectTransform rectTransform;
		private bool isExpanded;
		private bool isPlaced;
		private bool placementEventsSubscribed;

		public Erelia.Battle.Unit.Presenter LinkedUnit => Presenter;
		public Erelia.Core.Creature.Instance.Model LinkedCreature => Presenter?.Model?.Creature;
		protected bool IsPlaced => isPlaced;

		protected virtual void Awake()
		{
			rectTransform = transform as RectTransform;
			ApplyExpandedState(false);
			RefreshBackgroundColor();
		}

		protected virtual void OnEnable()
		{
			SubscribePlacementEvents();
			RefreshBackgroundColor();
		}

		protected virtual void OnDisable()
		{
			UnsubscribePlacementEvents();
		}

		protected virtual void OnValidate()
		{
		}

		public virtual void LinkUnit(Erelia.Battle.Unit.Presenter unitPresenter)
		{
			if (unitPresenter != null)
			{
				BindHierarchy(unitPresenter);
			}
			else
			{
				UnbindHierarchy();
			}
		}

		public override void Refresh()
		{
			isPlaced = Presenter != null && Presenter.IsPlaced;

			if (Presenter == null || !Presenter.TryGetCardDisplay(out string displayName, out Sprite displayIcon))
			{
				image.sprite = noCreatureSprite;
				creatureShownName.text = noCreatureName;
				SetExpanded(false);
				SetBackgroundColor(emptyColor);
				return;
			}

			image.sprite = displayIcon;
			creatureShownName.text = displayName;
			RefreshBackgroundColor();
		}

		protected void SetBackgroundColor(Color color)
		{
			if (backgroundImage == null)
			{
				return;
			}

			backgroundImage.color = color;
		}

		protected void SetPlaced(bool value)
		{
			if (isPlaced == value)
			{
				return;
			}

			isPlaced = value;
			RefreshBackgroundColor();
		}

		protected void RefreshBackgroundColor()
		{
			if (Presenter == null)
			{
				SetBackgroundColor(emptyColor);
				return;
			}

			if (TryGetOverrideBackgroundColor(out Color overrideColor))
			{
				SetBackgroundColor(overrideColor);
				return;
			}

			if (isPlaced)
			{
				SetBackgroundColor(placedColor);
				return;
			}

			SetBackgroundColor(idleColor);
		}

		protected virtual bool TryGetOverrideBackgroundColor(out Color color)
		{
			color = default;
			return false;
		}

		protected void SubscribePlacementEvents()
		{
			if (placementEventsSubscribed)
			{
				return;
			}

			Erelia.Core.Event.Bus.Subscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced>(OnPlacementCreaturePlaced);
			Erelia.Core.Event.Bus.Subscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreatureUnplaced>(OnPlacementCreatureUnplaced);
			placementEventsSubscribed = true;
		}

		protected void UnsubscribePlacementEvents()
		{
			if (!placementEventsSubscribed)
			{
				return;
			}

			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced>(OnPlacementCreaturePlaced);
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreatureUnplaced>(OnPlacementCreatureUnplaced);
			placementEventsSubscribed = false;
		}

		private void OnPlacementCreaturePlaced(Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced evt)
		{
			HandlePlacementCreaturePlaced(evt);
		}

		private void OnPlacementCreatureUnplaced(Erelia.Battle.Phase.Placement.Event.PlacementCreatureUnplaced evt)
		{
			HandlePlacementCreatureUnplaced(evt);
		}

		protected virtual void HandlePlacementCreaturePlaced(
			Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced evt)
		{
			if (!ReferenceEquals(evt?.Unit, LinkedUnit))
			{
				return;
			}

			SetPlaced(true);
		}

		protected virtual void HandlePlacementCreatureUnplaced(
			Erelia.Battle.Phase.Placement.Event.PlacementCreatureUnplaced evt)
		{
			if (!ReferenceEquals(evt?.Unit, LinkedUnit))
			{
				return;
			}

			SetPlaced(false);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			SetExpanded(true);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			SetExpanded(false);
		}

		protected virtual void SetExpanded(bool value)
		{
			if (Presenter == null)
			{
				value = false;
			}

			if (isExpanded == value)
			{
				return;
			}

			isExpanded = value;
			ApplyExpandedState(isExpanded);
		}

		public void SetPreferredHeights(float collapsedHeight, float expandedHeight)
		{
			collapsedPreferredHeight = Mathf.Max(0f, collapsedHeight);
			expandedPreferredHeight = Mathf.Max(collapsedPreferredHeight, expandedHeight);
			ApplyExpandedState(isExpanded);
		}

		private void ApplyExpandedState(bool value)
		{
			float targetHeight = value ? expandedPreferredHeight : collapsedPreferredHeight;

			if (layoutElement != null)
			{
				layoutElement.preferredHeight = targetHeight;
			}

			if (rectTransform != null)
			{
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
				LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
			}

			if (transform.parent is RectTransform parentRectTransform)
			{
				LayoutRebuilder.MarkLayoutForRebuild(parentRectTransform);
			}
		}
	}
}
