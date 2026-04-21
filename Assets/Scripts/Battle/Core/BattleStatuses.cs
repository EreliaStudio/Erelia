using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public sealed class BattleStatuses : IReadOnlyList<ObservableValue<BattleStatus>>
{
	private readonly ObservableList<BattleStatus> values = new();

	public BattleStatuses()
	{
		values.Changed += _ => Changed?.Invoke(this);
	}

	public event Action<BattleStatuses> Changed;

	public int Count => values.Count;

	public ObservableValue<BattleStatus> this[int p_index] => values[p_index];

	public void Add(Status p_status, int p_stackCount = 1, Duration p_duration = null, bool p_isSourcePassive = false)
	{
		if (p_status == null || p_stackCount <= 0)
		{
			return;
		}

		values.Add(new BattleStatus
		{
			Status = p_status,
			Stack = p_stackCount,
			RemainingDuration = Duration.Clone(p_duration),
			IsSourcePassive = p_isSourcePassive
		});
	}

	public int Remove(Status p_status, int p_nbStackToRemove = -1, bool p_includeSourcePassives = false)
	{
		if (p_status == null || Count == 0)
		{
			return 0;
		}

		return Remove(
			p_battleStatus => p_battleStatus.Status == p_status,
			p_nbStackToRemove,
			p_includeSourcePassives);
	}

	public int Remove(IEnumerable<string> p_tags, int p_nbStackToRemove = -1, bool p_includeSourcePassives = false)
	{
		if (p_tags == null || Count == 0)
		{
			return 0;
		}

		HashSet<string> tagSet = new HashSet<string>(p_tags);
		if (tagSet.Count == 0)
		{
			return 0;
		}

		return Remove(
			p_battleStatus => HasAnyTag(p_battleStatus.Status, tagSet),
			p_nbStackToRemove,
			p_includeSourcePassives);
	}

	public bool Contains(Status p_status, bool p_includeSourcePassives = true)
	{
		if (p_status == null || Count == 0)
		{
			return false;
		}

		for (int index = 0; index < Count; index++)
		{
			BattleStatus battleStatus = values[index]?.Value;
			if (battleStatus?.Status != p_status)
			{
				continue;
			}

			if (!p_includeSourcePassives && battleStatus.IsSourcePassive)
			{
				continue;
			}

			return true;
		}

		return false;
	}

	public bool Contains(IEnumerable<string> p_tags, bool p_includeSourcePassives = true)
	{
		if (p_tags == null || Count == 0)
		{
			return false;
		}

		HashSet<string> tagSet = new HashSet<string>(p_tags);
		if (tagSet.Count == 0)
		{
			return false;
		}

		for (int index = 0; index < Count; index++)
		{
			BattleStatus battleStatus = values[index]?.Value;
			if (battleStatus?.Status == null || !HasAnyTag(battleStatus.Status, tagSet))
			{
				continue;
			}

			if (!p_includeSourcePassives && battleStatus.IsSourcePassive)
			{
				continue;
			}

			return true;
		}

		return false;
	}

	public IEnumerator<ObservableValue<BattleStatus>> GetEnumerator()
	{
		return values.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private int Remove(
		Predicate<BattleStatus> p_match,
		int p_nbStackToRemove,
		bool p_includeSourcePassives)
	{
		if (p_match == null || p_nbStackToRemove == 0 || Count == 0)
		{
			return 0;
		}

		int stackToCleanse = p_nbStackToRemove < 0 ? int.MaxValue : p_nbStackToRemove;
		int cleansedStackCount = 0;

		for (int index = Count - 1; index >= 0 && stackToCleanse > 0; index--)
		{
			BattleStatus battleStatus = values[index]?.Value;
			if (battleStatus?.Status == null || !p_match(battleStatus))
			{
				continue;
			}

			if (!p_includeSourcePassives && battleStatus.IsSourcePassive)
			{
				continue;
			}

			int availableStackCount = Math.Max(1, battleStatus.Stack);
			int cleansedHere = Math.Min(availableStackCount, stackToCleanse);
			cleansedStackCount += cleansedHere;
			stackToCleanse -= cleansedHere;
			battleStatus.Stack = availableStackCount - cleansedHere;

			if (battleStatus.Stack <= 0)
			{
				values.RemoveAt(index);
				continue;
			}

			values.NotifyItemChangedAt(index);
		}

		return cleansedStackCount;
	}

	private static bool HasAnyTag(Status p_status, HashSet<string> p_tags)
	{
		if (p_status?.Tags == null || p_tags == null || p_tags.Count == 0)
		{
			return false;
		}

		for (int index = 0; index < p_status.Tags.Count; index++)
		{
			if (p_tags.Contains(p_status.Tags[index]))
			{
				return true;
			}
		}

		return false;
	}
}
