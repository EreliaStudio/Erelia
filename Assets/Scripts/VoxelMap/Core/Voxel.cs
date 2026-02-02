using System.Collections.Generic;
using UnityEngine;

public abstract class Voxel : ScriptableObject
{
    [SerializeField] private VoxelCollision collision = VoxelCollision.Solid;
    [SerializeField] private VoxelTraversal traversal = VoxelTraversal.Obstacle;
    protected List<VoxelFace> innerFaces = new List<VoxelFace>();
    protected Dictionary<OuterShellPlane, VoxelFace> outerShellByPlane = new Dictionary<OuterShellPlane, VoxelFace>();
    protected List<VoxelFace> maskFaces = new List<VoxelFace>();
    protected List<VoxelFace> flippedMaskFaces = new List<VoxelFace>();

    public IReadOnlyList<VoxelFace> InnerFaces => innerFaces;
    public IReadOnlyDictionary<OuterShellPlane, VoxelFace> OuterShellFaces => outerShellByPlane;
    public IReadOnlyList<VoxelFace> MaskFaces => maskFaces;
    public IReadOnlyList<VoxelFace> FlippedMaskFaces => flippedMaskFaces;
    public VoxelCollision Collision => collision;
    public VoxelTraversal Traversal => traversal;

    protected abstract List<VoxelFace> ConstructInnerFaces();
    protected abstract Dictionary<OuterShellPlane, VoxelFace> ConstructOuterShellFaces();
    protected abstract List<VoxelFace> ConstructMaskFaces();
    protected abstract List<VoxelFace> ConstructFlippedMaskFaces();

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
        flippedMaskFaces = ConstructFlippedMaskFaces() ?? new List<VoxelFace>();
    }

}
