using System;
using System.Collections.Generic;

namespace Tests
{
	internal sealed class BattleFeatEventCapture : IDisposable
	{
		private readonly List<Entry> entries = new();

		public BattleFeatEventCapture()
		{
			EventCenter.BattleEventOccurred += OnBattleEventOccurred;
		}

		public void Dispose()
		{
			EventCenter.BattleEventOccurred -= OnBattleEventOccurred;
			entries.Clear();
		}

		public int Count(BattleUnit p_unit)
		{
			int count = 0;
			for (int index = 0; index < entries.Count; index++)
			{
				if (ReferenceEquals(entries[index].Unit, p_unit))
				{
					count++;
				}
			}

			return count;
		}

		public TEvent Find<TEvent>(BattleUnit p_unit)
			where TEvent : BattleEvent
		{
			for (int index = 0; index < entries.Count; index++)
			{
				Entry entry = entries[index];
				if (ReferenceEquals(entry.Unit, p_unit) && entry.FeatEvent is TEvent typedEvent)
				{
					return typedEvent;
				}
			}

			return null;
		}

		public IReadOnlyList<BattleEvent> GetEvents(BattleUnit p_unit)
		{
			List<BattleEvent> events = new List<BattleEvent>();
			for (int index = 0; index < entries.Count; index++)
			{
				Entry entry = entries[index];
				if (ReferenceEquals(entry.Unit, p_unit))
				{
					events.Add(entry.FeatEvent);
				}
			}

			return events;
		}

		private void OnBattleEventOccurred(BattleUnit p_unit, BattleEvent p_featEvent)
		{
			if (p_unit != null && p_featEvent != null)
			{
				entries.Add(new Entry(p_unit, p_featEvent));
			}
		}

		private readonly struct Entry
		{
			public Entry(BattleUnit p_unit, BattleEvent p_featEvent)
			{
				Unit = p_unit;
				FeatEvent = p_featEvent;
			}

			public BattleUnit Unit { get; }
			public BattleEvent FeatEvent { get; }
		}
	}
}
