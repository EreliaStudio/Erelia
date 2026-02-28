using UnityEngine;

namespace Erelia.Battle
{
	[CreateAssetMenu(menuName = "Battle/Mask Sprite Registry", fileName = "MaskSpriteRegistry")]
	public sealed class MaskSpriteRegistry : Erelia.SingletonRegistry<MaskSpriteRegistry>
	{
		protected override string ResourcePath => "Mask/MaskSpriteRegistry";

		[SerializeField] private Sprite placementSprite;
		[SerializeField] private Sprite enemyPlacementSprite;
		[SerializeField] private Sprite attackRangeSprite;
		[SerializeField] private Sprite movementRangeSprite;
		[SerializeField] private Sprite areaOfEffectSprite;
		[SerializeField] private Sprite selectedSprite;

		public bool TryGetSprite(Erelia.BattleVoxel.Type type, out Sprite sprite)
		{
			sprite = type switch
			{
				Erelia.BattleVoxel.Type.Placement => placementSprite,
				Erelia.BattleVoxel.Type.EnemyPlacement => enemyPlacementSprite,
				Erelia.BattleVoxel.Type.AttackRange => attackRangeSprite,
				Erelia.BattleVoxel.Type.MovementRange => movementRangeSprite,
				Erelia.BattleVoxel.Type.AreaOfEffect => areaOfEffectSprite,
				Erelia.BattleVoxel.Type.Selected => selectedSprite,
				_ => null
			};

			return sprite != null;
		}
	}
}
