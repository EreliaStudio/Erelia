using UnityEngine;

[CreateAssetMenu(fileName = "NewVoxelDefinition", menuName = "Game/Voxel Definition")]
public class VoxelDefinition : ScriptableObject
{
	[SerializeField] private VoxelData data = new VoxelData();
	[SerializeReference] private VoxelShape shape;

	public VoxelData Data => data;
	public VoxelShape Shape => shape;

	public bool HasTag(string p_tag)
	{
		return data != null && data.HasTag(p_tag);
	}

	public void Initialize()
	{
		shape?.Initialize();
	}

	private void OnEnable()
	{
		Initialize();
	}
}
