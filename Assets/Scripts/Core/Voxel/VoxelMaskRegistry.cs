using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "VoxelMaskRegistry", menuName = "Game/Voxel Mask Registry")]
public class VoxelMaskRegistry : ScriptableObject
{
	[SerializedDictionary("Mask Type", "Sprite")]
	public SerializedDictionary<MaskType, Sprite> Sprites = new SerializedDictionary<MaskType, Sprite>();

	public bool TryGetSprite(MaskType maskType, out Sprite sprite)
	{
		return Sprites.TryGetValue(maskType, out sprite);
	}
}
