using System;
using UnityEngine;

[Serializable]
public class VoxelCubeShape : VoxelShape
{
	[SerializeField] private Sprite spritePosX;
	[SerializeField] private Sprite spriteNegX;
	[SerializeField] private Sprite spritePosY;
	[SerializeField] private Sprite spriteNegY;
	[SerializeField] private Sprite spritePosZ;
	[SerializeField] private Sprite spriteNegZ;

	protected override FaceSet ConstructRenderFaces()
	{
		var faceSet = new FaceSet();
		faceSet.OuterShell.PosX = CreateFaceForPlane(spritePosX, VoxelAxisPlane.PosX);
		faceSet.OuterShell.NegX = CreateFaceForPlane(spriteNegX, VoxelAxisPlane.NegX);
		faceSet.OuterShell.PosY = CreateFaceForPlane(spritePosY, VoxelAxisPlane.PosY);
		faceSet.OuterShell.NegY = CreateFaceForPlane(spriteNegY, VoxelAxisPlane.NegY);
		faceSet.OuterShell.PosZ = CreateFaceForPlane(spritePosZ, VoxelAxisPlane.PosZ);
		faceSet.OuterShell.NegZ = CreateFaceForPlane(spriteNegZ, VoxelAxisPlane.NegZ);
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

	private static Face CreateFaceForPlane(Sprite sprite, VoxelAxisPlane plane)
	{
		GetSpriteUvRect(sprite, out Vector2 uvAnchor, out Vector2 uvSize);
		Vector2 uvA = uvAnchor;
		Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
		Vector2 uvC = uvAnchor + uvSize;
		Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);

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
