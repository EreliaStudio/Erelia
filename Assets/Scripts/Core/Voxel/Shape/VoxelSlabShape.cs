using System;
using UnityEngine;

[Serializable]
public class VoxelSlabShape : VoxelShape
{
	private const float Height = 0.5f;

	[SerializeField] private Sprite spritePosX;
	[SerializeField] private Sprite spriteNegX;
	[SerializeField] private Sprite spritePosY;
	[SerializeField] private Sprite spriteNegY;
	[SerializeField] private Sprite spritePosZ;
	[SerializeField] private Sprite spriteNegZ;

	protected override FaceSet ConstructRenderFaces()
	{
		var faceSet = new FaceSet();
		faceSet.InnerFaces.Add(CreateTopInnerFace());
		faceSet.OuterShell.PosX = CreateHalfSideFace(spritePosX, true);
		faceSet.OuterShell.NegX = CreateHalfSideFace(spriteNegX, false);
		faceSet.OuterShell.NegY = CreateBottomFace(spriteNegY);
		faceSet.OuterShell.PosZ = CreateHalfBackFace(spritePosZ, true);
		faceSet.OuterShell.NegZ = CreateHalfBackFace(spriteNegZ, false);
		return faceSet;
	}

	protected override MaskSet ConstructMask()
	{
		const float maskOffset = 0.01f;
		var mask = new MaskSet();

		mask.PositiveYFaces.Add(CreateRectangle(
			new Vector3(0f, Height + maskOffset, 0f), new Vector2(0f, 0f),
			new Vector3(1f, Height + maskOffset, 0f), new Vector2(1f, 0f),
			new Vector3(1f, Height + maskOffset, 1f), new Vector2(1f, 1f),
			new Vector3(0f, Height + maskOffset, 1f), new Vector2(0f, 1f)));

		mask.NegativeYFaces.Add(CreateRectangle(
			new Vector3(0f, 1f + maskOffset, 0f), new Vector2(0f, 0f),
			new Vector3(1f, 1f + maskOffset, 0f), new Vector2(1f, 0f),
			new Vector3(1f, 1f + maskOffset, 1f), new Vector2(1f, 1f),
			new Vector3(0f, 1f + maskOffset, 1f), new Vector2(0f, 1f)));

		return mask;
	}

	protected override CardinalHeightSet ConstructPositiveYCardinalHeights()
	{
		return new CardinalHeightSet(
			positiveX: Height,
			negativeX: Height,
			positiveZ: Height,
			negativeZ: Height,
			stationary: Height);
	}

	protected override CardinalHeightSet ConstructNegativeYCardinalHeights()
	{
		return new CardinalHeightSet(
			positiveX: 1f,
			negativeX: 1f,
			positiveZ: 1f,
			negativeZ: 1f,
			stationary: 1f);
	}

	private Face CreateTopInnerFace()
	{
		GetSpriteUvRect(spritePosY, out Vector2 uvAnchor, out Vector2 uvSize);
		return CreateRectangle(
			new Vector3(0f, Height, 0f), uvAnchor,
			new Vector3(1f, Height, 0f), uvAnchor + new Vector2(uvSize.x, 0f),
			new Vector3(1f, Height, 1f), uvAnchor + uvSize,
			new Vector3(0f, Height, 1f), uvAnchor + new Vector2(0f, uvSize.y));
	}

	private static Face CreateHalfSideFace(Sprite sprite, bool positiveX)
	{
		GetSpriteUvRect(sprite, out Vector2 uvAnchor, out Vector2 uvSize);
		Vector2 halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
		Vector2 uvA = uvAnchor;
		Vector2 uvB = uvAnchor + new Vector2(halfUvSize.x, 0f);
		Vector2 uvC = uvAnchor + halfUvSize;
		Vector2 uvD = uvAnchor + new Vector2(0f, halfUvSize.y);

		return positiveX
			? CreateRectangle(new Vector3(1f, 0f, 0f), uvA, new Vector3(1f, 0f, 1f), uvB, new Vector3(1f, Height, 1f), uvC, new Vector3(1f, Height, 0f), uvD)
			: CreateRectangle(new Vector3(0f, 0f, 0f), uvA, new Vector3(0f, Height, 0f), uvB, new Vector3(0f, Height, 1f), uvC, new Vector3(0f, 0f, 1f), uvD);
	}

	private static Face CreateHalfBackFace(Sprite sprite, bool positiveZ)
	{
		GetSpriteUvRect(sprite, out Vector2 uvAnchor, out Vector2 uvSize);
		Vector2 halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
		Vector2 uvA = uvAnchor;
		Vector2 uvB = uvAnchor + new Vector2(halfUvSize.x, 0f);
		Vector2 uvC = uvAnchor + halfUvSize;
		Vector2 uvD = uvAnchor + new Vector2(0f, halfUvSize.y);

		return positiveZ
			? CreateRectangle(new Vector3(0f, 0f, 1f), uvA, new Vector3(0f, Height, 1f), uvB, new Vector3(1f, Height, 1f), uvC, new Vector3(1f, 0f, 1f), uvD)
			: CreateRectangle(new Vector3(0f, 0f, 0f), uvA, new Vector3(1f, 0f, 0f), uvB, new Vector3(1f, Height, 0f), uvC, new Vector3(0f, Height, 0f), uvD);
	}

	private static Face CreateBottomFace(Sprite sprite)
	{
		GetSpriteUvRect(sprite, out Vector2 uvAnchor, out Vector2 uvSize);
		return CreateRectangle(
			new Vector3(0f, 0f, 0f), uvAnchor,
			new Vector3(0f, 0f, 1f), uvAnchor + new Vector2(uvSize.x, 0f),
			new Vector3(1f, 0f, 1f), uvAnchor + uvSize,
			new Vector3(1f, 0f, 0f), uvAnchor + new Vector2(0f, uvSize.y));
	}
}
