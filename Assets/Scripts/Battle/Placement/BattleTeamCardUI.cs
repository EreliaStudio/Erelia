using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleTeamCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image creatureImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text detailText;
    [SerializeField] private CanvasGroup canvasGroup;

    private RectTransform rectTransform;
    private Transform originalParent;
    private Vector3 originalPosition;
    private BattlePlacementController placementController;
    private RectTransform dragRoot;
    private int teamIndex;
    private CreatureData creature;
    private bool isPlaced;

    public void Initialize(BattlePlacementController controller, RectTransform dragRoot)
    {
        placementController = controller;
        rectTransform = transform as RectTransform;
        this.dragRoot = dragRoot;
    }

    public void SetData(int index, CreatureData creatureData, CreatureSpeciesRegistry registry)
    {
        teamIndex = index;
        creature = creatureData;

        CreatureSpecies species = null;
        if (registry != null && creature != null)
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
            string displayName = creature == null
                ? string.Empty
                : !string.IsNullOrWhiteSpace(creature.Nickname)
                    ? creature.Nickname
                    : species != null ? species.DisplayName : "Unknown";
            nameText.text = displayName;
        }

        if (detailText != null)
        {
            detailText.text = species != null ? species.DisplayName : string.Empty;
        }
    }

    public void SetPlaced(bool placed)
    {
        isPlaced = placed;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = placed ? 0.4f : 1f;
            canvasGroup.interactable = !placed;
            canvasGroup.blocksRaycasts = !placed;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced || creature == null)
        {
            return;
        }

        originalParent = transform.parent;
        originalPosition = rectTransform != null ? rectTransform.position : transform.position;
        if (dragRoot != null)
        {
            transform.SetParent(dragRoot, true);
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.7f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaced || creature == null)
        {
            return;
        }

        if (rectTransform != null)
        {
            rectTransform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlaced || creature == null)
        {
            return;
        }

        bool placed = placementController != null
            && placementController.TryPlaceCreature(teamIndex, creature, eventData.position);

        if (originalParent != null)
        {
            transform.SetParent(originalParent, true);
        }

        if (rectTransform != null)
        {
            rectTransform.position = originalPosition;
        }

        if (placed)
        {
            SetPlaced(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
    }
}
