using System;
using UnityEngine;

[Serializable]
public class VoxelCrossPlaneShape : VoxelShape
{
	[SerializeField] private Sprite sprite;

	protected override FaceSet ConstructRenderFaces()
	{
		var faceSet = new FaceSet();
		faceSet.InnerFaces.Add(CreatePlaneA());
		faceSet.InnerFaces.Add(CreatePlaneABack());
		faceSet.InnerFaces.Add(CreatePlaneB());
		faceSet.InnerFaces.Add(CreatePlaneBBack());
		return faceSet;
	}

	protected override FaceSet ConstructCollisionFaces()
	{
		var faceSet = new FaceSet();
		faceSet.OuterShell.PosX = CreateCollisionPlane(VoxelAxisPlane.PosX);
		faceSet.OuterShell.NegX = CreateCollisionPlane(VoxelAxisPlane.NegX);
		faceSet.OuterShell.PosY = CreateCollisionPlane(VoxelAxisPlane.PosY);
		faceSet.OuterShell.NegY = CreateCollisionPlane(VoxelAxisPlane.NegY);
		faceSet.OuterShell.PosZ = CreateCollisionPlane(VoxelAxisPlane.PosZ);
		faceSet.OuterShell.NegZ = CreateCollisionPlane(VoxelAxisPlane.NegZ);
		return faceSet;
	}

	protected override MaskSet ConstructMask()
	{
		const float maskOffset = 0.01f;
		var maskFace = CreateRectangle(
			new Vector3(0f, 1f + maskOffset, 0f), new Vector2(0f, 0f),
			new Vector3(1f, 1f + maskOffset, 0f), new Vector2(1f, 0f),
			new Vector3(1f, 1f + maskOffset, 1f), new Vector2(1f, 1f),
			new Vector3(0f, 1f + maskOffset, 1f), new Vector2(0f, 1f));

		var mask = new MaskSet();
		mask.PositiveYFaces.Add(maskFace);
		mask.NegativeYFaces.Add(maskFace);
		return mask;
	}

	protected override CardinalHeightSet ConstructPositiveYCardinalHeights()
	{
		return new CardinalHeightSet(
			positiveX: 0f,
			negativeX: 0f,
			positiveZ: 0f,
			negativeZ: 0f,
			stationary: 0f);
	}

	private Face CreatePlaneA()
	{
		GetSpriteUvRect(sprite, out Vector2 uvAnchor, out Vector2 uvSize);
		return CreateRectangle(
			new Vector3(0f, 0f, 0f), uvAnchor,
			new Vector3(1f, 0f, 1f), uvAnchor + new Vector2(uvSize.x, 0f),
			new Vector3(1f, 1f, 1f), uvAnchor + uvSize,
			new Vector3(0f, 1f, 0f), uvAnchor + new Vector2(0f, uvSize.y));
	}

	private Face CreatePlaneABack()
	{
		GetSpriteUvRect(sprite, out Vector2 uvAnchor, out Vector2 uvSize);
		return CreateRectangle(
			new Vector3(0f, 0f, 0f), uvAnchor,
			new Vector3(0f, 1f, 0f), uvAnchor + new Vector2(0f, uvSize.y),
			new Vector3(1f, 1f, 1f), uvAnchor + uvSize,
			new Vector3(1f, 0f, 1f), uvAnchor + new Vector2(uvSize.x, 0f));
	}

	private Face CreatePlaneB()
	{
		GetSpriteUvRect(sprite, out Vector2 uvAnchor, out Vector2 uvSize);
		return CreateRectangle(
			new Vector3(1f, 0f, 0f), uvAnchor,
			new Vector3(0f, 0f, 1f), uvAnchor + new Vector2(uvSize.x, 0f),
			new Vector3(0f, 1f, 1f), uvAnchor + uvSize,
			new Vector3(1f, 1f, 0f), uvAnchor + new Vector2(0f, uvSize.y));
	}

	private Face CreatePlaneBBack()
	{
		GetSpriteUvRect(sprite, out Vector2 uvAnchor, out Vector2 uvSize);
		return CreateRectangle(
			new Vector3(1f, 0f, 0f), uvAnchor,
			new Vector3(1f, 1f, 0f), uvAnchor + new Vector2(0f, uvSize.y),
			new Vector3(0f, 1f, 1f), uvAnchor + uvSize,
			new Vector3(0f, 0f, 1f), uvAnchor + new Vector2(uvSize.x, 0f));
	}

	private static Face CreateCollisionPlane(VoxelAxisPlane plane)
	{
		Vector2 uvA = new Vector2(0f, 0f);
		Vector2 uvB = new Vector2(1f, 0f);
		Vector2 uvC = new Vector2(1f, 1f);
		Vector2 uvD = new Vector2(0f, 1f);

		switch (plane)
		{
			case VoxelAxisPlane.PosX:
				return CreateRectangle(new Vector3(1f, 0f, 0f), uvA, new Vector3(1f, 0f, 1f), uvB, new Vector3(1f, 1f, 1f), uvC, new Vector3(1f, 1f, 0f), uvD);
			case VoxelAxisPlane.NegX:
				return CreateRectangle(new Vector3(0f, 0f, 0f), uvA, new Vector3(0f, 1f, 0f), uvB, new Vector3(0f, 1f, 1f), uvC, new Vector3(0f, 0f, 1f), uvD);
			case VoxelAxisPlane.PosY:
				return CreateRectangle(new Vector3(0f, 1f, 0f), uvA, new Vector3(1f, 1f, 0f), uvB, new Vector3(1f, 1f, 1f), uvC, new Vector3(0f, 1f, 1f), uvD);
			case VoxelAxisPlane.NegY:
				return CreateRectangle(new Vector3(0f, 0f, 0f), uvA, new Vector3(0f, 0f, 1f), uvB, new Vector3(1f, 0f, 1f), uvC, new Vector3(1f, 0f, 0f), uvD);
			case VoxelAxisPlane.PosZ:
				return CreateRectangle(new Vector3(0f, 0f, 1f), uvA, new Vector3(0f, 1f, 1f), uvB, new Vector3(1f, 1f, 1f), uvC, new Vector3(1f, 0f, 1f), uvD);
			case VoxelAxisPlane.NegZ:
			default:
				return CreateRectangle(new Vector3(0f, 0f, 0f), uvA, new Vector3(1f, 0f, 0f), uvB, new Vector3(1f, 1f, 0f), uvC, new Vector3(0f, 1f, 0f), uvD);
		}
	}
}
