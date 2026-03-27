using UnityEngine;

namespace Erelia.Battle
{
	public readonly struct BattleResultEntry
	{
		public BattleResultEntry(
			Color accentColor,
			string title,
			string description,
			float progress01,
			string progressLabel)
		{
			AccentColor = accentColor;
			Title = title;
			Description = description;
			Progress01 = Mathf.Clamp01(progress01);
			ProgressLabel = progressLabel;
		}

		public Color AccentColor { get; }
		public string Title { get; }
		public string Description { get; }
		public float Progress01 { get; }
		public string ProgressLabel { get; }
	}
}

