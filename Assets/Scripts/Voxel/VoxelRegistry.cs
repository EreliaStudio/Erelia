using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "VoxelRegistry", menuName = "Game/Voxel Registry")]
public class VoxelRegistry : ScriptableObject
{
	[SerializedDictionary("Voxel Id", "Voxel Definition")]
	public SerializedDictionary<int, VoxelDefinition> Voxels = new SerializedDictionary<int, VoxelDefinition>();

	public bool TryGetVoxel(int id, out VoxelDefinition voxelDefinition)
	{
		return Voxels.TryGetValue(id, out voxelDefinition);
	}
}
