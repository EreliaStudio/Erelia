using System.Collections.Generic;
using UnityEngine;

public abstract class Voxel : ScriptableObject
{
    [SerializeField] private VoxelCollision collision = VoxelCollision.Solid;
    protected List<VoxelFace> innerFaces = new List<VoxelFace>();
    protected Dictionary<OuterShellPlane, VoxelFace> outerShellByPlane = new Dictionary<OuterShellPlane, VoxelFace>();
    protected List<VoxelFace> maskFaces = new List<VoxelFace>();

    public IReadOnlyList<VoxelFace> InnerFaces => innerFaces;
    public IReadOnlyDictionary<OuterShellPlane, VoxelFace> OuterShellFaces => outerShellByPlane;
    public IReadOnlyList<VoxelFace> MaskFaces => maskFaces;
    public VoxelCollision Collision => collision;

	protected abstract List<VoxelFace> ConstructInnerFaces();
	protected abstract Dictionary<OuterShellPlane, VoxelFace> ConstructOuterShellFaces();
    protected abstract List<VoxelFace> ConstructMaskFaces();

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
        maskFaces = ConstructMaskFaces() ?? new List<VoxelFace>();
	}

}
