using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle
{
	[CreateAssetMenu(menuName = "Battle/Mask Sprite Registry", fileName = "MaskSpriteRegistry")]
	public sealed class MaskSpriteRegistry : Erelia.SingletonRegistry<MaskSpriteRegistry>
	{
		protected override string ResourcePath => "Mask/MaskSpriteRegistry";

		[Serializable]
		public struct Entry
		{
			public Erelia.BattleVoxel.Type Type;
			public Sprite Sprite;
		}

		[SerializeField] private List<Entry> entries = new List<Entry>();

		private readonly Dictionary<Erelia.BattleVoxel.Type, Sprite> sprites =
			new Dictionary<Erelia.BattleVoxel.Type, Sprite>();

		public IReadOnlyList<Entry> Entries => entries;

		protected override void Rebuild()
		{
			sprites.Clear();
			for (int i = 0; i < entries.Count; i++)
			{
				Entry entry = entries[i];
				if (entry.Sprite == null)
				{
					continue;
				}

				sprites[entry.Type] = entry.Sprite;
			}
		}

		public bool TryGetSprite(Erelia.BattleVoxel.Type type, out Sprite sprite)
		{
			return sprites.TryGetValue(type, out sprite);
		}
	}
}
