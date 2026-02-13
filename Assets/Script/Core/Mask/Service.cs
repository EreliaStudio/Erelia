using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Mask
{
	[Serializable]
	public struct SpriteMapping
	{
		public Sprite PlacementSprite;
		public Sprite AttackRangeSprite;
		public Sprite MovementRangeSprite;
		public Sprite AreaOfEffectSprite;
		public Sprite SelectedSprite;
	}

	public class Service
	{
		private readonly Dictionary<Core.Mask.Model.Value, Sprite> spriteByMask = new Dictionary<Core.Mask.Model.Value, Sprite>();

		public Service(SpriteMapping maskMappings)
		{
			spriteByMask[Core.Mask.Model.Value.Placement] = maskMappings.PlacementSprite;
			spriteByMask[Core.Mask.Model.Value.AttackRange] = maskMappings.AttackRangeSprite;
			spriteByMask[Core.Mask.Model.Value.MovementRange] = maskMappings.MovementRangeSprite;
			spriteByMask[Core.Mask.Model.Value.AreaOfEffect] = maskMappings.AreaOfEffectSprite;
			spriteByMask[Core.Mask.Model.Value.Selected] = maskMappings.SelectedSprite;
		}

		public bool TryGetSprite(Core.Mask.Model.Value mask, out Sprite sprite)
		{
			if (spriteByMask.TryGetValue(mask, out sprite) == false)
			{
				return false;
			}

			return true;
		}
	}
}
