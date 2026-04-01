using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class CreatureUnit
{
	public CreatureSpecies Species = null;
	public string CurrentFormID = string.Empty;
	public Attributes Attributes = new Attributes();
	public List<Ability> Abilities = new List<Ability>();
	public List<Status> PermanentPassives = new List<Status>();
	public FeatBoardProgress FeatBoardProgress = new FeatBoardProgress();

	public void EnsureInitialized()
	{
		if (Species == null)
		{
			return;
		}

		if (FeatBoardProgress == null)
		{
			FeatBoardProgress = new FeatBoardProgress();
		}

		if (FeatBoardProgress.NodeProgress == null)
		{
			FeatBoardProgress.NodeProgress = new List<FeatNodeProgress>();
		}

		if (string.IsNullOrEmpty(CurrentFormID) || HasForm(CurrentFormID) == false)
		{
			CurrentFormID = GetDefaultFormID();
		}

		EnsureRootNodeUnlocked();
		RebuildFromProgress();
	}

	public void ResetFromSpecies()
	{
		if (Species == null)
		{
			Attributes = new Attributes();
			Abilities = new List<Ability>();
			PermanentPassives = new List<Status>();
			CurrentFormID = string.Empty;
			return;
		}

		Attributes = CloneAttributes(Species.Attributes);
		Abilities = new List<Ability>();
		PermanentPassives = new List<Status>();

		if (string.IsNullOrEmpty(CurrentFormID) || HasForm(CurrentFormID) == false)
		{
			CurrentFormID = GetDefaultFormID();
		}
	}

	public void EnsureRootNodeUnlocked()
	{
		if (Species == null || Species.FeatBoard == null || Species.FeatBoard.RootNode == null)
		{
			return;
		}

		if (FeatBoardProgress == null)
		{
			FeatBoardProgress = new FeatBoardProgress();
		}

		if (FeatBoardProgress.NodeProgress == null)
		{
			FeatBoardProgress.NodeProgress = new List<FeatNodeProgress>();
		}

		FeatNode rootNode = Species.FeatBoard.RootNode;
		FeatNodeProgress rootProgress = FeatBoardProgress.GetOrCreateProgress(rootNode);
		if (rootProgress == null)
		{
			return;
		}

		if (rootProgress.CompletionCount <= 0)
		{
			rootProgress.CompletionCount = 1;
			rootProgress.ResetRequirementProgress();
		}
	}

	public void RebuildFromProgress()
	{
		ResetFromSpecies();

		if (Species == null)
		{
			return;
		}

		if (FeatBoardProgress == null || FeatBoardProgress.NodeProgress == null)
		{
			return;
		}

		for (int progressIndex = 0; progressIndex < FeatBoardProgress.NodeProgress.Count; progressIndex++)
		{
			FeatNodeProgress nodeProgress = FeatBoardProgress.NodeProgress[progressIndex];
			if (nodeProgress == null)
			{
				continue;
			}

			FeatNode node = ResolveNode(nodeProgress.NodeId);
			if (node == null)
			{
				continue;
			}

			if (node.Rewards == null)
			{
				continue;
			}

			for (int completionIndex = 0; completionIndex < nodeProgress.CompletionCount; completionIndex++)
			{
				for (int rewardIndex = 0; rewardIndex < node.Rewards.Count; rewardIndex++)
				{
					FeatReward reward = node.Rewards[rewardIndex];
					if (reward == null)
					{
						continue;
					}

					reward.Apply(this);
				}
			}
		}
	}

	public FeatNode ResolveNode(string p_nodeId)
	{
		if (Species == null || Species.FeatBoard == null || Species.FeatBoard.Nodes == null || string.IsNullOrEmpty(p_nodeId))
		{
			return null;
		}

		for (int index = 0; index < Species.FeatBoard.Nodes.Count; index++)
		{
			FeatNode node = Species.FeatBoard.Nodes[index];
			if (node != null && node.Id == p_nodeId)
			{
				return node;
			}
		}

		return null;
	}

	public string GetDefaultFormID()
	{
		if (Species == null || Species.Forms == null || Species.Forms.Count == 0)
		{
			return string.Empty;
		}

		return Species.Forms.Keys.FirstOrDefault() ?? string.Empty;
	}

	public bool HasForm(string p_formID)
	{
		if (Species == null || Species.Forms == null || string.IsNullOrEmpty(p_formID))
		{
			return false;
		}

		return Species.Forms.ContainsKey(p_formID);
	}

	public void ClearProgress()
	{
		if (FeatBoardProgress == null)
		{
			FeatBoardProgress = new FeatBoardProgress();
		}

		FeatBoardProgress.NodeProgress = new List<FeatNodeProgress>();
		EnsureRootNodeUnlocked();
		RebuildFromProgress();
	}

	public CreatureForm GetForm()
	{
		if (Species == null)
		{
			throw new InvalidOperationException("CreatureUnit has no Species.");
		}

		if (string.IsNullOrEmpty(CurrentFormID))
		{
			throw new InvalidOperationException(
				$"CreatureUnit of species [{Species.name}] has no CurrentFormID.");
		}

		if (Species.Forms == null)
		{
			throw new InvalidOperationException(
				$"CreatureSpecies [{Species.name}] has no Forms dictionary.");
		}

		if (Species.Forms.TryGetValue(CurrentFormID, out CreatureForm form) == false)
		{
			throw new InvalidOperationException(
				$"CreatureSpecies [{Species.name}] does not contain form id [{CurrentFormID}].");
		}

		return form;
	}

	private static Attributes CloneAttributes(Attributes p_source)
	{
		if (p_source == null)
		{
			return new Attributes();
		}

		return new Attributes
		{
			Health = p_source.Health,
			ActionPoints = p_source.ActionPoints,
			Movement = p_source.Movement,
			Attack = p_source.Attack,
			Armor = p_source.Armor,
			Magic = p_source.Magic,
			Resistance = p_source.Resistance,
			BonusRange = p_source.BonusRange,
			Recovery = p_source.Recovery
		}; 
	}
}