using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Erelia.Core.UI
{
	public class CreatureCardElement :
		MonoBehaviour,
		IPointerEnterHandler,
		IPointerExitHandler,
		Erelia.Battle.Unit.UIView
	{
		[SerializeField] private Image image;
		[SerializeField] private Sprite noCreatureSprite;

		[SerializeField] private TMP_Text creatureShownName;
		[SerializeField] private string noCreatureName;

		[SerializeField] private Image backgroundImage;
		[SerializeField] private LayoutElement layoutElement;
		[SerializeField] private GameObject expandedValuesRoot;
		[SerializeField] private TMP_Text expandedHealthValueText;
		[SerializeField] private Color idleColor = Color.white;
		[SerializeField] private Color placedColor = Color.gray;
		[SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.85f);

		[SerializeField] private float collapsedPreferredHeight = 80f;
		[SerializeField] private float expandedPreferredHeight = 160f;

		private Erelia.Battle.Unit.Presenter linkedUnit;
		private RectTransform rectTransform;
		private bool isExpanded;
		private bool isPlaced;
		private bool unitSubscribed;

		public Erelia.Battle.Unit.Presenter LinkedUnit => linkedUnit;
		protected bool IsExpanded => isExpanded;
		protected bool IsPlaced => isPlaced;
		protected event Action Expanded;
		protected event Action Reduced;

		protected virtual void Awake()
		{
			rectTransform = transform as RectTransform;
			ApplyExpandedState(false);
			SetExpandedValuesVisible(false);
			RefreshBackgroundColor();
		}

		protected virtual void OnEnable()
		{
			SubscribeToUnit();
			RefreshBoundUnit();
		}

		protected virtual void OnDisable()
		{
			UnsubscribeFromUnit();
		}

		public virtual void LinkUnit(Erelia.Battle.Unit.Presenter presenter)
		{
			if (ReferenceEquals(linkedUnit, presenter))
			{
				RefreshBoundUnit();
				return;
			}

			UnsubscribeFromUnit();
			linkedUnit = presenter;
			isPlaced = linkedUnit != null && linkedUnit.IsPlaced;
			SubscribeToUnit();
			RefreshBoundUnit();
		}

		public virtual void ApplySnapshot(Erelia.Battle.Unit.BattleUnitSnapshot snapshot)
		{
			isPlaced = snapshot.IsPlaced;
			image.sprite = snapshot.Icon != null ? snapshot.Icon : noCreatureSprite;
			creatureShownName.text = string.IsNullOrEmpty(snapshot.DisplayName) ? noCreatureName : snapshot.DisplayName;
			RefreshExpandedHealthValueText(snapshot.MaxHealth, true);
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

		protected void RefreshBackgroundColor()
		{
			if (linkedUnit == null)
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

		public void OnPointerEnter(PointerEventData eventData)
		{
			SetExpanded(true);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			SetExpanded(false);
		}

		protected void SetExpanded(bool value)
		{
			if (linkedUnit == null)
			{
				value = false;
			}

			if (isExpanded == value)
			{
				return;
			}

			isExpanded = value;
			ApplyExpandedState(isExpanded);
			SetExpandedValuesVisible(isExpanded);

			if (isExpanded)
			{
				Expanded?.Invoke();
				return;
			}

			Reduced?.Invoke();
		}

		private void SubscribeToUnit()
		{
			if (unitSubscribed || linkedUnit == null || !isActiveAndEnabled)
			{
				return;
			}

			linkedUnit.Subscribe(this);
			unitSubscribed = true;
		}

		private void UnsubscribeFromUnit()
		{
			if (!unitSubscribed || linkedUnit == null)
			{
				return;
			}

			linkedUnit.Unsubscribe(this);
			unitSubscribed = false;
		}

		private void RefreshBoundUnit()
		{
			if (linkedUnit == null)
			{
				ClearDisplay();
				return;
			}

			ApplySnapshot(linkedUnit.Snapshot);
		}

		private void ClearDisplay()
		{
			isPlaced = false;
			image.sprite = noCreatureSprite;
			creatureShownName.text = noCreatureName;
			RefreshExpandedHealthValueText(0, false);
			SetExpanded(false);
			RefreshBackgroundColor();
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

		private void SetExpandedValuesVisible(bool value)
		{
			if (expandedValuesRoot == null || expandedValuesRoot.activeSelf == value)
			{
				return;
			}

			expandedValuesRoot.SetActive(value);
		}

		private void RefreshExpandedHealthValueText(int maxHealth, bool hasUnit)
		{
			if (expandedHealthValueText == null)
			{
				return;
			}

			expandedHealthValueText.text = hasUnit
				? BuildExpandedHealthValueLabel(maxHealth)
				: string.Empty;
		}

		private static string BuildExpandedHealthValueLabel(int maxHealth)
		{
			return $"Health : {Mathf.Max(0, maxHealth)}";
		}

	}
}
