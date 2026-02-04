using System;

[Serializable]
public class TrainerData
{
    public TeamData Team = new TeamData();

    public event Action TeamChanged;

    [NonSerialized] private bool isHooked;

    public void Initialize()
    {
        if (Team == null)
        {
            Team = new TeamData();
        }

        if (isHooked)
        {
            return;
        }

        Team.Changed += HandleTeamChanged;
        isHooked = true;
    }

    public void ApplyTeamAsset(TeamDataAsset teamAsset)
    {
        if (teamAsset == null)
        {
            return;
        }

        if (Team == null)
        {
            Team = new TeamData();
        }

        teamAsset.ApplyTo(Team);
        TeamChanged?.Invoke();
    }

    public void NotifyTeamChanged()
    {
        TeamChanged?.Invoke();
    }

    private void HandleTeamChanged()
    {
        TeamChanged?.Invoke();
    }
}
