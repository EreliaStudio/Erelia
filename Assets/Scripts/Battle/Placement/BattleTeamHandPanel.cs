using System.Collections.Generic;
using UnityEngine;

public class BattleTeamHandPanel : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private CreatureSpeciesRegistry speciesRegistry;
    [SerializeField] private BattlePlacementController placementController;
    [SerializeField] private BattleTeamCardUI cardPrefab;
    [SerializeField] private Transform cardRoot;
    [SerializeField] private RectTransform dragRoot;

    private readonly List<BattleTeamCardUI> cards = new List<BattleTeamCardUI>();
    private TrainerData currentTrainer;

    private void OnEnable()
    {
        HookTrainer();
        if (placementController != null)
        {
            placementController.CreaturePlaced += HandleCreaturePlaced;
        }
        Refresh();
    }

    private void OnDisable()
    {
        if (placementController != null)
        {
            placementController.CreaturePlaced -= HandleCreaturePlaced;
        }
        UnhookTrainer();
    }

    private void HookTrainer()
    {
        TrainerData trainer = playerData != null ? playerData.TrainerData : null;
        if (trainer == currentTrainer)
        {
            return;
        }

        UnhookTrainer();
        currentTrainer = trainer;
        if (currentTrainer != null)
        {
            currentTrainer.TeamChanged += Refresh;
        }
    }

    private void UnhookTrainer()
    {
        if (currentTrainer != null)
        {
            currentTrainer.TeamChanged -= Refresh;
        }

        currentTrainer = null;
    }

    private void Refresh()
    {
        if (cardPrefab == null || cardRoot == null)
        {
            return;
        }

        TeamData team = currentTrainer != null ? currentTrainer.Team : null;
        EnsureCardCount(TeamData.MaxSize);

        for (int i = 0; i < cards.Count; i++)
        {
            BattleTeamCardUI card = cards[i];
            CreatureData creature = null;
            if (team != null)
            {
                team.TryGetCreature(i, out creature);
            }

            bool hasCreature = creature != null;
            card.gameObject.SetActive(hasCreature);
            if (hasCreature)
            {
                card.Initialize(placementController, dragRoot);
                card.SetData(i, creature, speciesRegistry);
                card.SetPlaced(false);
            }
        }
    }

    private void EnsureCardCount(int count)
    {
        while (cards.Count < count)
        {
            BattleTeamCardUI instance = Instantiate(cardPrefab, cardRoot);
            cards.Add(instance);
        }
    }

    private void HandleCreaturePlaced(int teamIndex, CreatureData creature, Vector3Int cell)
    {
        if (teamIndex < 0 || teamIndex >= cards.Count)
        {
            return;
        }

        cards[teamIndex]?.SetPlaced(true);
    }
}
