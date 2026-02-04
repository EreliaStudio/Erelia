using UnityEngine;

public class TeamPreviewPanel : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private CreatureSpeciesRegistry speciesRegistry;
    [SerializeField] private TeamPreviewSlot[] slots;

    private TrainerData currentTrainer;

    private void OnEnable()
    {
        HookTrainer();
        Refresh();
    }

    private void OnDisable()
    {
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
        if (slots == null || slots.Length == 0)
        {
            return;
        }

        TeamData team = currentTrainer != null ? currentTrainer.Team : null;
        for (int i = 0; i < slots.Length; i++)
        {
            CreatureData creature = null;
            if (team != null)
            {
                team.TryGetCreature(i, out creature);
            }

            slots[i]?.SetData(creature, speciesRegistry);
        }
    }
}
