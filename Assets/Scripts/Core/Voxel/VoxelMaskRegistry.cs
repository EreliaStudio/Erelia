using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "VoxelMaskRegistry", menuName = "Game/Voxel Mask Registry")]
public class VoxelMaskRegistry : ScriptableObject
{
	[SerializedDictionary("Mask Type", "Sprite")]
	public SerializedDictionary<VoxelMask, Sprite> Sprites = new SerializedDictionary<VoxelMask, Sprite>();

	public bool TryGetSprite(VoxelMask VoxelMask, out Sprite sprite)
	{
		return Sprites.TryGetValue(VoxelMask, out sprite);
	}
}
