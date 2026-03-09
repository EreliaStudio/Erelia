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

		[SerializeField] private float collapsedPreferredHeight = 80f;
		[SerializeField] private float expandedPreferredHeight = 160f;

		private Erelia.Core.Creature.Instance.Model linkedCreature;
		private bool isExpanded;

		public Erelia.Core.Creature.Instance.Model LinkedCreature => linkedCreature;

		protected virtual void Awake()
		{
			ApplyExpandedState(false);
		}

		public virtual void LinkCreature(Erelia.Core.Creature.Instance.Model model)
		{
			linkedCreature = model;

			if (model == null ||
				Erelia.Core.Creature.SpeciesRegistry.Instance == null ||
				Erelia.Core.Creature.SpeciesRegistry.Instance.TryGet(model.SpeciesId, out Erelia.Core.Creature.Species species) == false)
			{
				image.sprite = noCreatureSprite;
				creatureShownName.text = noCreatureName;
				return;
			}

			image.sprite = species.Icon;
			creatureShownName.text = string.IsNullOrEmpty(model.Nickname) ? species.DisplayName : model.Nickname;
		}

		protected void SetBackgroundColor(Color color)
		{
			if (backgroundImage == null)
			{
				return;
			}

			backgroundImage.color = color;
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
			if (isExpanded == value)
			{
				return;
			}

			isExpanded = value;
			ApplyExpandedState(isExpanded);
		}

		private void ApplyExpandedState(bool value)
		{
			if (layoutElement != null)
			{
				layoutElement.preferredHeight = value ? expandedPreferredHeight : collapsedPreferredHeight;
			}

			LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
			if (transform.parent is RectTransform parentRectTransform)
			{
				LayoutRebuilder.MarkLayoutForRebuild(parentRectTransform);
			}
		}
	}
}