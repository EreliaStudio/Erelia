using UnityEngine;

namespace Erelia.Battle
{
	[CreateAssetMenu(menuName = "BattleVoxel/Mask Sprite Registry", fileName = "MaskSpriteRegistry")]
	public sealed class MaskSpriteRegistry : Erelia.Core.SingletonRegistry<MaskSpriteRegistry>
	{
		protected override string ResourcePath => "Mask/MaskSpriteRegistry";

		[SerializeField] private Sprite placementSprite;
		[SerializeField] private Sprite attackRangeSprite;
		[SerializeField] private Sprite movementRangeSprite;
		[SerializeField] private Sprite areaOfEffectSprite;
		[SerializeField] private Sprite selectedSprite;

		public bool TryGetSprite(Erelia.Battle.Voxel.Mask.Type type, out Sprite sprite)
		{
			sprite = type switch
			{
				Erelia.Battle.Voxel.Mask.Type.Placement => placementSprite,
				Erelia.Battle.Voxel.Mask.Type.AttackRange => attackRangeSprite,
				Erelia.Battle.Voxel.Mask.Type.MovementRange => movementRangeSprite,
				Erelia.Battle.Voxel.Mask.Type.AreaOfEffect => areaOfEffectSprite,
				Erelia.Battle.Voxel.Mask.Type.Selected => selectedSprite,
				_ => null
			};

			return sprite != null;
		}
	}
}
