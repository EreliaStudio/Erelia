using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Erelia.Core.UI
{
	public class CreatureCardElement :
		MonoBehaviour,
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

		[SerializeField] private float collapsedPreferredHeight = 80f;
		[SerializeField] private float expandedPreferredHeight = 160f;

		private Erelia.Core.Creature.Instance.Model linkedCreature;
		private RectTransform rectTransform;
		private bool isExpanded;
		private bool isPlaced;

		public Erelia.Core.Creature.Instance.Model LinkedCreature => linkedCreature;
		protected bool IsPlaced => isPlaced;

		protected virtual void Awake()
		{
			rectTransform = transform as RectTransform;
			ApplyExpandedState(false);
			RefreshBackgroundColor();
		}

		public virtual void LinkCreature(Erelia.Core.Creature.Instance.Model model)
		{
			linkedCreature = model;
			model = linkedCreature;
			isPlaced = false;

			if (model == null ||
				Erelia.Core.Creature.SpeciesRegistry.Instance == null ||
				Erelia.Core.Creature.SpeciesRegistry.Instance.TryGet(model.SpeciesId, out Erelia.Core.Creature.Species species) == false)
			{
				image.sprite = noCreatureSprite;
				creatureShownName.text = noCreatureName;
				SetExpanded(false);
				RefreshBackgroundColor();
				return;
			}

			image.sprite = species.Icon;
			creatureShownName.text = string.IsNullOrEmpty(model.Nickname) ? species.DisplayName : model.Nickname;
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

			if (linkedCreature == null)
			{
				SetBackgroundColor(emptyColor);
				return;
			}

			SetBackgroundColor(idleColor);
		}

		protected virtual bool TryGetOverrideBackgroundColor(out Color color)
		{
			color = default;
			return false;
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
			if (linkedCreature == null)
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

		private void ApplyExpandedState(bool value)
		{
			float targetHeight = value ? expandedPreferredHeight : collapsedPreferredHeight;

			if (layoutElement != null)
			{
				layoutElement.preferredHeight = targetHeight;
			}

			if (rectTransform != null)
			{
				// Fallback for parents that don't drive child height from the layout system.
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
