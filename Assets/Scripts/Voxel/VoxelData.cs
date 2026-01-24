using UnityEngine;

[CreateAssetMenu(menuName = "Voxel/VoxelData")]
public class VoxelData : ScriptableObject
{
	[SerializeField] public Vector2Int tileAnchor;
}
