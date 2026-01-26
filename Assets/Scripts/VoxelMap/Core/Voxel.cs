using System.Collections.Generic;
using UnityEngine;

public abstract class Voxel : ScriptableObject
{
	protected List<VoxelFace> innerFaces = new List<VoxelFace>();
	protected Dictionary<OuterShellPlane, VoxelFace> outerShellByPlane = new Dictionary<OuterShellPlane, VoxelFace>();

	public IReadOnlyList<VoxelFace> InnerFaces => innerFaces;
	public IReadOnlyDictionary<OuterShellPlane, VoxelFace> OuterShellFaces => outerShellByPlane;

	protected abstract List<VoxelFace> ConstructInnerFaces();
	protected abstract Dictionary<OuterShellPlane, VoxelFace> ConstructOuterShellFaces();

	protected virtual void OnEnable()
	{
		RebuildRuntimeFaces();
	}

	protected virtual void OnValidate()
	{
		RebuildRuntimeFaces();
	}

	protected void RebuildRuntimeFaces()
	{
		innerFaces = ConstructInnerFaces() ?? new List<VoxelFace>();
		outerShellByPlane = ConstructOuterShellFaces() ?? new Dictionary<OuterShellPlane, VoxelFace>();
	}

}
