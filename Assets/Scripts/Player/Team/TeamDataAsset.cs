using UnityEngine;

[CreateAssetMenu(menuName = "Creature/TeamData")]
public class TeamDataAsset : ScriptableObject
{
    [SerializeField] private CreatureData[] creatures = new CreatureData[TeamData.MaxSize];

    public CreatureData[] Creatures => creatures;

    public TeamData CreateRuntimeTeam()
    {
        TeamData team = new TeamData();
        team.CopyFrom(creatures);
        return team;
    }

    public void ApplyTo(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.CopyFrom(creatures);
    }
}
