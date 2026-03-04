using UnityEngine;

namespace Erelia.Battle
{
	/// <summary>
	/// Registry mapping mask voxel types to sprites used for mask rendering.
	/// Loads from Resources and provides sprite lookup for mask meshing.
	/// </summary>
	[CreateAssetMenu(menuName = "Battle/Mask Sprite Registry", fileName = "MaskSpriteRegistry")]
	public sealed class MaskSpriteRegistry : Erelia.Core.SingletonRegistry<MaskSpriteRegistry>
	{
		/// <summary>
		/// Resources path for the registry asset.
		/// </summary>
		protected override string ResourcePath => "Mask/MaskSpriteRegistry";

		/// <summary>
		/// Sprite used for player placement tiles.
		/// </summary>
		[SerializeField] private Sprite placementSprite;
		/// <summary>
		/// Sprite used for enemy placement tiles.
		/// </summary>
		[SerializeField] private Sprite enemyPlacementSprite;
		/// <summary>
		/// Sprite used for attack range tiles.
		/// </summary>
		[SerializeField] private Sprite attackRangeSprite;
		/// <summary>
		/// Sprite used for movement range tiles.
		/// </summary>
		[SerializeField] private Sprite movementRangeSprite;
		/// <summary>
		/// Sprite used for area-of-effect tiles.
		/// </summary>
		[SerializeField] private Sprite areaOfEffectSprite;
		/// <summary>
		/// Sprite used for selected tiles.
		/// </summary>
		[SerializeField] private Sprite selectedSprite;

		/// <summary>
		/// Tries to resolve a sprite for the given mask type.
		/// </summary>
		public bool TryGetSprite(Erelia.Battle.Voxel.Type type, out Sprite sprite)
		{
			// Map the mask type to its configured sprite.
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
