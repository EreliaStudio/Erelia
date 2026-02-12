using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mask
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
		private readonly Dictionary<Mask.Model.Value, Sprite> spriteByMask = new Dictionary<Mask.Model.Value, Sprite>();

		public Service(SpriteMapping maskMappings)
		{
			spriteByMask[Mask.Model.Value.Placement] = maskMappings.PlacementSprite;
			spriteByMask[Mask.Model.Value.AttackRange] = maskMappings.AttackRangeSprite;
			spriteByMask[Mask.Model.Value.MovementRange] = maskMappings.MovementRangeSprite;
			spriteByMask[Mask.Model.Value.AreaOfEffect] = maskMappings.AreaOfEffectSprite;
			spriteByMask[Mask.Model.Value.Selected] = maskMappings.SelectedSprite;
		}

		public bool TryGetSprite(Mask.Model.Value mask, out Sprite sprite)
		{
			if (spriteByMask.TryGetValue(mask, out sprite) == false)
			{
				return false;
			}

			return true;
		}
	}
}
