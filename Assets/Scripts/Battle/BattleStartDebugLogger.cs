using System.Text;
using UnityEngine;

[DisallowMultipleComponent]
public class BattleStartDebugLogger : MonoBehaviour
{
	private void OnEnable()
	{
		EventCenter.BattleStartRequested += OnBattleStartRequested;
	}

	private void OnDisable()
	{
		EventCenter.BattleStartRequested -= OnBattleStartRequested;
	}

	private void OnBattleStartRequested(BattleSetup setup)
	{
		if (setup == null)
		{
			Debug.Log("BattleStartDebugLogger: received a null battle setup.", this);
			return;
		}

		StringBuilder builder = new StringBuilder();
		builder.AppendLine("BattleStartDebugLogger: battle start requested.");

		if (setup.Board == null)
		{
			builder.AppendLine("Board: <none>");
		}
		else
		{
			builder.AppendLine($"Board: {setup.Board.Terrain.SizeX}x{setup.Board.Terrain.SizeY}x{setup.Board.Terrain.SizeZ}");
		}

		builder.AppendLine("Team:");
		EncounterUnit[] team = setup.Team;
		for (int index = 0; index < GameRule.TeamMemberCount; index++)
		{
			EncounterUnit unit = team != null && index < team.Length ? team[index] : null;
			builder.Append("  [");
			builder.Append(index + 1);
			builder.Append("] ");
			builder.AppendLine(GetUnitLabel(unit));
		}

		Debug.Log(builder.ToString(), this);
	}

	private static string GetUnitLabel(EncounterUnit unit)
	{
		if (unit == null || unit.Species == null)
		{
			return "-----";
		}

		try
		{
			CreatureForm form = unit.GetForm();
			if (form != null && !string.IsNullOrWhiteSpace(form.DisplayName))
			{
				return form.DisplayName;
			}
		}
		catch
		{
		}

		return unit.Species.name;
	}
}
