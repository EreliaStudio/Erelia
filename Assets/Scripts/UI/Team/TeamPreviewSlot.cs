using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TeamPreviewSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image creatureImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private CanvasGroup detailGroup;
    [SerializeField] private float revealOffset = 120f;

    private Vector2 contentDefaultPosition;

    private void Awake()
    {
        if (contentRoot != null)
        {
            contentDefaultPosition = contentRoot.anchoredPosition;
        }

        SetDetailVisible(false);
    }

    public void SetData(CreatureData creature, CreatureSpeciesRegistry registry)
    {
        if (creature == null)
        {
            if (creatureImage != null)
            {
                creatureImage.enabled = false;
                creatureImage.sprite = null;
            }

            if (nameText != null)
            {
                nameText.text = string.Empty;
            }

            return;
        }

        CreatureSpecies species = null;
        if (registry != null)
        {
            registry.TryGetSpecies(creature.SpeciesId, out species);
        }

        if (creatureImage != null)
        {
            creatureImage.enabled = species != null && species.Icon != null;
            creatureImage.sprite = species != null ? species.Icon : null;
        }

        if (nameText != null)
        {
            string displayName = !string.IsNullOrWhiteSpace(creature.Nickname)
                ? creature.Nickname
                : species != null ? species.DisplayName : "Unknown";
            nameText.text = displayName;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetDetailVisible(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetDetailVisible(false);
    }

    private void SetDetailVisible(bool visible)
    {
        if (detailGroup != null)
        {
            detailGroup.alpha = visible ? 1f : 0f;
            detailGroup.blocksRaycasts = visible;
            detailGroup.interactable = visible;
        }

        if (contentRoot != null)
        {
            contentRoot.anchoredPosition = visible
                ? contentDefaultPosition + new Vector2(-revealOffset, 0f)
                : contentDefaultPosition;
        }
    }
}
