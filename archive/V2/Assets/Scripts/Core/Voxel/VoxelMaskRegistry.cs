using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "VoxelMaskRegistry", menuName = "Game/Voxel Mask Registry")]
public class VoxelMaskRegistry : ScriptableObject
{
	public Sprite Placement = null;
	public Sprite AttackRange = null;
	public Sprite MovementRange = null;
	public Sprite AreaOfEffect = null;
	public Sprite Selected = null;

	public bool TryGetSprite(VoxelMask p_voxelMask, out Sprite p_sprite)
	{
		p_sprite = p_voxelMask switch
		{
			VoxelMask.Placement => Placement,
			VoxelMask.AttackRange => AttackRange,
			VoxelMask.MovementRange => MovementRange,
			VoxelMask.AreaOfEffect => AreaOfEffect,
			VoxelMask.Selected => Selected,
			_ => throw new System.ArgumentOutOfRangeException(nameof(p_voxelMask), p_voxelMask, null)
		};

		return true;
	}
}
