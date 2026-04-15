using System;
using System.Collections.Generic;

public sealed class BattleSetup
{
	public readonly EncounterUnit[] Team;
	public readonly BoardData Board;

	public BattleSetup(EncounterUnit[] team, BoardData board)
	{
		Team = CloneTeam(team);
		Board = board;
	}

	public static BattleSetup FromEntry(EncounterTier.Entry entry, BoardData board = null)
	{
		return new BattleSetup(entry?.Team, board);
	}

	private static EncounterUnit[] CloneTeam(EncounterUnit[] sourceTeam)
	{
		EncounterUnit[] clonedTeam = new EncounterUnit[GameRule.TeamMemberCount];
		if (sourceTeam == null)
		{
			return clonedTeam;
		}

		int count = Math.Min(sourceTeam.Length, clonedTeam.Length);
		for (int index = 0; index < count; index++)
		{
			clonedTeam[index] = CloneUnit(sourceTeam[index]);
		}

		return clonedTeam;
	}

	private static EncounterUnit CloneUnit(EncounterUnit sourceUnit)
	{
		if (sourceUnit == null)
		{
			return null;
		}

		var clonedUnit = new EncounterUnit
		{
			Species = sourceUnit.Species,
			CurrentFormID = sourceUnit.CurrentFormID,
			FeatBoardProgress = CloneFeatBoardProgress(sourceUnit.FeatBoardProgress),
			Behaviour = CloneBehaviour(sourceUnit.Behaviour)
		};

		if (clonedUnit.Species != null)
		{
			FeatProgressionService.ApplyProgress(clonedUnit);
		}
		else
		{
			clonedUnit.Attributes = new Attributes(sourceUnit.Attributes);
			clonedUnit.Abilities = sourceUnit.Abilities != null ? new List<Ability>(sourceUnit.Abilities) : new List<Ability>();
			clonedUnit.PermanentPassives = sourceUnit.PermanentPassives != null ? new List<Status>(sourceUnit.PermanentPassives) : new List<Status>();
		}

		return clonedUnit;
	}

	private static FeatBoardProgress CloneFeatBoardProgress(FeatBoardProgress sourceProgress)
	{
		var clonedProgress = new FeatBoardProgress();
		if (sourceProgress?.NodeProgress == null)
		{
			return clonedProgress;
		}

		for (int nodeIndex = 0; nodeIndex < sourceProgress.NodeProgress.Count; nodeIndex++)
		{
			FeatNodeProgress sourceNodeProgress = sourceProgress.NodeProgress[nodeIndex];
			if (sourceNodeProgress == null)
			{
				continue;
			}

			var clonedNodeProgress = new FeatNodeProgress(null)
			{
				NodeId = sourceNodeProgress.NodeId,
				CompletionCount = sourceNodeProgress.CompletionCount,
				RequirementProgress = new List<FeatRequirementProgress>()
			};

			if (sourceNodeProgress.RequirementProgress != null)
			{
				for (int requirementIndex = 0; requirementIndex < sourceNodeProgress.RequirementProgress.Count; requirementIndex++)
				{
					FeatRequirementProgress sourceRequirementProgress = sourceNodeProgress.RequirementProgress[requirementIndex];
					if (sourceRequirementProgress == null)
					{
						continue;
					}

					clonedNodeProgress.RequirementProgress.Add(new FeatRequirementProgress
					{
						Requirement = sourceRequirementProgress.Requirement,
						CurrentProgress = sourceRequirementProgress.CurrentProgress
					});
				}
			}

			clonedProgress.NodeProgress.Add(clonedNodeProgress);
		}

		return clonedProgress;
	}

	private static AIBehaviour CloneBehaviour(AIBehaviour sourceBehaviour)
	{
		var clonedBehaviour = new AIBehaviour();
		if (sourceBehaviour == null)
		{
			return clonedBehaviour;
		}

		clonedBehaviour.ActiveMode = sourceBehaviour.ActiveMode;
		if (sourceBehaviour.RulesByModes == null)
		{
			return clonedBehaviour;
		}

		foreach (var entry in sourceBehaviour.RulesByModes)
		{
			var clonedRules = new List<AIRule>();
			if (entry.Value != null)
			{
				for (int ruleIndex = 0; ruleIndex < entry.Value.Count; ruleIndex++)
				{
					AIRule sourceRule = entry.Value[ruleIndex];
					if (sourceRule == null)
					{
						continue;
					}

					clonedRules.Add(new AIRule
					{
						Conditions = CloneConditions(sourceRule.Conditions),
						Decision = CloneDecision(sourceRule.Decision)
					});
				}
			}

			clonedBehaviour.RulesByModes[entry.Key] = clonedRules;
		}

		return clonedBehaviour;
	}

	private static List<AICondition> CloneConditions(List<AICondition> sourceConditions)
	{
		var clonedConditions = new List<AICondition>();
		if (sourceConditions == null)
		{
			return clonedConditions;
		}

		for (int index = 0; index < sourceConditions.Count; index++)
		{
			AICondition sourceCondition = sourceConditions[index];
			clonedConditions.Add(sourceCondition == null ? null : (AICondition)Activator.CreateInstance(sourceCondition.GetType()));
		}

		return clonedConditions;
	}

	private static AIDecision CloneDecision(AIDecision sourceDecision)
	{
		return sourceDecision == null ? null : (AIDecision)Activator.CreateInstance(sourceDecision.GetType());
	}
}
