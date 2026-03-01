using UnityEngine;

namespace Erelia.Battle
{
	[CreateAssetMenu(menuName = "Battle/Mask Sprite Registry", fileName = "MaskSpriteRegistry")]
	public sealed class MaskSpriteRegistry : Erelia.Core.SingletonRegistry<MaskSpriteRegistry>
	{
		protected override string ResourcePath => "Mask/MaskSpriteRegistry";

		[SerializeField] private Sprite placementSprite;
		[SerializeField] private Sprite enemyPlacementSprite;
		[SerializeField] private Sprite attackRangeSprite;
		[SerializeField] private Sprite movementRangeSprite;
		[SerializeField] private Sprite areaOfEffectSprite;
		[SerializeField] private Sprite selectedSprite;

		public bool TryGetSprite(Erelia.Battle.Voxel.Type type, out Sprite sprite)
		{
			sprite = type switch
			{
				Erelia.Battle.Voxel.Type.Placement => placementSprite,
				Erelia.Battle.Voxel.Type.EnemyPlacement => enemyPlacementSprite,
				Erelia.Battle.Voxel.Type.AttackRange => attackRangeSprite,
				Erelia.Battle.Voxel.Type.MovementRange => movementRangeSprite,
				Erelia.Battle.Voxel.Type.AreaOfEffect => areaOfEffectSprite,
				Erelia.Battle.Voxel.Type.Selected => selectedSprite,
				_ => null
			};

			return sprite != null;
		}
	}
}
